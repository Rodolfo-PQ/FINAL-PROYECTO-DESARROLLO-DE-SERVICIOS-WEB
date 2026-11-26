namespace AutoFixManager.Api.Models;

public sealed class Repuesto
{
    public int Id { get; set; }
    public int CategoriaRepuestoId { get; set; }
    public required string Codigo { get; set; }
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
}
