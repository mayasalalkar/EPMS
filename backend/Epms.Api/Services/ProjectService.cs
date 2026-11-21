using System.Linq.Expressions;
using Epms.Api.Data;
using Epms.Api.Dtos;
using Epms.Api.Exceptions;
using Epms.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Epms.Api.Services;

public interface IProjectService
{
    Task<PagedResult<ProjectDto>> GetAllAsync(ProjectQuery query, int? employeeId, CancellationToken ct = default);
    Task<ProjectDto> GetByIdAsync(int id, int? employeeId, CancellationToken ct = default);
    Task<ProjectDto> CreateAsync(ProjectRequest request, CancellationToken ct = default);
    Task<ProjectDto> UpdateAsync(int id, ProjectRequest request, CancellationToken ct = default);
    Task<ProjectDto> UpdateAssignmentsAsync(int id, int[] employeeIds, CancellationToken ct = default);
    Task<ProjectDto> UpdateProgressAsync(int id, ProgressUpdateRequest request, int? employeeId, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public sealed class ProjectService(AppDbContext db) : IProjectService
{
    internal static readonly Expression<Func<Project, ProjectDto>> SelectDto = x =>
        new ProjectDto(x.Id, x.Title, x.Description, x.StartDate, x.Deadline, x.Status, x.Priority, x.ProgressPercent,
            x.Assignments.OrderBy(a => a.Employee.Name).ThenBy(a => a.EmployeeId)
                .Select(a => new EmployeeLookupDto(a.EmployeeId, a.Employee.Name)).ToList());

    internal static IQueryable<Project> Scope(IQueryable<Project> projects, int? employeeId) =>
        employeeId is int id ? projects.Where(x => x.Assignments.Any(a => a.EmployeeId == id)) : projects;

    public Task<PagedResult<ProjectDto>> GetAllAsync(ProjectQuery query, int? employeeId, CancellationToken ct = default)
    {
        var projects = Scope(db.Projects.AsNoTracking(), employeeId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            projects = projects.Where(x => x.Title.ToLower().Contains(search) || x.Description.ToLower().Contains(search));
        }
        if (query.Status is { } status)
            projects = projects.Where(x => x.Status == status);
        if (query.Priority is { } priority)
            projects = projects.Where(x => x.Priority == priority);

        var descending = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        IOrderedQueryable<Project> ordered = query.SortBy?.ToLowerInvariant() switch
        {
            "startdate" => Order(projects, x => x.StartDate, descending),
            "deadline" => Order(projects, x => x.Deadline, descending),
            "status" => Order(projects, x => x.Status == ProjectStatus.Pending ? 0 : x.Status == ProjectStatus.InProgress ? 1 : 2, descending),
            "priority" => Order(projects, x => x.Priority == ProjectPriority.Low ? 0 : x.Priority == ProjectPriority.Medium ? 1 : 2, descending),
            "progress" or "progresspercent" => Order(projects, x => x.ProgressPercent, descending),
            _ => Order(projects, x => x.Title, descending)
        };
        return Page.ReadAsync(ordered.ThenBy(x => x.Id).Select(SelectDto), query, ct);
    }

    public async Task<ProjectDto> GetByIdAsync(int id, int? employeeId, CancellationToken ct = default)
    {
        var projects = db.Projects.AsNoTracking().Where(x => x.Id == id);
        var project = await Scope(projects, employeeId).Select(SelectDto).SingleOrDefaultAsync(ct);
        if (project is not null)
            return project;
        if (employeeId is not null && await projects.AnyAsync(ct))
            throw new ForbiddenException("You are not assigned to this project.");
        throw new NotFoundException("Project not found.");
    }

    public async Task<ProjectDto> CreateAsync(ProjectRequest request, CancellationToken ct = default)
    {
        var project = new Project { Title = "", Description = "" };
        Apply(project, request);
        await SetAssignmentsAsync(project, request.EmployeeIds, ct);
        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(project.Id, null, ct);
    }

    public async Task<ProjectDto> UpdateAsync(int id, ProjectRequest request, CancellationToken ct = default)
    {
        var project = await FindAsync(id, ct);
        Apply(project, request);
        await SetAssignmentsAsync(project, request.EmployeeIds, ct);
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, null, ct);
    }

    public async Task<ProjectDto> UpdateAssignmentsAsync(int id, int[] employeeIds, CancellationToken ct = default)
    {
        var project = await FindAsync(id, ct);
        await SetAssignmentsAsync(project, employeeIds, ct);
        project.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, null, ct);
    }

    public async Task<ProjectDto> UpdateProgressAsync(int id, ProgressUpdateRequest request, int? employeeId,
        CancellationToken ct = default)
    {
        var (status, progress) = ReadProgress(request);
        var updatedAt = DateTime.UtcNow;
        var updated = await Scope(db.Projects.Where(x => x.Id == id), employeeId)
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.Status, status)
                .SetProperty(x => x.ProgressPercent, progress)
                .SetProperty(x => x.UpdatedAt, updatedAt), ct);
        if (updated == 0)
        {
            if (await db.Projects.AnyAsync(x => x.Id == id, ct))
                throw new ForbiddenException("You are not assigned to this project.");
            throw new NotFoundException("Project not found.");
        }
        return await GetByIdAsync(id, employeeId, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        db.Projects.Remove(await FindAsync(id, ct));
        await db.SaveChangesAsync(ct);
    }

    private async Task<Project> FindAsync(int id, CancellationToken ct) =>
        await db.Projects.Include(x => x.Assignments).SingleOrDefaultAsync(x => x.Id == id, ct)
        ?? throw new NotFoundException("Project not found.");

    private async Task SetAssignmentsAsync(Project project, int[] employeeIds, CancellationToken ct)
    {
        var ids = employeeIds.Distinct().ToArray();
        if (ids.Length == 0)
            throw new BadRequestException("Assign at least one employee.");
        if (await db.Employees.CountAsync(x => ids.Contains(x.Id), ct) != ids.Length)
            throw new BadRequestException("One or more selected employees do not exist.");
        project.Assignments.RemoveAll(x => !ids.Contains(x.EmployeeId));
        var assigned = project.Assignments.Select(x => x.EmployeeId).ToHashSet();
        project.Assignments.AddRange(ids.Where(id => !assigned.Contains(id))
            .Select(id => new ProjectAssignment { EmployeeId = id }));
    }

    private static void Apply(Project project, ProjectRequest request)
    {
        if (request.StartDate is not { } start || request.Deadline is not { } deadline || deadline.Date < start.Date)
            throw new BadRequestException("Deadline must be on or after the start date.");
        if (request.Priority is not { } priority || !Enum.IsDefined(priority))
            throw new BadRequestException("Choose a valid priority.");

        var (status, progress) = ReadProgress(request);
        project.Title = request.Title.Trim();
        project.Description = request.Description.Trim();
        project.StartDate = DateOnly.FromDateTime(start);
        project.Deadline = DateOnly.FromDateTime(deadline);
        project.Status = status;
        project.Priority = priority;
        project.ProgressPercent = progress;
        project.UpdatedAt = DateTime.UtcNow;
    }

    private static (ProjectStatus Status, int Progress) ReadProgress(ProgressUpdateRequest input)
    {
        if (input.Status is not { } status || !Enum.IsDefined(status) ||
            input.ProgressPercent is not (>= 0 and <= 100) || input.ProgressPercent is not int progress)
            throw new BadRequestException("Provide a valid status and a progress percentage between 0 and 100.");
        return (status, status == ProjectStatus.Completed ? 100 : progress);
    }

    private static IOrderedQueryable<Project> Order<T>(IQueryable<Project> projects,
        Expression<Func<Project, T>> selector, bool descending) =>
        descending ? projects.OrderByDescending(selector) : projects.OrderBy(selector);
}
