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
    public RoleController(RoleManager roleManager) { _roleManager = roleManager; }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.RolesView)]
    public async Task<OperationResult<List<RoleResponse>>> GetAll(CancellationToken ct)
        => OperationResult<List<RoleResponse>>.Ok(await _roleManager.GetAllRolesAsync(ct));

    [HttpGet("{id}")]
    [Authorize(Policy = PermissionKeys.RolesView)]
    public async Task<OperationResult<RoleResponse>> GetById(int id, CancellationToken ct)
    {
        var role = await _roleManager.GetByIdAsync(id, ct);
        return role is null
            ? OperationResult<RoleResponse>.Fail($"Role #{id} was not found.")
            : OperationResult<RoleResponse>.Ok(role);
    }

    [HttpGet("permissions")]
    [Authorize(Policy = PermissionKeys.RolesView)]
    public OperationResult<List<AvailablePermissionResponse>> GetPermissions()
        => OperationResult<List<AvailablePermissionResponse>>.Ok(_roleManager.GetAvailablePermissions());

    [HttpPost]
    [Authorize(Policy = PermissionKeys.RolesManage)]
    public async Task<OperationResult<RoleResponse>> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
        => await _roleManager.CreateRoleAsync(request, ct);

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionKeys.RolesManage)]
    public async Task<OperationResult<RoleResponse>> Update(int id, [FromBody] UpdateRoleRequest request, CancellationToken ct)
        => await _roleManager.UpdateRoleAsync(id, request, ct);

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionKeys.RolesManage)]
    public async Task<OperationResult<string>> Delete(int id, CancellationToken ct)
        => await _roleManager.DeleteRoleAsync(id, ct);
}
