using AutoFixManager.Api.Models;

namespace AutoFixManager.Api.Services;

/// <summary>Almacén singleton con los datos que viven durante el proceso actual de la API.</summary>
public sealed class InMemoryStore
{
    /// <summary>Bloqueo compartido para mantener consistentes las listas y sus relaciones.</summary>
    public object SyncRoot { get; } = new();
    /// <summary>Clientes registrados en memoria.</summary>
    public List<Cliente> Clientes { get; } = [];
    /// <summary>Vehículos registrados en memoria.</summary>
    public List<Vehiculo> Vehiculos { get; } = [];
    /// <summary>Órdenes registradas en memoria.</summary>
    public List<Servicio> Servicios { get; } = [];
    /// <summary>Siguiente ID disponible de cliente.</summary>
    public int NextClienteId { get; set; } = 1;
    /// <summary>Siguiente ID disponible de vehículo.</summary>
    public int NextVehiculoId { get; set; } = 1;
    /// <summary>Siguiente ID disponible de servicio.</summary>
    public int NextServicioId { get; set; } = 1;
}
