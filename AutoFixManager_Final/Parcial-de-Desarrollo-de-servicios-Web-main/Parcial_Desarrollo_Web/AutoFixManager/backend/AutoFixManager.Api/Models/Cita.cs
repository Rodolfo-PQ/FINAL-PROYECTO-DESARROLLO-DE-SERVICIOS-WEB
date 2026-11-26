namespace AutoFixManager.Api.Models;

public sealed class Cita
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int VehiculoId { get; set; }
    public int? ServicioId { get; set; }
    public DateTime FechaCita { get; set; }
    public required string Motivo { get; set; }
    public required string Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
}
