using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoFixManager.Api.Data;
using AutoFixManager.Api.Interfaces;
using AutoFixManager.Api.Middleware;
using AutoFixManager.Api.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AutoFixManager.Api.Services;

public sealed class TokenService(AutoFixDbContext db, IConfiguration configuration) : ITokenService
{
    public TokenPair Issue(Usuario usuario)
    {
        var refresh = CreateRefreshToken(usuario.Id);
        db.RefreshTokens.Add(refresh.Registro);
        db.SaveChanges();
        return CreatePair(usuario, refresh.PlainText, refresh.Registro.ExpiraEn);
    }

    public TokenPair Rotate(string refreshToken)
    {
        var tokenHash = Hash(refreshToken);
        var stored = db.RefreshTokens.Include(x => x.Usuario)
            .AsNoTracking()
            .FirstOrDefault(x => x.TokenHash == tokenHash);
        var now = DateTimeOffset.UtcNow;
        if (stored is null || stored.RevocadoEn.HasValue || stored.ExpiraEn <= now)
            throw new ApiException(401, "El refresh token no es válido o expiró.");

        var next = CreateRefreshToken(stored.UsuarioId);
        using var transaction = db.Database.BeginTransaction();
        var updated = db.RefreshTokens
            .Where(x => x.Id == stored.Id && x.RevocadoEn == null && x.ExpiraEn > now)
            .ExecuteUpdate(setters => setters
                .SetProperty(x => x.RevocadoEn, (DateTimeOffset?)now)
                .SetProperty(x => x.ReemplazadoPorHash, next.Registro.TokenHash));
        if (updated != 1)
            throw new ApiException(401, "El refresh token ya fue utilizado o revocado.");

        db.RefreshTokens.Add(next.Registro);
        db.SaveChanges();
        transaction.Commit();
        return CreatePair(stored.Usuario, next.PlainText, next.Registro.ExpiraEn);
    }

    public void Revoke(string refreshToken)
    {
        var tokenHash = Hash(refreshToken);
        var now = DateTimeOffset.UtcNow;
        db.RefreshTokens
            .Where(x => x.TokenHash == tokenHash && x.RevocadoEn == null)
            .ExecuteUpdate(setters => setters.SetProperty(x => x.RevocadoEn, (DateTimeOffset?)now));
    }

    private TokenPair CreatePair(Usuario usuario, string refreshToken, DateTimeOffset refreshExpiresAt)
    {
        var keyText = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Falta configurar Jwt:Key.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(configuration.GetValue("Jwt:AccessTokenMinutes", 15));
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(ClaimTypes.Role, usuario.Rol)
        };
        if (usuario.ClienteId.HasValue)
            claims.Add(new Claim("clienteId", usuario.ClienteId.Value.ToString()));

        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new TokenPair(new JwtSecurityTokenHandler().WriteToken(token), expiresAt, refreshToken, refreshExpiresAt);
    }

    private (string PlainText, RefreshTokenRegistro Registro) CreateRefreshToken(int usuarioId)
    {
        var plainText = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
        var expiresAt = DateTimeOffset.UtcNow.AddDays(configuration.GetValue("Jwt:RefreshTokenDays", 7));
        var registro = new RefreshTokenRegistro
        {
            UsuarioId = usuarioId,
            TokenHash = Hash(plainText),
            CreadoEn = DateTimeOffset.UtcNow,
            ExpiraEn = expiresAt
        };
        return (plainText, registro);
    }

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
