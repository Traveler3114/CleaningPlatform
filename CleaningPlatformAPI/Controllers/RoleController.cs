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
    public async Task<ActionResult<OperationResult<List<RoleResponse>>>> GetAll(CancellationToken ct)
    {
        var roles = await _roleManager.GetAllRolesAsync(ct);
        return Ok(OperationResult<List<RoleResponse>>.Ok(roles));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = PermissionKeys.RolesView)]
    public async Task<ActionResult<OperationResult<RoleResponse>>> GetById(int id, CancellationToken ct)
    {
        var result = await _roleManager.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("permissions")]
    [Authorize(Policy = PermissionKeys.RolesView)]
    public ActionResult<OperationResult<List<AvailablePermissionResponse>>> GetPermissions()
    {
        var permissions = _roleManager.GetAvailablePermissions();
        return Ok(OperationResult<List<AvailablePermissionResponse>>.Ok(permissions));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.RolesManage)]
    public async Task<ActionResult<OperationResult<RoleResponse>>> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        var result = await _roleManager.CreateRoleAsync(request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionKeys.RolesManage)]
    public async Task<ActionResult<OperationResult<RoleResponse>>> Update(int id, [FromBody] UpdateRoleRequest request, CancellationToken ct)
    {
        var result = await _roleManager.UpdateRoleAsync(id, request, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionKeys.RolesManage)]
    public async Task<ActionResult<OperationResult<string>>> Delete(int id, CancellationToken ct)
    {
        var result = await _roleManager.DeleteRoleAsync(id, ct);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}