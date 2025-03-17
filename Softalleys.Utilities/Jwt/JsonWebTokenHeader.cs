using System.Text.Json.Nodes;
using Softalleys.Utilities.Extensions;

namespace Softalleys.Utilities.Jwt;

/// <summary>
/// Represents the header segment of a JSON Web Token (JWT).
/// The header typically contains metadata about the token such as token type and the signing algorithm.
/// </summary>
public class JsonWebTokenHeader
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonWebTokenHeader"/> class with the specified JSON object.
    /// </summary>
    /// <param name="json">The <see cref="JsonObject"/> representing the JWT header.</param>
    public JsonWebTokenHeader(JsonObject json)
    {
        // Initialize the underlying JSON object for the header.
        Json = json;
    }

    /// <summary>
    /// Gets the underlying JSON object representing the JWT header.
    /// </summary>
    public JsonObject Json { get; }

    /// <summary>
    /// Gets or sets the type of the JWT, typically "JWT" or a similar identifier.
    /// This field is optional and may be used to declare the media type of the JWT.
    /// </summary>
    /// <remarks>
    /// The 'typ' parameter is recommended when the JWT is embedded in contexts
    /// where the token type is not inherently obvious, helping recipients process the token accordingly.
    /// </remarks>
    public string? Type
    {
        get => Json.GetProperty<string>(JwtClaimTypes.Type);
        set => Json.SetProperty(JwtClaimTypes.Type, value);
    }

    /// <summary>
    /// Gets or sets the algorithm used to sign the JWT.
    /// This indicates how the token is secured.
    /// </summary>
    /// <remarks>
    /// The 'alg' parameter identifies the cryptographic algorithm used to secure the JWT.
    /// Common values include HS256, RS256, and ES256, and it is crucial for verifying the integrity of the token.
    /// </remarks>
    public string? Algorithm
    {
        get => Json.GetProperty<string>(JwtClaimTypes.Algorithm);
        set => Json.SetProperty(JwtClaimTypes.Algorithm, value);
    }
}