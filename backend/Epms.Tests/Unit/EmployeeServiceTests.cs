using Epms.Api.Dtos;
using Epms.Api.Exceptions;
using Epms.Api.Services;
using Epms.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Epms.Tests.Unit;

public class EmployeeServiceTests : IAsyncLifetime
{
    private TestDbContextFactory _factory = null!;
    private EmployeeService _service = null!;

    public async Task InitializeAsync()
    {
        _factory = await TestDbContextFactory.CreateAsync();
        _service = new EmployeeService(_factory.CreateContext());
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    private static EmployeeRequest NewRequest(string? email = null, string department = "Engineering") => new()
    {
        Name = "Test Employee",
        Email = email ?? TestData.UniqueEmail("employee"),
        Phone = "9876543210",
        Department = department,
        Designation = "Developer"
    };

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedEmployee()
    {
        var request = NewRequest();

        var result = await _service.CreateAsync(request);

        Assert.True(result.Id > 0);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal("Engineering", result.Department);
        using var db = _factory.CreateContext();
        Assert.True(await db.Employees.AnyAsync(e => e.Email == request.Email));
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmployeeEmail_ThrowsConflict()
    {
        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(NewRequest(TestData.PriyaEmail)));
    }

    [Fact]
    public async Task CreateAsync_EmailUsedByUnlinkedUser_ThrowsConflict()
    {
        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(NewRequest(TestData.AdminEmail)));
    }

    [Fact]
    public async Task UpdateAsync_ExistingEmployee_UpdatesFields()
    {
        var created = await _service.CreateAsync(NewRequest());
        var update = NewRequest(created.Email, "QA");
        update.Name = "Updated Name";
        update.Designation = "Lead";

        var result = await _service.UpdateAsync(created.Id, update);

        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("QA", result.Department);
        Assert.Equal("Lead", result.Designation);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(99999, NewRequest()));
    }

    [Fact]
    public async Task UpdateAsync_EmailOfAnotherEmployee_ThrowsConflict()
    {
        var created = await _service.CreateAsync(NewRequest());

        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateAsync(created.Id, NewRequest(TestData.RahulEmail)));
    }

    [Fact]
    public async Task DeleteAsync_EmployeeWithUser_RemovesEmployeeAndLinkedUser()
    {
        using var setup = _factory.CreateContext();
        var priyaId = await setup.Employees.Where(e => e.Email == TestData.PriyaEmail).Select(e => e.Id).SingleAsync();

        await _service.DeleteAsync(priyaId);

        using var db = _factory.CreateContext();
        Assert.False(await db.Employees.AnyAsync(e => e.Id == priyaId));
        Assert.False(await db.Users.AnyAsync(u => u.Email == TestData.PriyaEmail));
        Assert.False(await db.ProjectAssignments.AnyAsync(a => a.EmployeeId == priyaId));
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(99999));
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(99999));
    }

    [Fact]
    public async Task GetAllAsync_Paging_ReturnsRequestedPageAndTotals()
    {
        using var db = _factory.CreateContext();
        var total = await db.Employees.CountAsync();

        var result = await _service.GetAllAsync(new EmployeeQuery { Page = 2, PageSize = 2 });

        Assert.Equal(2, result.Items.Count());
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(total, result.TotalCount);
        Assert.Equal((int)Math.Ceiling(total / 2.0), result.TotalPages);
    }

    [Fact]
    public async Task GetAllAsync_Search_FiltersByNameOrEmail()
    {
        var result = await _service.GetAllAsync(new EmployeeQuery { Search = "priya" });

        var item = Assert.Single(result.Items);
        Assert.Equal(TestData.PriyaEmail, item.Email);
    }

    [Fact]
    public async Task GetAllAsync_DepartmentFilter_ReturnsOnlyThatDepartment()
    {
        await _service.CreateAsync(NewRequest(department: "Finance"));
        await _service.CreateAsync(NewRequest(department: "Finance"));

        var result = await _service.GetAllAsync(new EmployeeQuery { Department = "Finance" });

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, e => Assert.Equal("Finance", e.Department));
    }

    [Fact]
    public async Task GetAllAsync_SortByNameDescending_ReturnsOrderedItems()
    {
        var result = await _service.GetAllAsync(new EmployeeQuery { SortBy = "name", SortDir = "desc", PageSize = 50 });

        var names = result.Items.Select(e => e.Name).ToList();
        Assert.Equal(names.OrderByDescending(n => n, StringComparer.Ordinal).ToList(), names);
    }

    [Fact]
    public async Task GetDepartmentsAsync_ReturnsDistinctDepartments()
    {
        var departments = await _service.GetDepartmentsAsync();

        Assert.NotEmpty(departments);
        Assert.Equal(departments.Count, departments.Distinct().Count());
        Assert.Contains("Engineering", departments);
    }
}
