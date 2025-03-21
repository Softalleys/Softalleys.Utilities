﻿using System.Text.Json.Nodes;
using Softalleys.Utilities.Extensions;

namespace Softalleys.Utilities.Jwt;

/// <summary>
///     Represents the payload part of a JSON Web Token (JWT), containing the claims or statements about the subject.
/// </summary>
/// <remarks>
///     The JWT payload is a JSON object that contains the claims transmitted by the token. Standard claims
///     such as issuer, subject, expiration time, and more can be included, as well as additional claims as needed.
///     This class provides a convenient way to work with the payload, allowing for easy access and modification of claims.
/// </remarks>
public class JsonWebTokenPayload
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonWebTokenPayload" /> class with the specified JSON object.
    /// </summary>
    /// <param name="json">The <see cref="JsonObject" /> representing the JWT payload.</param>
    public JsonWebTokenPayload(JsonObject json)
    {
        Json = json;
    }

    /// <summary>
    ///     The underlying JSON object representing the JWT payload.
    /// </summary>
    public JsonObject Json { get; }

    /// <summary>
    ///     Indexer to get or set claim values in the payload using the claim name.
    /// </summary>
    /// <param name="name">The name of the claim.</param>
    /// <returns>The value of the claim if it exists; otherwise, null.</returns>
    public JsonNode? this[string name]
    {
        get => Json[name];
        set => Json.SetProperty(name, value);
    }

    /// <summary>
    ///     The unique identifier of the JWT.
    /// </summary>
    public string? JwtId
    {
        get => Json.GetProperty<string>(JwtClaimTypes.JwtId);
        set => Json.SetProperty(JwtClaimTypes.JwtId, value);
    }

    /// <summary>
    ///     The time at which the JWT was issued, represented as a Unix timestamp.
    /// </summary>
    public DateTimeOffset? IssuedAt
    {
        get => Json.GetUnixTimeSeconds(JwtClaimTypes.IssuedAt);
        set => Json.SetUnixTimeSeconds(JwtClaimTypes.IssuedAt, value);
    }

    /// <summary>
    ///     The time before which the JWT must not be accepted for processing, represented as a Unix timestamp.
    /// </summary>
    public DateTimeOffset? NotBefore
    {
        get => Json.GetUnixTimeSeconds(JwtClaimTypes.NotBefore);
        set => Json.SetUnixTimeSeconds(JwtClaimTypes.NotBefore, value);
    }

    /// <summary>
    ///     The expiration time on or after which the JWT must not be accepted for processing, represented as a Unix timestamp.
    /// </summary>
    public DateTimeOffset? ExpiresAt
    {
        get => Json.GetUnixTimeSeconds(JwtClaimTypes.ExpiresAt);
        set => Json.SetUnixTimeSeconds(JwtClaimTypes.ExpiresAt, value);
    }

    /// <summary>
    ///     The issuer of the JWT.
    /// </summary>
    public string? Issuer
    {
        get => Json.GetProperty<string>(JwtClaimTypes.Issuer);
        set => Json.SetProperty(JwtClaimTypes.Issuer, value);
    }

    /// <summary>
    ///     The intended audiences for the JWT.
    /// </summary>
    public IEnumerable<string> Audiences
    {
        get => Json.GetArrayOfStrings(JwtClaimTypes.Audience);
        set => Json.SetArrayOrString(JwtClaimTypes.Audience, value);
    }

    /// <summary>
    ///     Gets or sets the subject of the JWT.
    ///     The subject typically represents the principal that is the focus of the JWT, often a user identifier.
    /// </summary>
    /// <remarks>
    ///     The 'sub' (subject) claim is a standard claim in JWTs used to uniquely identify the principal,
    ///     usually in the context of authentication or user identity. It is commonly a user ID or username.
    /// </remarks>
    public string? Subject
    {
        get => Json.GetProperty<string>(JwtClaimTypes.Subject);
        set => Json.SetProperty(JwtClaimTypes.Subject, value);
    }

    /// <summary>
    ///     The session ID associated with the JWT, typically used to manage session state across applications.
    /// </summary>
    /// <remarks>
    ///     The session ID can link the JWT to a specific session for the user, allowing for effective session management and
    ///     security controls.
    /// </remarks>
    public string? SessionId
    {
        get => Json.GetProperty<string>(JwtClaimTypes.SessionId);
        set => Json.SetProperty(JwtClaimTypes.SessionId, value);
    }


    /// <summary>
    /// Gets or sets the tenant identifier associated with the JWT.
    /// </summary>
    /// <value>
    /// The tenant identifier.
    /// </value>
    public string? TenantId
    {
        get => Json.GetProperty<string>(JwtClaimTypes.TenantId);
        set => Json.SetProperty(JwtClaimTypes.TenantId, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user is a super administrator.
    /// </summary>
    /// <value>
    /// <c>true</c> if the user is a super administrator; otherwise, <c>false</c>.
    /// </value>
    public bool? IsSuperAdmin
    {
        get => Json.GetProperty<bool>(JwtClaimTypes.IsSuperAdmin);
        set => Json.SetProperty(JwtClaimTypes.IsSuperAdmin, value);
    }

    /// <summary>
    ///     The client ID for which the JWT was issued, identifying the client application in OAuth 2.0 and OpenID Connect
    ///     flows.
    /// </summary>
    /// <remarks>
    ///     This property is crucial in scenarios where the JWT is used to convey or assert the identity of a client
    ///     application to the authorization server or resource server.
    /// </remarks>
    public string? ClientId
    {
        get => Json.GetProperty<string>(JwtClaimTypes.ClientId);
        set => Json.SetProperty(JwtClaimTypes.ClientId, value);
    }

    /// <summary>
    ///     The scope of access granted by the JWT.
    ///     Scope is typically a space-separated list of permissions or access levels and is not part of the standard JWT
    ///     claims.
    /// </summary>
    /// <remarks>
    ///     The 'scope' claim is often used in OAuth 2.0 and OpenID Connect contexts to specify the extent of access
    ///     granted by the token. Each value in the list represents a specific permission or access level granted to the token
    ///     bearer.
    ///     This property ensures that the scope is represented appropriately as either a single value or an array of values.
    /// </remarks>
    public IEnumerable<string> Scope
    {
        get => Json.GetSpaceSeparatedStrings(JwtClaimTypes.Scope);
        set => Json.SetSpaceSeparatedStrings(JwtClaimTypes.Scope, value);
    }

    /// <summary>
    ///     Identifies the identity provider that authenticated the end user, useful in federated identity scenarios.
    /// </summary>
    /// <remarks>
    ///     This claim is particularly relevant in systems that support multiple identity providers,
    ///     helping to trace the origin of the authentication and ensuring that the JWT can be validated appropriately.
    /// </remarks>
    public string? IdentityProvider
    {
        get => Json.GetProperty<string>(JwtClaimTypes.IdentityProvider);
        set => Json.SetProperty(JwtClaimTypes.IdentityProvider, value);
    }

    /// <summary>
    ///     Represents the time when the authentication occurred, facilitating checks against token freshness
    ///     and replay attacks.
    /// </summary>
    /// <remarks>
    ///     Storing the authentication time is critical for applications requiring a high level of assurance
    ///     regarding the moment a user was authenticated, allowing for precise control over session validity
    ///     and user authentication status.
    /// </remarks>
    public DateTimeOffset? AuthenticationTime
    {
        get => Json.GetUnixTimeSeconds(JwtClaimTypes.AuthenticationTime);
        set => Json.SetUnixTimeSeconds(JwtClaimTypes.AuthenticationTime, value);
    }

    /// <summary>
    ///     A value used to associate a client session with an ID token, mitigating replay attacks.
    /// </summary>
    public string? Nonce
    {
        get => Json.GetProperty<string>(JwtClaimTypes.Nonce);
        set => Json.SetProperty(JwtClaimTypes.Nonce, value);
    }

    /// <summary>
    ///     Gets or sets the username from the JWT claims.
    ///     Represents the user name provided in the token payload.
    /// </summary>
    public string? Username
    {
        get => Json.GetProperty<string>(JwtClaimTypes.Username);
        set => Json.SetProperty(JwtClaimTypes.Username, value);
    }

    /// <summary>
    ///     Gets or sets the preferred username from the JWT claims.
    ///     This value is typically used for display purposes if different from the username.
    /// </summary>
    public string? PreferredUsername
    {
        get => Json.GetProperty<string>(JwtClaimTypes.PreferredUsername);
        set => Json.SetProperty(JwtClaimTypes.PreferredUsername, value);
    }

    /// <summary>
    ///     Gets or sets the given name from the JWT claims.
    ///     Represents the first name of the user.
    /// </summary>
    public string? GivenName
    {
        get => Json.GetProperty<string>(JwtClaimTypes.GivenName);
        set => Json.SetProperty(JwtClaimTypes.GivenName, value);
    }

    /// <summary>
    ///     Converts a collection of JwtClaims into a dictionary suitable for a JWT payload.
    /// </summary>
    /// <returns>A dictionary of claims where the key is the claim type and the value is the claim value.</returns>
    public IDictionary<string, object> ToDictionary()
    {
        var uniqueClaims = Json
            .ExceptBy(
                JwtSecurityTokenHandlerConstants.ClaimTypesToExclude,
                claim => claim.Key)
            .GroupBy(
                claim => claim.Key,
                claim => claim.Value.ToJsonElement());

        var result = uniqueClaims.ToDictionary(
            claim => claim.Key,
            claim => claim.Count() == 1 ? (object)claim.Single() : claim.ToArray());

        return result;
    }
}