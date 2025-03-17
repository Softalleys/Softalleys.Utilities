namespace Softalleys.Utilities.Jwt;

/// <summary>
///     Defines constants used with JwtSecurityTokenHandler, particularly for specifying claim types
///     that should be excluded during certain operations.
/// </summary>
internal static class JwtSecurityTokenHandlerConstants
{
    /// <summary>
    ///     An array of claim types that are often excluded from JWT token processing.
    ///     These claims are typically handled specially by JWT security token handlers
    ///     due to their significance in the JWT standard and security implications.
    /// </summary>
    public static readonly string[] ClaimTypesToExclude =
    {
        // Issuer claim, identifies the principal that issued the JWT.
        JsonWebTokenClaimTypes.Iss,

        // Audience claim, identifies the recipients that the JWT is intended for.
        JsonWebTokenClaimTypes.Aud,

        // Expiration time claim, specifies the expiration time on or after which the JWT must not be accepted.
        JsonWebTokenClaimTypes.Exp,

        // Not before claim, specifies the time before which the JWT must not be accepted.
        JsonWebTokenClaimTypes.Nbf,

        // Issued at claim, indicates the time at which the JWT was issued.
        JsonWebTokenClaimTypes.Iat
    };
}