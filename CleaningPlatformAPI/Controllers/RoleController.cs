using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Common;

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
    public async Task<ActionResult<List<RoleResponse>>> GetAll(CancellationToken ct)
    {
        return Ok(await _roleManager.GetAllRolesAsync(ct));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = PermissionKeys.RolesView)]
    public async Task<ActionResult<RoleResponse>> GetById(int id, CancellationToken ct)
    {
        return Ok(await _roleManager.GetByIdAsync(id, ct));
    }

    [HttpGet("permissions")]
    [Authorize(Policy = PermissionKeys.RolesView)]
    public ActionResult<List<AvailablePermissionResponse>> GetPermissions()
    {
        return Ok(_roleManager.GetAvailablePermissions());
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.RolesManage)]
    public async Task<ActionResult<RoleResponse>> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        return Ok(await _roleManager.CreateRoleAsync(request, ct));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionKeys.RolesManage)]
    public async Task<ActionResult<RoleResponse>> Update(int id, [FromBody] UpdateRoleRequest request, CancellationToken ct)
    {
        return Ok(await _roleManager.UpdateRoleAsync(id, request, ct));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionKeys.RolesManage)]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        await _roleManager.DeleteRoleAsync(id, ct);
        return NoContent();
    }
}