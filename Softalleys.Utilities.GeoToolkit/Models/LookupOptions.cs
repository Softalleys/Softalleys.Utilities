namespace Softalleys.Utilities.GeoToolkit.Models;

/// <summary>
/// Options for place lookup operations.
/// </summary>
public record LookupOptions : GeocodingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to include detailed attribution information in the results.
    /// </summary>
    public bool IncludeAttribution { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include polygon data in the results, if available.
    /// </summary>
    public bool IncludePolygon { get; set; } = false;

    /// <summary>
    /// Gets or sets the polygon geometry type to return (e.g., "geojson", "kml", "svg").
    /// </summary>
    public string? PolygonFormat { get; set; }
}
