namespace AutoFixManager.Api.Models;

public sealed class ServicioRepuesto
{
    public int ServicioId { get; set; }
    public int RepuestoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
