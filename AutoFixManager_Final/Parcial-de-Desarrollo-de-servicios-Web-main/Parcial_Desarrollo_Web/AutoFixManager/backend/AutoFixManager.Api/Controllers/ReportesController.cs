using AutoFixManager.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFixManager.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/reportes")]
public sealed class ReportesController(IReporteService reporteService) : ControllerBase
{
    [HttpGet("servicios/estado")]
    public IActionResult ResumenServiciosPorEstado() =>
        Ok(reporteService.GetResumenServiciosPorEstado());
}
