namespace Softalleys.Utilities.GeoToolkit.Models;

/// <summary>
/// Represents a structured address for more precise geocoding queries.
/// </summary>
public class StructuredAddress
{
    /// <summary>
    /// Gets or sets the house number.
    /// </summary>
    public string? HouseNumber { get; set; }

    /// <summary>
    /// Gets or sets the street name.
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// Gets or sets the city, town, or village name.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the county or district name.
    /// </summary>
    public string? County { get; set; }

    /// <summary>
    /// Gets or sets the state or province name.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Gets or sets the country name.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public string? PostalCode { get; set; }
}
