using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly RoleManager _roleManager;

    public RoleController(RoleManager roleManager)
    {
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<OperationResult<List<RoleResponse>>> GetAll(CancellationToken ct)
    {
        return OperationResult<List<RoleResponse>>.Ok(await _roleManager.GetAllRolesAsync(ct));
    }

    [HttpGet("{id}")]
    public async Task<OperationResult<RoleResponse>> GetById(int id, CancellationToken ct)
    {
        var role = await _roleManager.GetByIdAsync(id, ct);
        return role is null
            ? OperationResult<RoleResponse>.Fail("Role not found.")
            : OperationResult<RoleResponse>.Ok(role);
    }

    [HttpGet("permissions")]
    public OperationResult<List<AvailablePermissionResponse>> GetPermissions()
    {
        return OperationResult<List<AvailablePermissionResponse>>.Ok(_roleManager.GetAvailablePermissions());
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.ActionsRoleManage)]
    public async Task<OperationResult<RoleResponse>> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        return await _roleManager.CreateRoleAsync(request, ct);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionKeys.ActionsRoleManage)]
    public async Task<OperationResult<RoleResponse>> Update(int id, [FromBody] UpdateRoleRequest request, CancellationToken ct)
    {
        return await _roleManager.UpdateRoleAsync(id, request, ct);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionKeys.ActionsRoleManage)]
    public async Task<OperationResult<string>> Delete(int id, CancellationToken ct)
    {
        return await _roleManager.DeleteRoleAsync(id, ct);
    }
}
