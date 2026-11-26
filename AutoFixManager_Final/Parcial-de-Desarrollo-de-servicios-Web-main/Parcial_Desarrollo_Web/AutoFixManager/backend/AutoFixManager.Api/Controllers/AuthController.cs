using AutoFixManager.Api.Data;
using AutoFixManager.Api.DTOs;
using AutoFixManager.Api.Interfaces;
using AutoFixManager.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoFixManager.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    AutoFixDbContext db,
    IPasswordHasher<Usuario> hasher,
    ITokenService tokenService,
    IConfiguration configuration) : ControllerBase
{
    private const string RefreshCookieName = "autofix_refresh";

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login(LoginDto input)
    {
        var username = input.NombreUsuario?.Trim();
        var password = input.Contrasena;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            return Unauthorized(new { error = "Usuario o contraseña incorrectos." });

        var user = db.Usuarios.FirstOrDefault(x => x.NombreUsuario == username);
        var verification = user is null
            ? PasswordVerificationResult.Failed
            : hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (user is null || verification == PasswordVerificationResult.Failed)
            return Unauthorized(new { error = "Usuario o contraseña incorrectos." });
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, password);
            db.SaveChanges();
        }

        var pair = tokenService.Issue(user);
        SetRefreshCookie(pair);
        return Ok(new
        {
            token = pair.AccessToken,
            expira = pair.AccessExpiresAt,
            usuario = user.NombreUsuario,
            rol = user.Rol
        });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public IActionResult Refresh()
    {
        if (!Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { error = "No hay una sesión renovable." });

        var pair = tokenService.Rotate(refreshToken);
        SetRefreshCookie(pair);
        return Ok(new { token = pair.AccessToken, expira = pair.AccessExpiresAt });
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        if (Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken) &&
            !string.IsNullOrWhiteSpace(refreshToken))
            tokenService.Revoke(refreshToken);

        Response.Cookies.Delete(RefreshCookieName, CookieOptions(DateTimeOffset.UtcNow));
        return NoContent();
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("usuarios")]
    public IActionResult ListarUsuarios() =>
        Ok(db.Usuarios.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new { id = x.Id, usuario = x.NombreUsuario, rol = x.Rol, clienteId = x.ClienteId })
            .ToList());

    [Authorize(Roles = "ADMIN")]
    [HttpPost("usuarios")]
    public IActionResult CrearUsuario(UsuarioCreateDto input)
    {
        var username = input.NombreUsuario.Trim();
        if (username.Length < 4)
            return BadRequest(new { error = "El nombre de usuario debe tener al menos 4 caracteres no vacíos." });

        if (db.Usuarios.Any(x => x.NombreUsuario.ToLower() == username.ToLower()))
            return Conflict(new { error = "El nombre de usuario ya existe." });

        if (input.Rol == "CLIENTE" &&
            (!input.ClienteId.HasValue || !db.Clientes.Any(x => x.Id == input.ClienteId.Value)))
            return BadRequest(new { error = "El usuario CLIENTE debe estar asociado a un cliente existente." });

        if (input.Rol == "CLIENTE" && db.Usuarios.Any(x => x.ClienteId == input.ClienteId))
            return Conflict(new { error = "El cliente ya tiene una cuenta de acceso." });

        if (input.Rol != "CLIENTE" && input.ClienteId.HasValue)
            return BadRequest(new { error = "Solo el rol CLIENTE puede tener ClienteId." });

        var user = new Usuario
        {
            NombreUsuario = username,
            Rol = input.Rol,
            ClienteId = input.ClienteId
        };
        user.PasswordHash = hasher.HashPassword(user, input.Contrasena);
        db.Usuarios.Add(user);
        db.SaveChanges();

        return Created($"/api/auth/usuarios/{user.Id}", new
        {
            id = user.Id,
            usuario = user.NombreUsuario,
            rol = user.Rol,
            clienteId = user.ClienteId
        });
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("usuarios/{id:int}")]
    public IActionResult EliminarUsuario(int id)
    {
        var user = db.Usuarios.Find(id);
        if (user is null)
            return NotFound(new { error = $"No existe el usuario {id}." });
        if (user.Rol == "ADMIN")
            return Conflict(new { error = "No se pueden eliminar cuentas ADMIN desde esta ruta." });
        if (db.Servicios.Any(x => x.MecanicoId == id) ||
            db.ServiciosMecanicos.Any(x => x.MecanicoId == id))
            return Conflict(new { error = "No se puede eliminar el mecánico mientras tenga órdenes asignadas." });

        db.Usuarios.Remove(user);
        db.SaveChanges();
        return NoContent();
    }

    private void SetRefreshCookie(TokenPair pair) =>
        Response.Cookies.Append(RefreshCookieName, pair.RefreshToken, CookieOptions(pair.RefreshExpiresAt));

    private CookieOptions CookieOptions(DateTimeOffset expiresAt) => new()
    {
        HttpOnly = true,
        Secure = configuration.GetValue<bool>("Auth:SecureRefreshCookie"),
        SameSite = SameSiteMode.Lax,
        Path = "/api/auth",
        Expires = expiresAt,
        IsEssential = true
    };
}
