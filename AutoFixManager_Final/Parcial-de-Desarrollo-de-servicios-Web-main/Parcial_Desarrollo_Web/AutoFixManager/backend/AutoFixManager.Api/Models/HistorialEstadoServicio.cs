namespace AutoFixManager.Api.Models;

public sealed class HistorialEstadoServicio
{
    public int Id { get; set; }
    public int ServicioId { get; set; }
    public int? UsuarioId { get; set; }
    public string? EstadoAnterior { get; set; }
    public required string EstadoNuevo { get; set; }
    public DateTime FechaCambio { get; set; }
    public string? Comentario { get; set; }
}
