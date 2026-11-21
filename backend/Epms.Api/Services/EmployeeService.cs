using System.Linq.Expressions;
using Epms.Api.Data;
using Epms.Api.Dtos;
using Epms.Api.Exceptions;
using Epms.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Epms.Api.Services;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeDto>> GetAllAsync(EmployeeQuery query, CancellationToken ct = default);
    Task<List<DirectoryLookupDto>> GetLookupAsync(CancellationToken ct = default);
    Task<List<string>> GetDepartmentsAsync(CancellationToken ct = default);
    Task<EmployeeDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<EmployeeDto> CreateAsync(EmployeeRequest input, CancellationToken ct = default);
    Task<EmployeeDto> UpdateAsync(int id, EmployeeRequest input, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public sealed class EmployeeService(AppDbContext db) : IEmployeeService
{
    private static readonly Expression<Func<Employee, EmployeeDto>> SelectDto =
        x => new EmployeeDto(x.Id, x.Name, x.Email, x.Phone, x.Department, x.Designation);

    public async Task<PagedResult<EmployeeDto>> GetAllAsync(EmployeeQuery query, CancellationToken ct = default)
    {
        var employees = db.Employees.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            employees = employees.Where(x => x.Name.ToLower().Contains(search) || x.Email.Contains(search) ||
                x.Department.ToLower().Contains(search) || x.Designation.ToLower().Contains(search) ||
                x.Phone.Contains(search));
        }
        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            var department = query.Department.Trim().ToLowerInvariant();
            employees = employees.Where(x => x.Department.ToLower() == department);
        }
        var descending = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        Expression<Func<Employee, string>> key = query.SortBy?.ToLowerInvariant() switch
        {
            "email" => x => x.Email,
            "phone" => x => x.Phone,
            "department" => x => x.Department,
            "designation" => x => x.Designation,
            _ => x => x.Name
        };
        var ordered = descending ? employees.OrderByDescending(key) : employees.OrderBy(key);
        return await Page.ReadAsync(ordered.ThenBy(x => x.Id).Select(SelectDto), query, ct);
    }

    public Task<List<DirectoryLookupDto>> GetLookupAsync(CancellationToken ct = default) =>
        db.Employees.OrderBy(x => x.Name).ThenBy(x => x.Id)
            .Select(x => new DirectoryLookupDto(x.Id, x.Name, x.Email)).ToListAsync(ct);

    public Task<List<string>> GetDepartmentsAsync(CancellationToken ct = default) =>
        db.Employees.Select(x => x.Department).Distinct().OrderBy(x => x).ToListAsync(ct);

    public async Task<EmployeeDto> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Employees.Where(x => x.Id == id).Select(SelectDto).SingleOrDefaultAsync(ct)
        ?? throw new NotFoundException("Employee not found.");

    public async Task<EmployeeDto> CreateAsync(EmployeeRequest input, CancellationToken ct = default)
    {
        var email = NormalizeEmail(input.Email);
        await CheckEmailAsync(email, 0, ct);
        var employee = new Employee
        {
            Email = email, Name = input.Name.Trim(), Phone = input.Phone.Trim(),
            Department = input.Department.Trim(), Designation = input.Designation.Trim()
        };
        db.Employees.Add(employee);
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(employee.Id, ct);
    }

    public async Task<EmployeeDto> UpdateAsync(int id, EmployeeRequest input, CancellationToken ct = default)
    {
        var employee = await FindAsync(id, ct);
        var email = NormalizeEmail(input.Email);
        if (employee.Email != email)
        {
            if (employee.User is not null)
                throw new ConflictException("The email of an employee with a login cannot be changed here.");
            await CheckEmailAsync(email, id, ct);
            employee.Email = email;
        }
        employee.Name = input.Name.Trim();
        employee.Phone = input.Phone.Trim();
        employee.Department = input.Department.Trim();
        employee.Designation = input.Designation.Trim();
        employee.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var employee = await FindAsync(id, ct);
        if (await db.ProjectAssignments.AnyAsync(x => x.EmployeeId == id &&
            x.Project.Assignments.Count == 1, ct))
            throw new ConflictException("Reassign projects where this employee is the only assignee before deleting them.");
        db.Employees.Remove(employee);
        await db.SaveChangesAsync(ct);
    }

    internal static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private async Task<Employee> FindAsync(int id, CancellationToken ct = default) =>
        await db.Employees.Include(x => x.User).SingleOrDefaultAsync(x => x.Id == id, ct)
        ?? throw new NotFoundException("Employee not found.");

    private async Task CheckEmailAsync(string email, int id, CancellationToken ct = default)
    {
        if (await db.Employees.AnyAsync(x => x.Email == email && x.Id != id, ct) ||
            await db.Users.AnyAsync(x => x.Email == email && (x.EmployeeId == null || x.EmployeeId != id), ct))
            throw new ConflictException("This email address is already in use.");
    }
}

internal static class Page
{
    public static async Task<PagedResult<T>> ReadAsync<T>(IQueryable<T> query, PageQuery page, CancellationToken ct = default)
    {
        var total = await query.CountAsync(ct);
        var skip = ((long)page.Page - 1) * page.PageSize;
        var items = skip >= total ? [] : await query.Skip((int)skip).Take(page.PageSize).ToListAsync(ct);
        return new PagedResult<T>(items, page.Page, page.PageSize, total);
    }
}
