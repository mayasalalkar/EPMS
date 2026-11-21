using System.Net;
using System.Net.Http.Json;
using Epms.Api.Dtos;
using Epms.Tests.Helpers;

namespace Epms.Tests.Integration;

public class DashboardEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DashboardEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetDashboard_WithoutToken_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/dashboard");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_AsAdmin_ReturnsGlobalSummary()
    {
        var admin = await _factory.CreateAdminClientAsync();

        var response = await admin.GetAsync("/api/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dashboard = await response.Content.ReadFromJsonAsync<DashboardDto>(TestData.JsonOptions);
        Assert.True(dashboard!.TotalEmployees >= 6);
        Assert.True(dashboard.TotalProjects >= 6);
        Assert.Equal(dashboard.TotalProjects,
            dashboard.CompletedProjects + dashboard.InProgressProjects + dashboard.PendingProjects);
        Assert.True(dashboard.UpcomingDeadlines.Count() <= 5);
    }

    [Fact]
    public async Task GetDashboard_AsEmployee_ReturnsScopedSummary()
    {
        var auth = await _factory.LoginAsync(TestData.PriyaEmail, TestData.EmployeePassword);
        var priya = await _factory.CreatePriyaClientAsync();

        var dashboard = await priya.GetFromJsonAsync<DashboardDto>("/api/dashboard", TestData.JsonOptions);

        Assert.Equal(1, dashboard!.TotalEmployees);
        var row = Assert.Single(dashboard.EmployeeAssignments);
        Assert.Equal(auth.User.EmployeeId!.Value, row.EmployeeId);
        Assert.Equal(dashboard.TotalProjects, row.TotalProjects);
    }
}
