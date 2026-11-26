using AutoFixManager.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFixManager.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/respaldos")]
public sealed class RespaldosController(ISqliteBackupService backupService) : ControllerBase
{
    [HttpPost("sqlite")]
    public async Task<IActionResult> CrearRespaldoSqlite(CancellationToken cancellationToken)
    {
        var result = await backupService.CreateBackupAsync(cancellationToken);
        return Ok(result);
    }
}
