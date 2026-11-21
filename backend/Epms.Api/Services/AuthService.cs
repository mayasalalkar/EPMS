using System.Text;
using Epms.Api.Data;
using Epms.Api.Dtos;
using Epms.Api.Exceptions;
using Epms.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Epms.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest input, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest input, CancellationToken ct = default);
    Task<UserDto> GetCurrentUserAsync(int id, CancellationToken ct = default);
}

public sealed class AuthService(AppDbContext db, ITokenService tokens, IHostEnvironment environment) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest input, CancellationToken ct = default)
    {
        if (Encoding.UTF8.GetByteCount(input.Password) > 72)
            throw new BadRequestException("The password must be at most 72 UTF-8 bytes.");
        var email = EmployeeService.NormalizeEmail(input.Email);
        if (await db.Users.AnyAsync(x => x.Email == email, ct))
            throw new ConflictException("An account already exists for this email.");

        var employee = await db.Employees.SingleOrDefaultAsync(x => x.Email == email, ct);
        if (employee is null)
        {
            employee = new Employee
            {
                Email = email, Name = input.Name.Trim(), Phone = input.Phone?.Trim() ?? "",
                Department = string.IsNullOrWhiteSpace(input.Department) ? "General" : input.Department.Trim(),
                Designation = string.IsNullOrWhiteSpace(input.Designation) ? "Employee" : input.Designation.Trim()
            };
        }
        else if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
            throw new ConflictException("This employee record cannot be linked through self-registration in this environment.");

        var user = new User
        {
            Email = email, Role = Role.Employee, Employee = employee,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password)
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return CreateResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest input, CancellationToken ct = default)
    {
        var email = EmployeeService.NormalizeEmail(input.Email);
        var user = await db.Users.Include(x => x.Employee).SingleOrDefaultAsync(x => x.Email == email, ct);
        if (Encoding.UTF8.GetByteCount(input.Password) > 72 || user is null ||
            !BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");
        return CreateResponse(user);
    }

    public async Task<UserDto> GetCurrentUserAsync(int id, CancellationToken ct = default)
    {
        var user = await db.Users.AsNoTracking().Include(x => x.Employee).SingleOrDefaultAsync(x => x.Id == id, ct);
        return user is null ? throw new UnauthorizedException("The account is no longer available.") : Map(user);
    }

    private AuthResponse CreateResponse(User user)
    {
        var dto = Map(user);
        return tokens.Create(dto);
    }

    private static UserDto Map(User user) =>
        new(user.Id, user.Email, user.Employee?.Name ?? "Administrator", user.Role, user.EmployeeId);
}
