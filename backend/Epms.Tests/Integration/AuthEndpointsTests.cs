using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Epms.Api.Dtos;
using Epms.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;

namespace Epms.Tests.Integration;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ValidAdmin_ReturnsTokenAndUser()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = TestData.AdminEmail, Password = TestData.AdminPassword }, TestData.JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(TestData.JsonOptions);
        Assert.False(string.IsNullOrWhiteSpace(auth!.Token));
        Assert.Equal(TestData.AdminEmail, auth.User.Email);
        Assert.Equal("Admin", auth.User.Role.ToString());
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401WithMessage()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = TestData.AdminEmail, Password = "Wrong@123" }, TestData.JsonOptions);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Invalid email or password.", body.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Register_NewUser_ReturnsEmployeeAuthResponse()
    {
        var client = _factory.CreateClient();
        var email = TestData.UniqueEmail("register");

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Name = "Anita Desai", Email = email, Password = "Secret@123" }, TestData.JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(TestData.JsonOptions);
        Assert.Equal(email, auth!.User.Email);
        Assert.Equal("Employee", auth.User.Role.ToString());
        Assert.NotNull(auth.User.EmployeeId);
    }

    [Fact]
    public async Task Register_ExistingUserEmail_Returns409()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Name = "Priya", Email = TestData.PriyaEmail, Password = "Secret@123" }, TestData.JsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400ValidationProblem()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Name = "Short", Email = TestData.UniqueEmail(), Password = "123" }, TestData.JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task Register_ExistingEmployee_LinksByEmailInDemo()
    {
        using var admin = await _factory.CreateAdminClientAsync();
        using var guest = _factory.CreateClient();
        var email = TestData.UniqueEmail("existing");
        var created = await admin.PostAsJsonAsync("/api/employees", new EmployeeRequest
        {
            Name = "Existing Employee", Email = email, Phone = "5550100099", Department = "Design", Designation = "Designer"
        }, TestData.JsonOptions);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var record = (await created.Content.ReadFromJsonAsync<EmployeeDto>(TestData.JsonOptions))!;
        var request = new RegisterRequest { Email = email, Name = "Entered at registration", Password = "Secret@123" };
        var registered = await guest.PostAsJsonAsync("/api/auth/register", request, TestData.JsonOptions);
        Assert.Equal(HttpStatusCode.OK, registered.StatusCode);
        var auth = (await registered.Content.ReadFromJsonAsync<AuthResponse>(TestData.JsonOptions))!;
        Assert.Equal(record.Id, auth.User.EmployeeId);
        Assert.Equal(record.Name, auth.User.Name);
        Assert.Equal(HttpStatusCode.Conflict,
            (await guest.PostAsJsonAsync("/api/auth/register", request, TestData.JsonOptions)).StatusCode);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithEmployeeToken_ReturnsCurrentUser()
    {
        var client = await _factory.CreatePriyaClientAsync();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>(TestData.JsonOptions);
        Assert.Equal(TestData.PriyaEmail, user!.Email);
        Assert.NotNull(user.EmployeeId);
    }

    [Fact]
    public async Task SwaggerJson_IsAvailable()
    {
        using var factory = _factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development"));
        var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("/api/projects", await response.Content.ReadAsStringAsync());
    }
}
