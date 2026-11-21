using Epms.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Epms.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectAssignment> ProjectAssignments => Set<ProjectAssignment>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<User>(user =>
        {
            user.HasIndex(x => x.Email).IsUnique();
            user.Property(x => x.Email).HasMaxLength(254);
            user.Property(x => x.PasswordHash).HasMaxLength(100);
            user.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            user.HasOne(x => x.Employee).WithOne(x => x.User).HasForeignKey<User>(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        model.Entity<Employee>(employee =>
        {
            employee.HasIndex(x => x.Email).IsUnique();
            employee.Property(x => x.Email).HasMaxLength(254);
            employee.Property(x => x.Name).HasMaxLength(120);
            employee.Property(x => x.Phone).HasMaxLength(30);
            employee.Property(x => x.Department).HasMaxLength(80);
            employee.Property(x => x.Designation).HasMaxLength(100);
        });
        model.Entity<Project>(project =>
        {
            project.Property(x => x.Title).HasMaxLength(160);
            project.Property(x => x.Description).HasMaxLength(2000);
            project.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            project.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
            project.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Projects_Dates", "Deadline >= StartDate");
                table.HasCheckConstraint("CK_Projects_Progress", "ProgressPercent BETWEEN 0 AND 100");
            });
        });
        model.Entity<ProjectAssignment>(assignment =>
        {
            assignment.HasKey(x => new { x.ProjectId, x.EmployeeId });
            assignment.HasOne(x => x.Project).WithMany(x => x.Assignments).OnDelete(DeleteBehavior.Cascade);
            assignment.HasOne(x => x.Employee).WithMany(x => x.Assignments).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
