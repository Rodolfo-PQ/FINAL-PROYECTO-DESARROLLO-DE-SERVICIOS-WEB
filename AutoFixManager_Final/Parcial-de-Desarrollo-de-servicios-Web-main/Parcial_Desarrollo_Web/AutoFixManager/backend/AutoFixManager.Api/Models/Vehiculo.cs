namespace AutoFixManager.Api.Models;

/// <summary>Vehículo asociado a un cliente propietario.</summary>
public sealed record Vehiculo
{
    /// <summary>Identificador asignado por la API durante la ejecución actual.</summary>
    public int Id { get; set; }
    /// <summary>Identificador del cliente propietario.</summary>
    public int ClienteId { get; set; }
    /// <summary>Placa normalizada; no puede repetirse en la colección.</summary>
    public required string Placa { get; set; }
    /// <summary>Fabricante del vehículo.</summary>
    public required string Marca { get; set; }
    /// <summary>Modelo comercial del vehículo.</summary>
    public required string Modelo { get; set; }
    /// <summary>Año de fabricación declarado.</summary>
    public int Anio { get; set; }
    /// <summary>Color exterior.</summary>
    public required string Color { get; set; }
}
