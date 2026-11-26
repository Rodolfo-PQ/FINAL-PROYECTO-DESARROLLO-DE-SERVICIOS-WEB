namespace AutoFixManager.Api.Models;

public sealed class RefreshTokenRegistro
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public required string TokenHash { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
    public DateTimeOffset ExpiraEn { get; set; }
    public DateTimeOffset? RevocadoEn { get; set; }
    public string? ReemplazadoPorHash { get; set; }
    public Usuario Usuario { get; set; } = null!;
}
