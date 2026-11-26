namespace AutoFixManager.Api.Models;

public sealed class CategoriaRepuesto
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
}
