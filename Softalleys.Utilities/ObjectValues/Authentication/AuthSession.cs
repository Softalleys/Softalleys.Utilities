namespace Softalleys.Utilities.ObjectValues.Authentication;

/// <summary>
/// Represents a model of an authentication session for a logged-in user, capturing essential details about the user's
/// authentication state and interactions within the system.
/// </summary>
/// <param name="Subject">The unique identifier for the user in the session.</param>
/// <param name="SessionId">The unique identifier of the session.</param>
/// <param name="Issuer">The entity that issued the authentication token.</param>
/// <param name="Audience">The intended recipient of the authentication token.</param>
/// <param name="AuthenticationTime">The timestamp indicating when the user was authenticated.</param>
/// <param name="ExpireAt">The timestamp indicating when the authentication session will expire.</param>
public record AuthSession(
    Guid Subject,
    string SessionId,
    string Issuer,
    string Audience,
    DateTimeOffset AuthenticationTime,
    DateTimeOffset ExpireAt)
{
    /// <summary>
    /// The unique identifier for the user in the session. This is typically used to retrieve user details or confirm
    /// the user's identity across the application.
    /// </summary>
    public Guid Subject { get; init; } = Subject;

    /// <summary>
    /// The unique identifier for the tenant in which the session is active.
    /// Useful for ensuring sessions are scoped to a specific tenant in multi-tenant applications.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// The unique identifier of the session.
    /// Used to track the session across requests and different services.
    /// </summary>
    public string SessionId { get; init; } = SessionId;

    /// <summary>
    /// The timestamp indicating when the user was authenticated.
    /// Important for session expiration and activity logging.
    /// </summary>
    public DateTimeOffset AuthenticationTime { get; init; } = AuthenticationTime;

    /// <summary>
    /// The timestamp indicating when the user was authenticated. Used for session management and activity logging.
    /// </summary>
    public DateTimeOffset IssuedAt { get; init; } = AuthenticationTime;

    /// <summary>
    /// The provider used to authenticate the user.
    /// Could be a local or external identity provider.
    /// </summary>
    public string? IdentityProvider { get; init; }

    /// <summary>
    /// The authentication context class reference dictating the level of assurance provided by the authentication process.
    /// </summary>
    public string? AuthContextClassRef { get; init; }

    /// <summary>
    /// A collection of client identifiers that the user has interacted with during the session.
    /// Facilitates the management and tracking of user consent across multiple clients.
    /// </summary>
    public ICollection<string> AffectedClientIds { get; init; } = new List<string>();

    /// <summary>
    /// The specific client identifier associated with the session.
    /// </summary>
    public string? ClientId { get; init; }

    /// <summary>
    /// The preferred username of the user.
    /// </summary>
    public string PreferredUsername { get; init; } = string.Empty;

    /// <summary>
    /// The given name of the user.
    /// </summary>
    public string GivenName { get; init; } = string.Empty;

    /// <summary>
    /// The username of the user.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// The email address of the user.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The locale of the user, defaulting to \`es-MX\`.
    /// </summary>
    public string Locale { get; set; } = "es-MX";

    /// <summary>
    /// The time zone associated with the user, defaulting to \`America/Mexico_City\`.
    /// </summary>
    public string TimeZone { get; set; } = "America/Mexico_City";

    /// <summary>
    /// The IP address from which the session was initiated.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// The set of scopes granted to the user for this session.
    /// Scopes define the permissions or access rights the user has within the application.
    /// </summary>
    public IList<string> Scopes { get; init; } = new List<string>();

    /// <summary>
    /// The set of roles assigned to the user for this session.
    /// Roles represent the user's responsibilities or access levels within the system.
    /// </summary>
    public IList<string> Roles { get; init; } = new List<string>();
}