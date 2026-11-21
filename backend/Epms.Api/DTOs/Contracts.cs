using System.ComponentModel.DataAnnotations;
using Epms.Api.Models;

namespace Epms.Api.Dtos;

public class EmployeeRequest
{
    [Required, StringLength(120)] public string Name { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required, Phone, StringLength(30)] public string Phone { get; set; } = "";
    [Required, StringLength(80)] public string Department { get; set; } = "";
    [Required, StringLength(100)] public string Designation { get; set; } = "";
}

public sealed class RegisterRequest
{
    [Required, StringLength(120)] public string Name { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Phone, StringLength(30)] public string? Phone { get; set; }
    [StringLength(80)] public string? Department { get; set; }
    [StringLength(100)] public string? Designation { get; set; }
    [Required, StringLength(72, MinimumLength = 8)] public string Password { get; set; } = "";
}

public sealed class LoginRequest
{
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required, StringLength(72)] public string Password { get; set; } = "";
}

public record UserDto(int Id, string Email, string Name, Role Role, int? EmployeeId);
public record AuthResponse(string Token, UserDto User, DateTime ExpiresAt);
public record EmployeeDto(int Id, string Name, string Email, string Phone, string Department, string Designation);
public record EmployeeLookupDto(int Id, string Name);
public record DirectoryLookupDto(int Id, string Name, string Email);

public class PageQuery
{
    [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 10;
    [StringLength(160)] public string? Search { get; set; }
    [StringLength(30)] public string? SortBy { get; set; }
    [RegularExpression("(?i)^(asc|desc)$")] public string SortDir { get; set; } = "asc";
}

public sealed class EmployeeQuery : PageQuery
{
    [StringLength(80)] public string? Department { get; set; }
}

public sealed class ProjectQuery : PageQuery
{
    [EnumDataType(typeof(ProjectStatus))] public ProjectStatus? Status { get; set; }
    [EnumDataType(typeof(ProjectPriority))] public ProjectPriority? Priority { get; set; }
}

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public sealed class ProjectRequest : ProgressUpdateRequest
{
    [Required, StringLength(160)] public string Title { get; set; } = "";
    [Required, StringLength(2000)] public string Description { get; set; } = "";
    [Required] public DateTime? StartDate { get; set; }
    [Required] public DateTime? Deadline { get; set; }
    [Required, EnumDataType(typeof(ProjectPriority))] public ProjectPriority? Priority { get; set; }
    [Required, MinLength(1)] public int[] EmployeeIds { get; set; } = [];
}

public class ProgressUpdateRequest
{
    [Required, EnumDataType(typeof(ProjectStatus))] public ProjectStatus? Status { get; set; }
    [Required, Range(0, 100)] public int? ProgressPercent { get; set; }
}

public sealed class AssignmentRequest
{
    [Required, MinLength(1)] public int[] EmployeeIds { get; set; } = [];
}

public record ProjectDto(int Id, string Title, string Description, DateOnly StartDate, DateOnly Deadline,
    ProjectStatus Status, ProjectPriority Priority, int ProgressPercent, IReadOnlyList<EmployeeLookupDto> AssignedEmployees);

public record StatusCounts(int Pending, int InProgress, int Completed);
public record PriorityCounts(int Low, int Medium, int High);
public record EmployeeSummary(int EmployeeId, string Name, string Department, int TotalProjects,
    int PendingProjects, int InProgressProjects, int CompletedProjects);
public record DashboardDto(int TotalEmployees, int TotalProjects, StatusCounts StatusCounts,
    PriorityCounts PriorityCounts, IReadOnlyList<EmployeeSummary> EmployeeAssignments,
    int OverdueProjects, IReadOnlyList<ProjectDto> UpcomingDeadlines)
{
    public int PendingProjects => StatusCounts.Pending;
    public int InProgressProjects => StatusCounts.InProgress;
    public int CompletedProjects => StatusCounts.Completed;
}
