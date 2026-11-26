namespace AutoFixManager.Api.Models;

public sealed class DetalleOrdenCompra
{
    public int OrdenCompraId { get; set; }
    public int RepuestoId { get; set; }
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
}
