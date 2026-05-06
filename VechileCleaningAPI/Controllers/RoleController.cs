using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VechileCleaningAPI.Common;
using VechileCleaningAPI.Dtos;
using VechileCleaningAPI.Managers;

namespace VechileCleaningAPI.Controllers;

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

    // GET /api/roles — all roles with permissions
    [HttpGet]
    [Authorize(Policy = "actions.role.manage")]
    public async Task<ActionResult<OperationResult<List<RoleDto>>>> GetAll()
    {
        var roles = await _roleManager.GetAllRolesAsync();
        return Ok(OperationResult<List<RoleDto>>.Ok(roles));
    }

    // GET /api/roles/permissions — available permission key list
    [HttpGet("permissions")]
    public ActionResult<OperationResult<List<AvailablePermissionDto>>> GetPermissions()
    {
        var permissions = _roleManager.GetAvailablePermissions();
        return Ok(OperationResult<List<AvailablePermissionDto>>.Ok(permissions));
    }

    // POST /api/roles — create role
    [HttpPost]
    [Authorize(Policy = "actions.role.manage")]
    public async Task<ActionResult<OperationResult<RoleDto>>> Create(CreateRoleDto dto)
    {
        var result = await _roleManager.CreateRoleAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    // PUT /api/roles/{id} — update role
    [HttpPut("{id}")]
    [Authorize(Policy = "actions.role.manage")]
    public async Task<ActionResult<OperationResult<RoleDto>>> Update(int id, UpdateRoleDto dto)
    {
        var result = await _roleManager.UpdateRoleAsync(id, dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    // DELETE /api/roles/{id} — delete role
    [HttpDelete("{id}")]
    [Authorize(Policy = "actions.role.manage")]
    public async Task<ActionResult<OperationResult<string>>> Delete(int id)
    {
        var result = await _roleManager.DeleteRoleAsync(id);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
