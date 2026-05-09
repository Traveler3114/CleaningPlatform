// CleaningPlatformAPI/Controllers/ClientController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientController : ControllerBase
{
    private readonly ClientManager _clientManager;

    public ClientController(ClientManager clientManager)
    {
        _clientManager = clientManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? type)
    {
        var clients = await _clientManager.GetAllAsync(search, type);
        return Ok(clients);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = await _clientManager.GetByIdAsync(id);
        if (client == null)
            return NotFound("Client not found.");
        return Ok(client);
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.PagesClients)]
    public async Task<IActionResult> Create([FromBody] CreateClientDto dto)
    {
        var result = await _clientManager.CreateAsync(dto);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result.Data);
    }

    [HttpPut("{id:int}/type")]
    [Authorize(Policy = PermissionKeys.PagesClients)]
    public async Task<IActionResult> UpdateType(int id, [FromBody] UpdateTypeRequest request)
    {
        var result = await _clientManager.UpdateTypeAsync(id, request.NewType);
        if (!result.Success)
            return BadRequest(result.Message);
        return Ok(result.Data);
    }
}

// Simple request DTO (can be in the same file or a separate one)
public class UpdateTypeRequest
{
    public string NewType { get; set; } = string.Empty;
}