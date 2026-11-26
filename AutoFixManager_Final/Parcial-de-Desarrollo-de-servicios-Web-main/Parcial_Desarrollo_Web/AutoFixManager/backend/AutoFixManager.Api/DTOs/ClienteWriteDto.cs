using System.ComponentModel.DataAnnotations;

namespace AutoFixManager.Api.DTOs;

public sealed class ClienteWriteDto
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string Nombre { get; init; } = string.Empty;

    [Required, StringLength(80, MinimumLength = 2)]
    public string Apellido { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(30, MinimumLength = 7)]
    public string Telefono { get; init; } = string.Empty;
}