using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Epms.Api.Dtos;
using Epms.Tests.Helpers;

namespace Epms.Tests.Integration;

public class EmployeeEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EmployeeEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static EmployeeRequest NewRequest(string? email = null) => new()
    {
        Name = "Kavya Iyer",
        Email = email ?? TestData.UniqueEmail("kavya"),
        Phone = "9988776655",
        Department = "Engineering",
        Designation = "Developer"
    };

    private static async Task<EmployeeDto> CreateEmployeeAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/employees", NewRequest(), TestData.JsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<EmployeeDto>(TestData.JsonOptions))!;
    }

    [Fact]
    public async Task GetEmployees_WithoutToken_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/employees");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetEmployees_AsEmployee_Returns403()
    {
        var client = await _factory.CreatePriyaClientAsync();

        var response = await client.GetAsync("/api/employees");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateEmployee_AsEmployee_Returns403()
    {
        var client = await _factory.CreatePriyaClientAsync();

        var response = await client.PostAsJsonAsync("/api/employees", NewRequest(), TestData.JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetEmployees_AsAdmin_ReturnsPagedResult()
    {
        var client = await _factory.CreateAdminClientAsync();

        var response = await client.GetAsync("/api/employees?page=1&pageSize=3&sortBy=name&sortDir=asc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EmployeeDto>>(TestData.JsonOptions);
        Assert.Equal(3, result!.Items.Count());
        Assert.True(result.TotalCount >= 6);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task GetEmployees_SearchQuery_FiltersResults()
    {
        var client = await _factory.CreateAdminClientAsync();

        var result = await client.GetFromJsonAsync<PagedResult<EmployeeDto>>("/api/employees?search=rahul", TestData.JsonOptions);

        var item = Assert.Single(result!.Items);
        Assert.Equal(TestData.RahulEmail, item.Email);
    }

    [Fact]
    public async Task CreateGetUpdateDelete_AsAdmin_ReturnsExpectedStatusCodes()
    {
        var client = await _factory.CreateAdminClientAsync();

        var created = await CreateEmployeeAsync(client);

        var getResponse = await client.GetAsync($"/api/employees/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var update = NewRequest(created.Email);
        update.Designation = "Senior Developer";
        var putResponse = await client.PutAsJsonAsync($"/api/employees/{created.Id}", update, TestData.JsonOptions);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        var updated = await putResponse.Content.ReadFromJsonAsync<EmployeeDto>(TestData.JsonOptions);
        Assert.Equal("Senior Developer", updated!.Designation);

        var deleteResponse = await client.DeleteAsync($"/api/employees/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await client.GetAsync($"/api/employees/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }

    [Fact]
    public async Task CreateEmployee_DuplicateEmail_Returns409()
    {
        var client = await _factory.CreateAdminClientAsync();

        var response = await client.PostAsJsonAsync("/api/employees", NewRequest(TestData.PriyaEmail), TestData.JsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(body.RootElement.GetProperty("detail").GetString()));
    }

    [Fact]
    public async Task CreateEmployee_InvalidPayload_Returns400ValidationProblem()
    {
        var client = await _factory.CreateAdminClientAsync();
        var request = new EmployeeRequest { Name = "", Email = "not-an-email", Phone = "", Department = "", Designation = "" };

        var response = await client.PostAsJsonAsync("/api/employees", request, TestData.JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = body.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Email", out _) || errors.TryGetProperty("email", out _));
    }

    [Fact]
    public async Task GetEmployee_UnknownId_Returns404()
    {
        var client = await _factory.CreateAdminClientAsync();

        var response = await client.GetAsync("/api/employees/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DepartmentsAndLookup_AsAdmin_ReturnData()
    {
        var client = await _factory.CreateAdminClientAsync();

        var departments = await client.GetFromJsonAsync<List<string>>("/api/employees/departments", TestData.JsonOptions);
        var lookup = await client.GetFromJsonAsync<List<DirectoryLookupDto>>("/api/employees/lookup", TestData.JsonOptions);

        Assert.Contains("Engineering", departments!);
        Assert.Contains(lookup!, e => e.Email == TestData.PriyaEmail);
    }
}
