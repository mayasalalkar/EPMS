using Epms.Api.Dtos;
using Epms.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Epms.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Employee")]
[Route("api/dashboard")]
public sealed class DashboardController(IDashboardService dashboard) : ControllerBase
{
    [HttpGet]
    public Task<DashboardDto> Get(CancellationToken ct) => dashboard.GetSummaryAsync(
        User.IsInRole("Admin") ? null : int.Parse(User.FindFirst("employeeId")!.Value), ct);
}
