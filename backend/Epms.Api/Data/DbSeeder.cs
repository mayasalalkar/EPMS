using Epms.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Epms.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, string? adminPassword, string? employeePassword,
        bool includeTestAccounts = false)
    {
        if (string.IsNullOrWhiteSpace(adminPassword) || string.IsNullOrWhiteSpace(employeePassword) ||
            await db.Users.AnyAsync() || await db.Employees.AnyAsync())
            return;

        var john = NewEmployee("John Miller", "john", "Engineering", "Developer", 1);
        var priya = NewEmployee("Priya Sharma", "priya.sharma", "Engineering", "Tech Lead", 2);
        var rahul = NewEmployee("Rahul Verma", "rahul.verma", "Operations", "Analyst", 3);
        var aisha = NewEmployee("Aisha Patel", "aisha.patel", "Design", "Designer", 4);
        var leo = NewEmployee("Leo Chen", "leo.chen", "Operations", "Coordinator", 5);
        var eva = NewEmployee("Eva Martin", "eva.martin", "Quality", "QA Engineer", 6);
        db.Employees.AddRange(john, priya, rahul, aisha, leo, eva);

        var employeeHash = BCrypt.Net.BCrypt.HashPassword(employeePassword);
        db.Users.Add(new User { Email = "admin@epms.local", Role = Role.Admin,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword) });
        db.Users.Add(new User { Email = john.Email, Role = Role.Employee, Employee = john, PasswordHash = employeeHash });
        if (includeTestAccounts)
        {
            db.Users.AddRange(
                new User { Email = priya.Email, Role = Role.Employee, Employee = priya, PasswordHash = employeeHash },
                new User { Email = rahul.Email, Role = Role.Employee, Employee = rahul, PasswordHash = employeeHash });
        }

        db.Projects.AddRange(
            NewProject("Employee Portal", "Build the internal employee self-service portal.", -18, 12,
                ProjectStatus.InProgress, ProjectPriority.High, 60, john, priya, aisha),
            NewProject("Asset Inventory", "Review hardware and update the asset register.", -13, -2,
                ProjectStatus.InProgress, ProjectPriority.Medium, 30, rahul, leo),
            NewProject("Onboarding Refresh", "Consolidate the new-hire checklists.", -20, -4,
                ProjectStatus.Completed, ProjectPriority.Medium, 100, john, leo),
            NewProject("Support Dashboard", "Report on help desk response and resolution times.", 2, 28,
                ProjectStatus.Pending, ProjectPriority.Low, 0, john, eva),
            NewProject("Accessibility Audit", "Assess core application screens for accessibility.", 1, 21,
                ProjectStatus.Pending, ProjectPriority.High, 0, aisha, eva),
            NewProject("Release Checklist", "Document the steps for a repeatable production release.", -12, -1,
                ProjectStatus.Completed, ProjectPriority.Low, 100, eva, rahul));

        await db.SaveChangesAsync();
    }

    private static Employee NewEmployee(string name, string email, string department, string designation, int suffix) =>
        new() { Name = name, Email = $"{email}@epms.local", Department = department,
            Designation = designation, Phone = $"555010000{suffix}" };

    private static Project NewProject(string title, string description, int startOffset, int deadlineOffset,
        ProjectStatus status, ProjectPriority priority, int progress, params Employee[] employees)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new Project
        {
            Title = title, Description = description, StartDate = today.AddDays(startOffset),
            Deadline = today.AddDays(deadlineOffset), Status = status, Priority = priority,
            ProgressPercent = progress, Assignments = employees.Select(x => new ProjectAssignment { Employee = x }).ToList()
        };
    }
}
