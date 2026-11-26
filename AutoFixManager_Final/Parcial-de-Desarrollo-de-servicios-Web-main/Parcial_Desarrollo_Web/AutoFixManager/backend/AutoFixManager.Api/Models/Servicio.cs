namespace AutoFixManager.Api.Models;

/// <summary>Orden de trabajo solicitada para un vehículo de un cliente.</summary>
public sealed record Servicio
{
    /// <summary>Identificador asignado por la API durante la ejecución actual.</summary>
    public int Id { get; set; }
    /// <summary>Cliente relacionado con la orden.</summary>
    public int ClienteId { get; set; }
    /// <summary>Vehículo sobre el que se realizará el trabajo.</summary>
    public int VehiculoId { get; set; }
    /// <summary>Mecánico asignado a la orden, si aplica.</summary>
    public int? MecanicoId { get; set; }
    public ICollection<ServicioMecanico> MecanicosAsignados { get; set; } = new List<ServicioMecanico>();
    /// <summary>Detalle del trabajo solicitado.</summary>
    public required string Descripcion { get; set; }
    /// <summary>Categoría de mantenimiento.</summary>
    public required string TipoMantenimiento { get; set; }
    /// <summary>Fecha planificada o registrada para el ingreso del vehículo.</summary>
    public DateTime FechaIngreso { get; set; }
    /// <summary>Estimación monetaria; la API impide valores negativos.</summary>
    public decimal CostoEstimado { get; set; }
    /// <summary>Estado operativo de la orden.</summary>
    public required string Estado { get; set; }
    /// <summary>Fecha UTC de creación de la orden.</summary>
    public DateTime FechaCreacion { get; set; }
}
