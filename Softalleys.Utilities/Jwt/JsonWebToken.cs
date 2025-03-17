using System.Text.Json.Nodes;
using Microsoft.IdentityModel.Tokens;
using Softalleys.Utilities.Extensions;

namespace Softalleys.Utilities.Jwt;

/// <summary>
/// Represents a JSON Web Token composed of a header and a payload.
/// </summary>
public record JsonWebToken
{
    /// <summary>
    /// Gets or initializes the header part of the JWT.
    /// </summary>
    public JsonWebTokenHeader Header { get; init; } = new(new JsonObject());

    /// <summary>
    /// Gets or initializes the payload part of the JWT.
    /// </summary>
    public JsonWebTokenPayload Payload { get; init; } = new(new JsonObject());

    /// <summary>
    /// Converts the JSON Web Token into a <see cref="SecurityTokenDescriptor"/> instance.
    /// </summary>
    /// <returns>
    /// A <see cref="SecurityTokenDescriptor"/> that represents the details of the JWT, including token type,
    /// issuer, audience, timing constraints, and claims.
    /// </returns>
    public SecurityTokenDescriptor ToSecurityTokenDescriptor()
    {
        return new SecurityTokenDescriptor
        {
            TokenType = Header.Type,
            Issuer = Payload.Issuer,
            Audience = Payload.Audiences.SingleOrDefault(),

            IssuedAt = Payload.IssuedAt.CheckDateOverflow(nameof(Payload.IssuedAt)),
            NotBefore = Payload.NotBefore.CheckDateOverflow(nameof(Payload.NotBefore)),
            Expires = Payload.ExpiresAt.CheckDateOverflow(nameof(Payload.ExpiresAt)),

            Claims = Payload.ToDictionary()
        };
    }
}