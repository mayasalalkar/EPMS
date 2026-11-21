using Epms.Api.Dtos;
using Epms.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Epms.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/employees")]
public sealed class EmployeesController(IEmployeeService employees) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<EmployeeDto>> List([FromQuery] EmployeeQuery query, CancellationToken ct) =>
        employees.GetAllAsync(query, ct);

    [HttpGet("departments")]
    public Task<List<string>> Departments(CancellationToken ct) => employees.GetDepartmentsAsync(ct);

    [HttpGet("lookup")]
    public Task<List<DirectoryLookupDto>> Lookup(CancellationToken ct) => employees.GetLookupAsync(ct);

    [HttpGet("{id:int}")]
    public Task<EmployeeDto> Get(int id, CancellationToken ct) => employees.GetByIdAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(EmployeeRequest request, CancellationToken ct)
    {
        var employee = await employees.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { employee.Id }, employee);
    }

    [HttpPut("{id:int}")]
    public Task<EmployeeDto> Update(int id, EmployeeRequest request, CancellationToken ct) =>
        employees.UpdateAsync(id, request, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await employees.DeleteAsync(id, ct);
        return NoContent();
    }

}
