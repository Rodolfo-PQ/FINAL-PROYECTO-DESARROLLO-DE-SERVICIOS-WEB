namespace AutoFixManager.Api.Models;

/// <summary>Representa a la persona que solicita servicios al taller.</summary>
public sealed record Cliente
{
    /// <summary>Identificador asignado por la API durante la ejecución actual.</summary>
    public int Id { get; set; }
    /// <summary>Nombre del cliente.</summary>
    public required string Nombre { get; set; }
    /// <summary>Apellido del cliente.</summary>
    public required string Apellido { get; set; }
    /// <summary>Correo de contacto, único sin distinguir mayúsculas.</summary>
    public required string Email { get; set; }
    /// <summary>Teléfono de contacto.</summary>
    public required string Telefono { get; set; }
    /// <summary>Fecha UTC en la que el cliente se registró.</summary>
    public DateTime FechaRegistro { get; set; }
}
