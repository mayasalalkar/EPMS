using Epms.Api.Dtos;
using Epms.Api.Exceptions;
using Epms.Api.Models;
using Epms.Api.Services;
using Epms.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Epms.Tests.Unit;

public class AuthServiceTests : IAsyncLifetime
{
    private TestDbContextFactory _factory = null!;
    private AuthService _service = null!;
    private TokenService _tokens = null!;

    public async Task InitializeAsync()
    {
        _factory = await TestDbContextFactory.CreateAsync();
        var settings = new JwtSettings(new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)),
            "Epms.Api", "Epms.Client", 60);
        _tokens = new TokenService(settings);
        _service = new AuthService(_factory.CreateContext(), _tokens, new HostingEnvironment { EnvironmentName = "Development" });
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    private static RegisterRequest NewRegisterRequest(string email) => new()
    {
        Name = "New Person",
        Email = email,
        Password = "Secret@123"
    };

    [Fact]
    public async Task RegisterAsync_NewEmail_CreatesEmployeeUserWithDefaults()
    {
        var email = TestData.UniqueEmail("register");

        var result = await _service.RegisterAsync(NewRegisterRequest(email));

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("Employee", result.User.Role.ToString());
        Assert.NotNull(result.User.EmployeeId);
        using var db = _factory.CreateContext();
        var employee = await db.Employees.SingleAsync(e => e.Email == email);
        Assert.Equal(employee.Id, result.User.EmployeeId);
        Assert.Equal("General", employee.Department);
        Assert.Equal("Employee", employee.Designation);
    }

    [Fact]
    public async Task RegisterAsync_ExistingEmployeeEmail_LinksOnlyInDemoEnvironments()
    {
        var email = TestData.UniqueEmail("existing");
        int employeeId;
        using (var setup = _factory.CreateContext())
        {
            var employee = new Employee
            {
                Name = "Existing Person",
                Email = email,
                Phone = "9123456780",
                Department = "Design",
                Designation = "Designer",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            setup.Employees.Add(employee);
            await setup.SaveChangesAsync();
            employeeId = employee.Id;
        }

        var request = NewRegisterRequest(email);
        using var registration = _factory.CreateContext();
        var productionAuth = new AuthService(registration, _tokens, new HostingEnvironment { EnvironmentName = "Production" });
        await Assert.ThrowsAsync<ConflictException>(() => productionAuth.RegisterAsync(request));
        var result = await _service.RegisterAsync(request);

        Assert.Equal(employeeId, result.User.EmployeeId);
        using var db = _factory.CreateContext();
        Assert.Equal(1, await db.Employees.CountAsync(e => e.Email == email));
        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_ExistingUserEmail_ThrowsConflict()
    {
        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterAsync(NewRegisterRequest(TestData.PriyaEmail)));
    }

    [Fact]
    public async Task LoginAsync_ValidAdminCredentials_ReturnsTokenAndAdminRole()
    {
        var result = await _service.LoginAsync(new LoginRequest { Email = TestData.AdminEmail, Password = TestData.AdminPassword });

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("Admin", result.User.Role.ToString());
        Assert.Null(result.User.EmployeeId);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_EmployeeCredentials_TokenContainsEmployeeIdClaim()
    {
        var result = await _service.LoginAsync(new LoginRequest { Email = TestData.PriyaEmail, Password = TestData.EmployeePassword });

        var token = new JsonWebTokenHandler().ReadJsonWebToken(result.Token);
        var claim = Assert.Single(token.Claims, c => c.Type == "employeeId");
        Assert.Equal(result.User.EmployeeId.ToString(), claim.Value);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginRequest { Email = TestData.AdminEmail, Password = "Wrong@123" }));
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginRequest { Email = TestData.UniqueEmail("nobody"), Password = "Secret@123" }));
    }

    [Fact]
    public async Task GetCurrentUserAsync_ExistingUser_ReturnsUserDto()
    {
        var login = await _service.LoginAsync(new LoginRequest { Email = TestData.RahulEmail, Password = TestData.EmployeePassword });

        var result = await _service.GetCurrentUserAsync(login.User.Id);

        Assert.Equal(TestData.RahulEmail, result.Email);
        Assert.False(string.IsNullOrWhiteSpace(result.Name));
    }
}
