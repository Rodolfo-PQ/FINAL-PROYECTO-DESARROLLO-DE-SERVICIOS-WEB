using System.Text.Json.Serialization;

namespace AutoFixManager.Api.Models;

public sealed class ServicioMecanico
{
    public int ServicioId { get; set; }
    public int MecanicoId { get; set; }
    public DateTimeOffset FechaAsignacion { get; set; }
    [JsonIgnore]
    public Servicio Servicio { get; set; } = null!;
    [JsonIgnore]
    public Usuario Mecanico { get; set; } = null!;
}
