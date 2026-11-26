using System.Text.Json.Serialization;

namespace AutoFixManager.Api.Models;
public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "CLIENTE";
    public int? ClienteId { get; set; }
    [JsonIgnore]
    public ICollection<ServicioMecanico> ServiciosAsignados { get; set; } = new List<ServicioMecanico>();
    [JsonIgnore]
    public ICollection<RefreshTokenRegistro> RefreshTokens { get; set; } = new List<RefreshTokenRegistro>();
}
