namespace AutoFixManager.Api.Models;

public sealed class MovimientoInventario
{
    public int Id { get; set; }
    public int RepuestoId { get; set; }
    public int? UsuarioId { get; set; }
    public required string TipoMovimiento { get; set; }
    public int Cantidad { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public string? Referencia { get; set; }
}
