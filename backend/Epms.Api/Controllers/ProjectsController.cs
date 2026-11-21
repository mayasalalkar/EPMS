using Epms.Api.Dtos;
using Epms.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Epms.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Employee")]
[Route("api/projects")]
public sealed class ProjectsController(IProjectService projects) : ControllerBase
{
    private int? EmployeeScope => User.IsInRole("Admin") ? null : int.Parse(User.FindFirst("employeeId")!.Value);

    [HttpGet]
    public Task<PagedResult<ProjectDto>> List([FromQuery] ProjectQuery query, CancellationToken ct) =>
        projects.GetAllAsync(query, EmployeeScope, ct);

    [HttpGet("{id:int}")]
    public Task<ProjectDto> Get(int id, CancellationToken ct) => projects.GetByIdAsync(id, EmployeeScope, ct);

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(ProjectRequest request, CancellationToken ct)
    {
        var project = await projects.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { project.Id }, project);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public Task<ProjectDto> Update(int id, ProjectRequest request, CancellationToken ct) =>
        projects.UpdateAsync(id, request, ct);

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await projects.DeleteAsync(id, ct);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/assignments")]
    public Task<ProjectDto> Assign(int id, AssignmentRequest request, CancellationToken ct) =>
        projects.UpdateAssignmentsAsync(id, request.EmployeeIds, ct);

    [HttpPatch("{id:int}/progress")]
    public Task<ProjectDto> Progress(int id, ProgressUpdateRequest request, CancellationToken ct) =>
        projects.UpdateProgressAsync(id, request, EmployeeScope, ct);
}
