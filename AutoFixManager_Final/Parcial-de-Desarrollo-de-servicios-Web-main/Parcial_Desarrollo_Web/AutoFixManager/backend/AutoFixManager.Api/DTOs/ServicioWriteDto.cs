using System.ComponentModel.DataAnnotations;

namespace AutoFixManager.Api.DTOs;

public sealed class ServicioWriteDto
{
    [Range(1, int.MaxValue)]
    public int ClienteId { get; init; }

    [Range(1, int.MaxValue)]
    public int VehiculoId { get; init; }

    public int? MecanicoId { get; init; }

    [MaxLength(10)]
    public List<int>? MecanicoIds { get; init; }

    [Required, StringLength(500, MinimumLength = 5)]
    public string Descripcion { get; init; } = string.Empty;

    [Required, StringLength(80, MinimumLength = 2)]
    public string TipoMantenimiento { get; init; } = string.Empty;

    [Required]
    public DateTime? FechaIngreso { get; init; }

    [Range(typeof(decimal), "0", "100000000")]
    public decimal CostoEstimado { get; init; }

    [Required, RegularExpression("^(Programado|En proceso|Finalizado|Cancelado)$")]
    public string Estado { get; init; } = string.Empty;
}
