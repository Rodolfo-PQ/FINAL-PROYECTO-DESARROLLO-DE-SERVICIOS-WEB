using AutoFixManager.Api.Models;

namespace AutoFixManager.Api.Interfaces;

public interface IReporteService
{
    IReadOnlyList<ServicioResumenEstado> GetResumenServiciosPorEstado();
}
