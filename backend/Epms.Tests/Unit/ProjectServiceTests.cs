using Epms.Api.Dtos;
using Epms.Api.Exceptions;
using Epms.Api.Models;
using Epms.Api.Services;
using Epms.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Epms.Tests.Unit;

public class ProjectServiceTests : IAsyncLifetime
{
    private TestDbContextFactory _factory = null!;
    private ProjectService _service = null!;
    private int _priyaId;
    private int _rahulId;

    public async Task InitializeAsync()
    {
        _factory = await TestDbContextFactory.CreateAsync();
        _service = new ProjectService(_factory.CreateContext());
        using var db = _factory.CreateContext();
        _priyaId = await db.Employees.Where(e => e.Email == TestData.PriyaEmail).Select(e => e.Id).SingleAsync();
        _rahulId = await db.Employees.Where(e => e.Email == TestData.RahulEmail).Select(e => e.Id).SingleAsync();
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    private static ProjectRequest NewRequest(params int[] employeeIds) => new()
    {
        Title = $"Project {Guid.NewGuid():N}",
        Description = "Test project",
        StartDate = DateTime.UtcNow.Date,
        Deadline = DateTime.UtcNow.Date.AddDays(30),
        Status = ProjectStatus.InProgress,
        Priority = ProjectPriority.Medium,
        ProgressPercent = 20,
        EmployeeIds = [.. employeeIds]
    };

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesProjectWithAssignments()
    {
        var result = await _service.CreateAsync(NewRequest(_priyaId, _rahulId));

        Assert.True(result.Id > 0);
        Assert.Equal(20, result.ProgressPercent);
        Assert.Equal(new[] { _priyaId, _rahulId }.OrderBy(i => i), result.AssignedEmployees.Select(e => e.Id).OrderBy(i => i));
    }

    [Fact]
    public async Task CreateAsync_DeadlineBeforeStartDate_ThrowsBadRequest()
    {
        var request = NewRequest();
        request.Deadline = DateTime.UtcNow.Date.AddDays(-1);

        await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_UnknownEmployeeIds_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateAsync(NewRequest(_priyaId, 99999)));
    }

    [Fact]
    public async Task CreateAsync_StatusCompleted_ForcesFullProgress()
    {
        var request = NewRequest(_priyaId);
        request.Status = ProjectStatus.Completed;
        request.ProgressPercent = 40;

        var result = await _service.CreateAsync(request);

        Assert.Equal(ProjectStatus.Completed, result.Status);
        Assert.Equal(100, result.ProgressPercent);
    }

    [Fact]
    public async Task UpdateAsync_StatusCompleted_ForcesFullProgress()
    {
        var created = await _service.CreateAsync(NewRequest(_priyaId));
        var update = NewRequest(_priyaId);
        update.Title = "Renamed";
        update.Status = ProjectStatus.Completed;
        update.ProgressPercent = 10;

        var result = await _service.UpdateAsync(created.Id, update);

        Assert.Equal("Renamed", result.Title);
        Assert.Equal(100, result.ProgressPercent);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(99999, NewRequest()));
    }

    [Fact]
    public async Task UpdateAssignmentsAsync_ReplacesAssignedEmployees()
    {
        var created = await _service.CreateAsync(NewRequest(_priyaId));

        var result = await _service.UpdateAssignmentsAsync(created.Id, [_rahulId]);

        var assigned = Assert.Single(result.AssignedEmployees);
        Assert.Equal(_rahulId, assigned.Id);
    }

    [Fact]
    public async Task UpdateAssignmentsAsync_UnknownEmployee_ThrowsBadRequest()
    {
        var created = await _service.CreateAsync(NewRequest(_priyaId));

        await Assert.ThrowsAsync<BadRequestException>(() => _service.UpdateAssignmentsAsync(created.Id, [99999]));
    }

    [Fact]
    public async Task UpdateProgressAsync_AssignedEmployee_UpdatesProgress()
    {
        var created = await _service.CreateAsync(NewRequest(_priyaId));

        var result = await _service.UpdateProgressAsync(
            created.Id,
            new ProgressUpdateRequest { Status = ProjectStatus.InProgress, ProgressPercent = 65 },
            _priyaId);

        Assert.Equal(65, result.ProgressPercent);
        Assert.Equal(ProjectStatus.InProgress, result.Status);
    }

    [Fact]
    public async Task UpdateProgressAsync_UnassignedEmployee_ThrowsForbidden()
    {
        var created = await _service.CreateAsync(NewRequest(_priyaId));

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.UpdateProgressAsync(
            created.Id,
            new ProgressUpdateRequest { Status = ProjectStatus.InProgress, ProgressPercent = 50 },
            _rahulId));
    }

    [Fact]
    public async Task UpdateProgressAsync_AdminCompletes_ForcesFullProgress()
    {
        var created = await _service.CreateAsync(NewRequest(_priyaId));

        var result = await _service.UpdateProgressAsync(
            created.Id,
            new ProgressUpdateRequest { Status = ProjectStatus.Completed, ProgressPercent = 70 },
            null);

        Assert.Equal(100, result.ProgressPercent);
    }

    [Fact]
    public async Task GetAllAsync_EmployeeScope_ReturnsOnlyAssignedProjects()
    {
        await _service.CreateAsync(NewRequest(_rahulId));

        var result = await _service.GetAllAsync(new ProjectQuery { PageSize = 50 }, _priyaId);

        using var db = _factory.CreateContext();
        var expected = await db.Projects.CountAsync(p => p.Assignments.Any(a => a.EmployeeId == _priyaId));
        Assert.Equal(expected, result.TotalCount);
        Assert.All(result.Items, p => Assert.Contains(p.AssignedEmployees, e => e.Id == _priyaId));
    }

    [Fact]
    public async Task GetAllAsync_StatusFilter_ReturnsMatchingProjects()
    {
        var result = await _service.GetAllAsync(new ProjectQuery { Status = ProjectStatus.Completed, PageSize = 50 }, null);

        Assert.All(result.Items, p => Assert.Equal(ProjectStatus.Completed, p.Status));
    }

    [Fact]
    public async Task GetByIdAsync_UnassignedEmployee_ThrowsForbidden()
    {
        var created = await _service.CreateAsync(NewRequest(_rahulId));

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.GetByIdAsync(created.Id, _priyaId));
    }

    [Fact]
    public async Task DeleteAsync_ExistingProject_RemovesProject()
    {
        var created = await _service.CreateAsync(NewRequest(_priyaId));

        await _service.DeleteAsync(created.Id);

        using var db = _factory.CreateContext();
        Assert.False(await db.Projects.AnyAsync(p => p.Id == created.Id));
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(99999));
    }
}
