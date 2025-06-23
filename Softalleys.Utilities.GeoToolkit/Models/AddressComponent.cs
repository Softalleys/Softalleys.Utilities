using System.Text.Json.Serialization;

namespace Softalleys.Utilities.GeoToolkit.Models;

/// <summary>
/// Represents the components of an address broken down into standardized fields.
/// </summary>
public record AddressComponent
{
    /// <summary>
    /// Gets or sets the house number.
    /// </summary>
    [JsonPropertyName("house_number")]
    public string? HouseNumber { get; set; }

    /// <summary>
    /// Gets or sets the street name.
    /// </summary>
    public string? Road { get; set; }

    /// <summary>
    /// Gets or sets the neighborhood or suburb name.
    /// </summary>
    public string? Suburb { get; set; }

    /// <summary>
    /// Gets or sets the city, town, or village name.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the municipality name.
    /// </summary>
    public string? Municipality { get; set; }

    /// <summary>
    /// Gets or sets the county or district name.
    /// </summary>
    public string? County { get; set; }

    /// <summary>
    /// Gets or sets the state or province name.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    [JsonPropertyName("postcode")]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the country name.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets the country code (ISO 3166-1 alpha-2).
    /// </summary>
    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    /// <summary>
    /// Gets or sets additional address fields that don't fit into standard properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalFields { get; set; }
}
