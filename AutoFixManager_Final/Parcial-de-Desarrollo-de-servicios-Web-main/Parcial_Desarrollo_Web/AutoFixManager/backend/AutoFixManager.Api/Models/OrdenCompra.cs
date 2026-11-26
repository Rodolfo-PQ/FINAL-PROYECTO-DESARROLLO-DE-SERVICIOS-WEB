namespace AutoFixManager.Api.Models;

public sealed class OrdenCompra
{
    public int Id { get; set; }
    public int ProveedorId { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public required string Estado { get; set; }
    public decimal Total { get; set; }
}
