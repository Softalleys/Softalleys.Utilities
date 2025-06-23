using System.Text.Json.Serialization;

namespace Softalleys.Utilities.GeoToolkit.Models;

/// <summary>
/// Represents a geocoding result with location and address information.
/// </summary>
public class GeocodingResult
{
    /// <summary>
    /// Gets or sets the unique identifier of the location.
    /// </summary>
    public string? PlaceId { get; set; }

    /// <summary>
    /// Gets or sets the latitude coordinate.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude coordinate.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Gets or sets the fully formatted address.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the address components broken down into structured fields.
    /// </summary>
    public AddressComponent? Address { get; set; }

    /// <summary>
    /// Gets or sets the bounding box coordinates of the result, if available.
    /// Format: [south, north, west, east]
    /// </summary>
    public double[]? BoundingBox { get; set; }

    /// <summary>
    /// Gets or sets the OSM type (Node, Way, Relation).
    /// </summary>
    [JsonPropertyName("osm_type")]
    public string? OsmType { get; set; }

    /// <summary>
    /// Gets or sets the OSM ID.
    /// </summary>
    [JsonPropertyName("osm_id")]
    public string? OsmId { get; set; }

    /// <summary>
    /// Gets or sets the type of the result (e.g., "city", "street", "building").
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the importance rank of the result (0 to 1).
    /// </summary>
    public double? Importance { get; set; }

    /// <summary>
    /// Gets or sets the source provider that returned this result.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Gets or sets the license information for the data.
    /// </summary>
    public string? License { get; set; }

    /// <summary>
    /// Gets or sets additional provider-specific data.
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }
}
