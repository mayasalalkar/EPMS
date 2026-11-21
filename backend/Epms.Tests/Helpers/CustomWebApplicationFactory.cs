using System.Net.Http.Headers;
using System.Net.Http.Json;
using Epms.Api.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace Epms.Tests.Helpers;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _anchor =
        new($"Data Source=epms-test-{Guid.NewGuid():N};Mode=Memory;Cache=Shared;Pooling=False");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DatabaseProvider", "Sqlite");
        _anchor.Open();
        builder.UseSetting("ConnectionStrings:Sqlite", _anchor.ConnectionString);
        builder.UseSetting("Demo:AdminPassword", TestData.AdminPassword);
        builder.UseSetting("Demo:EmployeePassword", TestData.EmployeePassword);
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest { Email = email, Password = password },
            TestData.JsonOptions);
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(TestData.JsonOptions);
        return auth!;
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password)
    {
        var auth = await LoginAsync(email, password);
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return client;
    }

    public Task<HttpClient> CreateAdminClientAsync() =>
        CreateAuthenticatedClientAsync(TestData.AdminEmail, TestData.AdminPassword);

    public Task<HttpClient> CreatePriyaClientAsync() =>
        CreateAuthenticatedClientAsync(TestData.PriyaEmail, TestData.EmployeePassword);

    public Task<HttpClient> CreateRahulClientAsync() =>
        CreateAuthenticatedClientAsync(TestData.RahulEmail, TestData.EmployeePassword);

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _anchor.Dispose();
    }
}
