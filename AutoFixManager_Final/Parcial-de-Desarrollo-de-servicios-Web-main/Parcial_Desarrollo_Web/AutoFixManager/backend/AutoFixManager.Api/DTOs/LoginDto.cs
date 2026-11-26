using System.ComponentModel.DataAnnotations;
namespace AutoFixManager.Api.DTOs;
public sealed class LoginDto { [Required] public string NombreUsuario { get; init; } = ""; [Required] public string Contrasena { get; init; } = ""; }
public sealed class UsuarioCreateDto { [Required, StringLength(80, MinimumLength=4)] public string NombreUsuario { get; init; } = ""; [Required, MinLength(8)] public string Contrasena { get; init; } = ""; [Required, RegularExpression("^(ADMIN|MECANICO|CLIENTE)$")] public string Rol { get; init; } = "CLIENTE"; public int? ClienteId { get; init; } }
