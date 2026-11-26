using System.Security.Claims;
using AutoFixManager.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoFixManager.Api.Controllers;

[ApiController]
[Route("api/portal")]
[Authorize(Roles = "CLIENTE")]
public sealed class PortalClienteController(AutoFixDbContext db) : ControllerBase
{
    [HttpGet("mis-vehiculos")]
    public IActionResult Vehiculos()
    {
        if (!TryGetClienteId(out var clienteId))
            return Unauthorized();
        return Ok(db.Vehiculos.AsNoTracking().Where(x => x.ClienteId == clienteId).ToList());
    }

    [HttpGet("mis-servicios")]
    public IActionResult Servicios()
    {
        if (!TryGetClienteId(out var clienteId))
            return Unauthorized();
        return Ok(db.Servicios.AsNoTracking()
            .Where(x => x.ClienteId == clienteId)
            .OrderByDescending(x => x.FechaCreacion)
            .ToList());
    }

    private bool TryGetClienteId(out int clienteId) =>
        int.TryParse(User.FindFirstValue("clienteId"), out clienteId);
}
