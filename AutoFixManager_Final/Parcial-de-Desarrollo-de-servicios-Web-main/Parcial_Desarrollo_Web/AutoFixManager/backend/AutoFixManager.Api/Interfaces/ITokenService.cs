using AutoFixManager.Api.Models;

namespace AutoFixManager.Api.Interfaces;

public interface ITokenService
{
    TokenPair Issue(Usuario usuario);
    TokenPair Rotate(string refreshToken);
    void Revoke(string refreshToken);
}

public sealed record TokenPair(string AccessToken, DateTimeOffset AccessExpiresAt, string RefreshToken, DateTimeOffset RefreshExpiresAt);
