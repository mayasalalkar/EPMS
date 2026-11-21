using Epms.Api.Models;
using Epms.Api.Services;
using Epms.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Epms.Tests.Unit;

public class DashboardServiceTests : IAsyncLifetime
{
    private TestDbContextFactory _factory = null!;
    private DashboardService _service = null!;

    public async Task InitializeAsync()
    {
        _factory = await TestDbContextFactory.CreateAsync();
        _service = new DashboardService(_factory.CreateContext());
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetSummaryAsync_Admin_ReturnsGlobalCounts()
    {
        using var db = _factory.CreateContext();
        var projects = await db.Projects.ToListAsync();

        var result = await _service.GetSummaryAsync(null);

        Assert.Equal(await db.Employees.CountAsync(), result.TotalEmployees);
        Assert.Equal(projects.Count, result.TotalProjects);
        Assert.Equal(projects.Count(p => p.Status == ProjectStatus.Completed), result.CompletedProjects);
        Assert.Equal(projects.Count(p => p.Status == ProjectStatus.InProgress), result.InProgressProjects);
        Assert.Equal(projects.Count(p => p.Status == ProjectStatus.Pending), result.PendingProjects);
        Assert.NotEmpty(result.EmployeeAssignments);
    }

    [Fact]
    public async Task GetSummaryAsync_Employee_ReturnsScopedCounts()
    {
        using var db = _factory.CreateContext();
        var priyaId = await db.Employees.Where(e => e.Email == TestData.PriyaEmail).Select(e => e.Id).SingleAsync();
        var projects = await db.Projects.Where(p => p.Assignments.Any(a => a.EmployeeId == priyaId)).ToListAsync();

        var result = await _service.GetSummaryAsync(priyaId);

        Assert.Equal(1, result.TotalEmployees);
        Assert.Equal(projects.Count, result.TotalProjects);
        Assert.Equal(projects.Count(p => p.Status == ProjectStatus.Completed), result.CompletedProjects);
        var row = Assert.Single(result.EmployeeAssignments);
        Assert.Equal(priyaId, row.EmployeeId);
        Assert.Equal(projects.Count, row.TotalProjects);
    }

    [Fact]
    public async Task GetSummaryAsync_UpcomingDeadlines_ExcludesCompletedAndIsOrdered()
    {
        var result = await _service.GetSummaryAsync(null);

        var upcoming = result.UpcomingDeadlines.ToList();
        Assert.True(upcoming.Count <= 5);
        Assert.All(upcoming, p => Assert.NotEqual(ProjectStatus.Completed, p.Status));
        Assert.Equal(upcoming.OrderBy(p => p.Deadline).Select(p => p.Id), upcoming.Select(p => p.Id));
    }
}
