using System.ComponentModel.DataAnnotations;

namespace AutoFixManager.Api.DTOs;

public sealed class VehiculoWriteDto
{
    [Range(1, int.MaxValue)]
    public int ClienteId { get; init; }

    [Required, RegularExpression("^[A-Za-z0-9 -]{5,12}$")]
    public string Placa { get; init; } = string.Empty;

    [Required, StringLength(60, MinimumLength = 2)]
    public string Marca { get; init; } = string.Empty;

    [Required, StringLength(80, MinimumLength = 1)]
    public string Modelo { get; init; } = string.Empty;

    [Range(1900, 2100)]
    public int Anio { get; init; }

    [Required, StringLength(40, MinimumLength = 2)]
    public string Color { get; init; } = string.Empty;
}
