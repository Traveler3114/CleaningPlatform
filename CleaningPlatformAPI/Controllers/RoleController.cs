using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
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
    public async Task<OperationResult<List<RoleDto>>> GetAll()
    {
        var roles = await _roleManager.GetAllRolesAsync();
        return OperationResult<List<RoleDto>>.Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<OperationResult<RoleDto>> GetById(int id)
    {
        var roles = await _roleManager.GetAllRolesAsync();
        var role = roles.FirstOrDefault(r => r.Id == id);
        return role is null
            ? OperationResult<RoleDto>.Fail("Role not found.")
            : OperationResult<RoleDto>.Ok(role);
    }

    [HttpGet("permissions")]
    public OperationResult<List<AvailablePermissionDto>> GetPermissions()
    {
        var permissions = _roleManager.GetAvailablePermissions();
        return OperationResult<List<AvailablePermissionDto>>.Ok(permissions);
    }

    [HttpPost]
    [Authorize(Policy = "actions.role.manage")]
    public async Task<OperationResult<RoleDto>> Create(CreateRoleDto dto)
    {
        return await _roleManager.CreateRoleAsync(dto);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "actions.role.manage")]
    public async Task<OperationResult<RoleDto>> Update(int id, UpdateRoleDto dto)
    {
        return await _roleManager.UpdateRoleAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "actions.role.manage")]
    public async Task<OperationResult<string>> Delete(int id)
    {
        return await _roleManager.DeleteRoleAsync(id);
    }
}