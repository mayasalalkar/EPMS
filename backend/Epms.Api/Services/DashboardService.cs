using Epms.Api.Data;
using Epms.Api.Dtos;
using Epms.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Epms.Api.Services;

public interface IDashboardService
{
    Task<DashboardDto> GetSummaryAsync(int? employeeId, CancellationToken ct = default);
}

public sealed class DashboardService(AppDbContext db) : IDashboardService
{
    public async Task<DashboardDto> GetSummaryAsync(int? employeeId, CancellationToken ct = default)
    {
        var projects = ProjectService.Scope(db.Projects.AsNoTracking(), employeeId);
        var employees = db.Employees.AsNoTracking();
        if (employeeId is int id)
            employees = employees.Where(x => x.Id == id);

        var rows = await employees.OrderBy(x => x.Name).ThenBy(x => x.Id).Select(x => new EmployeeSummary(
            x.Id, x.Name, x.Department, x.Assignments.Count,
            x.Assignments.Count(a => a.Project.Status == ProjectStatus.Pending),
            x.Assignments.Count(a => a.Project.Status == ProjectStatus.InProgress),
            x.Assignments.Count(a => a.Project.Status == ProjectStatus.Completed))).ToListAsync(ct);

        var counts = await projects.GroupBy(x => new { x.Status, x.Priority }).Select(group => new
        {
            group.Key.Status, group.Key.Priority, Total = group.Count()
        }).ToListAsync(ct);
        int CountStatus(ProjectStatus status) => counts.Where(x => x.Status == status).Sum(x => x.Total);
        int CountPriority(ProjectPriority priority) => counts.Where(x => x.Priority == priority).Sum(x => x.Total);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var open = projects.Where(x => x.Status != ProjectStatus.Completed);
        var overdue = await open.CountAsync(x => x.Deadline < today, ct);
        var upcoming = await open.Where(x => x.Deadline >= today).OrderBy(x => x.Deadline).ThenBy(x => x.Id)
            .Take(5).Select(ProjectService.SelectDto).ToListAsync(ct);

        return new DashboardDto(rows.Count, counts.Sum(x => x.Total),
            new StatusCounts(CountStatus(ProjectStatus.Pending), CountStatus(ProjectStatus.InProgress), CountStatus(ProjectStatus.Completed)),
            new PriorityCounts(CountPriority(ProjectPriority.Low), CountPriority(ProjectPriority.Medium), CountPriority(ProjectPriority.High)),
            rows, overdue, upcoming);
    }
}
