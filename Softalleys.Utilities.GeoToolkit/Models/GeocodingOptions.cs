namespace Softalleys.Utilities.GeoToolkit.Models;

/// <summary>
/// Base options class for geocoding operations.
/// </summary>
public record GeocodingOptions
{
    /// <summary>
    /// Gets or sets the maximum number of results to return.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Gets or sets the language code for results (e.g., "en", "es", "fr").
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include a bounding box in the results.
    /// </summary>
    public bool IncludeBoundingBox { get; set; } = true;

    /// <summary>
    /// Gets or sets a bounding box to restrict the search within [south, north, west, east].
    /// </summary>
    public double[]? BoundingBox { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether address details should be included in the results.
    /// </summary>
    public bool IncludeAddressDetails { get; set; } = true;

    /// <summary>
    /// Gets or sets a filter to restrict results to certain categories/types.
    /// </summary>
    public string[]? Categories { get; set; }

    /// <summary>
    /// Gets or sets the maximum distance in meters to search around the coordinate.
    /// </summary>
    public double? Radius { get; set; }

    /// <summary>
    /// Gets or sets the zoom level for the reverse geocoding request.
    /// Lower values return more general results (country, region),
    /// higher values return more specific results (streets, buildings).
    /// </summary>
    public int? ZoomLevel { get; set; }
}
