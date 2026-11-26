namespace AutoFixManager.Api.Models;

public sealed class ServicioResumenEstado
{
    public required string Estado { get; set; }
    public long Cantidad { get; set; }
    public decimal CostoTotal { get; set; }
}
