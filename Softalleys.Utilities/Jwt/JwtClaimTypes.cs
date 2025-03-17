namespace Softalleys.Utilities.Jwt;

/// <summary>
///     Provides constants for JWT claim types.
/// </summary>
public static class JwtClaimTypes
{
    /// <summary>
    ///     The 'typ' claim represents the type of the JWT.
    /// </summary>
    public const string Type = "typ";

    /// <summary>
    ///     The 'alg' (algorithm) claim identifies the cryptographic algorithm used to secure the JWT.
    ///     It is typically found in the JWT header.
    /// </summary>
    public const string Algorithm = "alg";

    /// <summary>
    ///     The 'idp' claim represents the identity provider that authenticated the end user.
    /// </summary>
    public const string IdentityProvider = "idp";

    /// <summary>
    ///     The 'events' claim represents the events associated with the authentication.
    /// </summary>
    public const string Events = "events";

    /// <summary>
    ///     The 'scope' claim represents the scope of access requested.
    /// </summary>
    public const string Scope = "scope";

    /// <summary>
    ///     The 'requested_claims' claim represents the specific claims requested by the client.
    /// </summary>
    public const string RequestedClaims = "requested_claims";

    /// <summary>
    ///     The 'ip_address' claim represents the IP address of the client that requested the authentication.
    /// </summary>
    public const string IpAddress = "ip_address";

    /// <summary>
    ///     The 'auth_method' claim represents the authentication method used to authenticate the end user.
    /// </summary>
    public const string SuperAdmin = "superadmin";

    /// <summary>
    ///     The 'sub' (subject) claim identifies the principal that is the subject of the JWT.
    ///     Typically used to represent the user or entity the token is about.
    /// </summary>
    public const string Subject = JsonWebTokenClaimTypes.Sub;

    /// <summary>
    ///     The 'sid' (session ID) claim identifies the session to which the JWT is linked.
    ///     Useful for maintaining state between the client and the issuer.
    /// </summary>
    public const string SessionId = JsonWebTokenClaimTypes.Sid;

    /// <summary>
    ///     The 'iss' (issuer) claim identifies the principal that issued the JWT.
    ///     It is typically a URI identifying the issuer.
    /// </summary>
    public const string Issuer = JsonWebTokenClaimTypes.Iss;

    /// <summary>
    ///     The 'nonce' claim provides a string value used to associate a client session with an ID token.
    /// </summary>
    public const string Nonce = JsonWebTokenClaimTypes.Nonce;

    /// <summary>
    ///     The 'aud' (audience) claim identifies the recipients that the JWT is intended for.
    /// </summary>
    public const string Audience = JsonWebTokenClaimTypes.Aud;

    /// <summary>
    ///     The 'jti' (JWT ID) claim provides a unique identifier for the JWT.
    /// </summary>
    public const string JwtId = JsonWebTokenClaimTypes.Jti;

    /// <summary>
    ///     The 'tid' claim represents the tenant ID associated with the JWT.
    /// </summary>
    public const string TenantId = JsonWebTokenClaimTypes.TenantId;

    /// <summary>
    ///     The 'auth_time' claim represents the time when the authentication occurred.
    ///     It is expressed as the number of seconds since Unix epoch.
    /// </summary>
    public const string AuthenticationTime = JsonWebTokenClaimTypes.AuthTime;

    /// <summary>
    ///     The 'client_id' claim represents the identifier for the client that requested the authentication.
    ///     Often used in OAuth 2.0 and OpenID Connect flows.
    /// </summary>
    public const string ClientId = JsonWebTokenClaimTypes.ClientId;

    /// <summary>
    ///     The 'username' claim represents the user's username or account name.
    /// </summary>
    public const string Username = JsonWebTokenClaimTypes.Username;

    /// <summary>
    ///     The 'preferred_username' claim represents the user's preferred username.
    /// </summary>
    public const string PreferredUsername = JsonWebTokenClaimTypes.PreferredUsername;

    /// <summary>
    ///     The 'acr' (Authentication Context Class Reference) claim provides the reference values for the authentication
    ///     context class.
    /// </summary>
    public const string AuthContextClassRef = JsonWebTokenClaimTypes.Acr;

    /// <summary>
    ///     The 'email' claim represents the user's email address.
    /// </summary>
    public const string Email = JsonWebTokenClaimTypes.Email;

    /// <summary>
    ///     The 'email_verified' claim is a boolean that is true if the user's email address has been verified; otherwise, it
    ///     is false.
    /// </summary>
    public const string EmailVerified = JsonWebTokenClaimTypes.EmailVerified;

    /// <summary>
    ///     The 'phone_number' claim represents the user's phone number.
    /// </summary>
    public const string PhoneNumber = JsonWebTokenClaimTypes.PhoneNumber;

    /// <summary>
    ///     The 'phone_number_verified' claim is a boolean that is true if the user's phone number has been verified;
    ///     otherwise, it is false.
    /// </summary>
    public const string PhoneNumberVerified = JsonWebTokenClaimTypes.PhoneNumberVerified;

    /// <summary>
    ///     The 'c_hash' claim is used for the code hash value in OpenID Connect.
    ///     It is a hash of the authorization code issued by the authorization server.
    /// </summary>
    public const string CodeHash = JsonWebTokenClaimTypes.CHash;

    /// <summary>
    ///     The 'at_hash' claim is used for the access token hash value in OpenID Connect.
    ///     It provides validation that the access token is tied to the identity token.
    /// </summary>
    public const string AccessTokenHash = JsonWebTokenClaimTypes.AtHash;

    /// <summary>
    ///     The 'iat' (issued at) claim identifies the time at which the JWT was issued.
    ///     It is expressed as the number of seconds since the Unix epoch.
    ///     This claim can be used to determine the age of the JWT.
    /// </summary>
    public const string IssuedAt = JsonWebTokenClaimTypes.Iat;

    /// <summary>
    ///     The 'nbf' (not before) claim identifies the time before which the JWT must not be accepted for processing.
    ///     It is expressed as the number of seconds since the Unix epoch.
    ///     This claim is used to define the earliest time at which the JWT is considered valid.
    /// </summary>
    public const string NotBefore = JsonWebTokenClaimTypes.Nbf;

    /// <summary>
    ///     The 'exp' (expiration time) claim identifies the expiration time on or after which the JWT must not be accepted for
    ///     processing.
    ///     It is expressed as the number of seconds since the Unix epoch.
    ///     This claim is used to define the maximum lifespan of the JWT.
    /// </summary>
    public const string ExpiresAt = JsonWebTokenClaimTypes.Exp;

    /// <summary>
    /// Indicates whether the user has super administrator privileges.
    /// </summary>
    public const string IsSuperAdmin = "is_super_admin";

    /// <summary>
    /// Represents the given name of the user.
    /// </summary>
    public const string GivenName = "given_name";

    /// <summary>
    /// Represents the family (last) name of the user.
    /// </summary>
    public const string FamilyName = "family_name";

    /// <summary>
    /// Represents the full name of the user.
    /// </summary>
    public const string Name = "name";
}