namespace Epms.Api.Models;

public enum Role { Admin, Employee }
public enum ProjectStatus { Pending, InProgress, Completed }
public enum ProjectPriority { Low, Medium, High }

public sealed class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public Role Role { get; set; }
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class Employee
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Department { get; set; }
    public required string Designation { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
    public List<ProjectAssignment> Assignments { get; set; } = [];
}

public sealed class Project
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly Deadline { get; set; }
    public ProjectStatus Status { get; set; }
    public ProjectPriority Priority { get; set; }
    public int ProgressPercent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<ProjectAssignment> Assignments { get; set; } = [];
}

public sealed class ProjectAssignment
{
    public int ProjectId { get; set; }
    public int EmployeeId { get; set; }
    public Project Project { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
