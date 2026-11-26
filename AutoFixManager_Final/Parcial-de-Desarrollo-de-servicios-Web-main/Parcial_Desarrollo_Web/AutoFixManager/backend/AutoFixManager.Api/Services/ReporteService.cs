using AutoFixManager.Api.Data;
using AutoFixManager.Api.Interfaces;
using AutoFixManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoFixManager.Api.Services;

public sealed class ReporteService(AutoFixDbContext db) : IReporteService
{
    public IReadOnlyList<ServicioResumenEstado> GetResumenServiciosPorEstado() =>
        db.ResumenesServicios
            .FromSqlRaw("CALL sp_ResumenServiciosPorEstado()")
            .AsNoTracking()
            .ToList();
}
