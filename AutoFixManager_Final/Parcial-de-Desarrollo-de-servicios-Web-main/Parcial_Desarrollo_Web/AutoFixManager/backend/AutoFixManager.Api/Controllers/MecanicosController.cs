using System.Security.Claims;
using AutoFixManager.Api.DTOs;
using AutoFixManager.Api.Filters;
using AutoFixManager.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFixManager.Api.Controllers;

[ApiController]
[Authorize(Roles = "MECANICO")]
[Route("api/mecanico")]
public sealed class MecanicosController(IServicioService servicioService) : ControllerBase
{
    [HttpGet("mis-clientes")]
    public IActionResult MisClientes()
    {
        if (!TryGetMecanicoId(out var mecanicoId))
            return Unauthorized();
        return Ok(servicioService.GetClientesByMecanico(mecanicoId));
    }

    [HttpGet("mis-servicios")]
    public IActionResult MisServicios()
    {
        if (!TryGetMecanicoId(out var mecanicoId))
            return Unauthorized();
        return Ok(servicioService.GetByMecanico(mecanicoId));
    }

    [HttpGet("mis-vehiculos")]
    public IActionResult MisVehiculos()
    {
        if (!TryGetMecanicoId(out var mecanicoId))
            return Unauthorized();
        return Ok(servicioService.GetVehiculosByMecanico(mecanicoId));
    }

    [HttpGet("servicios/{id:int}")]
    [RequireMecanicoAsignacion]
    public IActionResult ObtenerServicio(int id)
    {
        if (!TryGetMecanicoId(out var mecanicoId))
            return Unauthorized();
        return Ok(servicioService.GetByIdForMecanico(id, mecanicoId));
    }

    [HttpPost("servicios")]
    public IActionResult RegistrarServicio(ServicioWriteDto input)
    {
        if (!TryGetMecanicoId(out var mecanicoId))
            return Unauthorized();
        var servicio = servicioService.CreateForMecanico(input, mecanicoId);
        return CreatedAtAction(nameof(ObtenerServicio), new { id = servicio.Id }, servicio);
    }

    [HttpPut("servicios/{id:int}")]
    [RequireMecanicoAsignacion]
    public IActionResult ActualizarServicio(int id, ServicioWriteDto input)
    {
        if (!TryGetMecanicoId(out var mecanicoId))
            return Unauthorized();
        return Ok(servicioService.UpdateForMecanico(id, input, mecanicoId));
    }

    private bool TryGetMecanicoId(out int mecanicoId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out mecanicoId);
}
