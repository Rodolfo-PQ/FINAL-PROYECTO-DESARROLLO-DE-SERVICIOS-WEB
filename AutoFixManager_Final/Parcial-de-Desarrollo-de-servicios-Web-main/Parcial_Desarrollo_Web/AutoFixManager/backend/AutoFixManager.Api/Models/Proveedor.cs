namespace AutoFixManager.Api.Models;

public sealed class Proveedor
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public bool Activo { get; set; } = true;
}
