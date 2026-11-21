using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Epms.Api.Dtos;
using Epms.Api.Models;
using Epms.Tests.Helpers;

namespace Epms.Tests.Integration;

public class ProjectEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProjectEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static ProjectRequest NewRequest(params int[] employeeIds) => new()
    {
        Title = $"Integration Project {Guid.NewGuid():N}",
        Description = "Created by integration tests",
        StartDate = DateTime.UtcNow.Date,
        Deadline = DateTime.UtcNow.Date.AddDays(14),
        Status = ProjectStatus.Pending,
        Priority = ProjectPriority.High,
        ProgressPercent = 0,
        EmployeeIds = [.. employeeIds]
    };

    private async Task<int> GetEmployeeIdAsync(string email)
    {
        var auth = await _factory.LoginAsync(email, TestData.EmployeePassword);
        return auth.User.EmployeeId!.Value;
    }

    private static async Task<ProjectDto> CreateProjectAsync(HttpClient admin, params int[] employeeIds)
    {
        var response = await admin.PostAsJsonAsync("/api/projects", NewRequest(employeeIds), TestData.JsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ProjectDto>(TestData.JsonOptions))!;
    }

    [Fact]
    public async Task GetProjects_WithoutToken_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProject_AsEmployee_Returns403()
    {
        var client = await _factory.CreatePriyaClientAsync();

        var response = await client.PostAsJsonAsync("/api/projects", NewRequest(), TestData.JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateGetUpdateDelete_AsAdmin_ReturnsExpectedStatusCodes()
    {
        var admin = await _factory.CreateAdminClientAsync();
        var priyaId = await GetEmployeeIdAsync(TestData.PriyaEmail);

        var created = await CreateProjectAsync(admin, priyaId);
        Assert.Contains(created.AssignedEmployees, e => e.Id == priyaId);

        var getResponse = await admin.GetAsync($"/api/projects/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var update = NewRequest(priyaId);
        update.Title = "Updated Integration Project";
        update.Status = ProjectStatus.Completed;
        var putResponse = await admin.PutAsJsonAsync($"/api/projects/{created.Id}", update, TestData.JsonOptions);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        var updated = await putResponse.Content.ReadFromJsonAsync<ProjectDto>(TestData.JsonOptions);
        Assert.Equal("Updated Integration Project", updated!.Title);
        Assert.Equal(100, updated.ProgressPercent);

        var deleteResponse = await admin.DeleteAsync($"/api/projects/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await admin.GetAsync($"/api/projects/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }

    [Fact]
    public async Task CreateProject_DeadlineBeforeStart_Returns400WithMessage()
    {
        var admin = await _factory.CreateAdminClientAsync();
        var request = NewRequest(await GetEmployeeIdAsync(TestData.PriyaEmail));
        request.Deadline = DateTime.UtcNow.Date.AddDays(-5);

        var response = await admin.PostAsJsonAsync("/api/projects", request, TestData.JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.TryGetProperty("detail", out _));
    }

    [Fact]
    public async Task CreateProject_MissingTitle_Returns400ValidationProblem()
    {
        var admin = await _factory.CreateAdminClientAsync();
        var request = NewRequest(await GetEmployeeIdAsync(TestData.PriyaEmail));
        request.Title = "";

        var response = await admin.PostAsJsonAsync("/api/projects", request, TestData.JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("Title", out _));
    }

    [Fact]
    public async Task CreateProject_NoAssignees_Returns400()
    {
        using var admin = await _factory.CreateAdminClientAsync();
        var response = await admin.PostAsJsonAsync("/api/projects", NewRequest(), TestData.JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("EmployeeIds", out _));
    }

    [Fact]
    public async Task UpdateAssignments_AsAdmin_ReturnsUpdatedProject()
    {
        var admin = await _factory.CreateAdminClientAsync();
        var priyaId = await GetEmployeeIdAsync(TestData.PriyaEmail);
        var rahulId = await GetEmployeeIdAsync(TestData.RahulEmail);
        var created = await CreateProjectAsync(admin, priyaId);

        var response = await admin.PutAsJsonAsync($"/api/projects/{created.Id}/assignments",
            new AssignmentRequest { EmployeeIds = [priyaId, rahulId] }, TestData.JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>(TestData.JsonOptions);
        Assert.Equal(2, project!.AssignedEmployees.Count());
    }

    [Fact]
    public async Task UpdateProgress_AssignedEmployee_Returns200()
    {
        var admin = await _factory.CreateAdminClientAsync();
        var priyaId = await GetEmployeeIdAsync(TestData.PriyaEmail);
        var created = await CreateProjectAsync(admin, priyaId);
        var priya = await _factory.CreatePriyaClientAsync();

        var response = await priya.PatchAsJsonAsync($"/api/projects/{created.Id}/progress",
            new ProgressUpdateRequest { Status = ProjectStatus.InProgress, ProgressPercent = 45 }, TestData.JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>(TestData.JsonOptions);
        Assert.Equal(45, project!.ProgressPercent);
        Assert.Equal(ProjectStatus.InProgress, project.Status);
    }

    [Fact]
    public async Task UpdateProgress_UnassignedEmployee_Returns403()
    {
        var admin = await _factory.CreateAdminClientAsync();
        var priyaId = await GetEmployeeIdAsync(TestData.PriyaEmail);
        var created = await CreateProjectAsync(admin, priyaId);
        var rahul = await _factory.CreateRahulClientAsync();

        var response = await rahul.PatchAsJsonAsync($"/api/projects/{created.Id}/progress",
            new ProgressUpdateRequest { Status = ProjectStatus.InProgress, ProgressPercent = 30 }, TestData.JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetProjects_AsEmployee_ReturnsOnlyAssignedProjects()
    {
        var admin = await _factory.CreateAdminClientAsync();
        var rahulId = await GetEmployeeIdAsync(TestData.RahulEmail);
        var priyaId = await GetEmployeeIdAsync(TestData.PriyaEmail);
        var rahulOnly = await CreateProjectAsync(admin, rahulId);
        var priya = await _factory.CreatePriyaClientAsync();

        var result = await priya.GetFromJsonAsync<PagedResult<ProjectDto>>("/api/projects?pageSize=50", TestData.JsonOptions);

        Assert.NotEmpty(result!.Items);
        Assert.All(result.Items, p => Assert.Contains(p.AssignedEmployees, e => e.Id == priyaId));
        Assert.DoesNotContain(result.Items, p => p.Id == rahulOnly.Id);
    }

    [Fact]
    public async Task GetProject_UnassignedEmployee_Returns403()
    {
        var admin = await _factory.CreateAdminClientAsync();
        var rahulId = await GetEmployeeIdAsync(TestData.RahulEmail);
        var rahulOnly = await CreateProjectAsync(admin, rahulId);
        var priya = await _factory.CreatePriyaClientAsync();

        var response = await priya.GetAsync($"/api/projects/{rahulOnly.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetProjects_StatusFilter_ReturnsMatchingProjects()
    {
        var admin = await _factory.CreateAdminClientAsync();

        var result = await admin.GetFromJsonAsync<PagedResult<ProjectDto>>("/api/projects?status=Completed&pageSize=50", TestData.JsonOptions);

        Assert.All(result!.Items, p => Assert.Equal(ProjectStatus.Completed, p.Status));
    }
}
