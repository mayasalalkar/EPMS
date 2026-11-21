using Epms.Api.Dtos;
using Epms.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Epms.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public Task<AuthResponse> Register(RegisterRequest request, CancellationToken ct) => auth.RegisterAsync(request, ct);

    [HttpPost("login")]
    public Task<AuthResponse> Login(LoginRequest request, CancellationToken ct) => auth.LoginAsync(request, ct);

    [Authorize]
    [HttpGet("me")]
    public Task<UserDto> Me(CancellationToken ct) =>
        auth.GetCurrentUserAsync(int.Parse(User.FindFirst("sub")!.Value), ct);
}
