namespace Softalleys.Utilities.GeoToolkit.Models;

/// <summary>
/// Nominatim-specific options for geocoding operations.
/// </summary>
public record NominatimGeocodingOptions : GeocodingOptions
{
    /// <summary>
    /// Gets or sets the application name for identifying requests to the Nominatim server.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets an email address for contact in case of problems with the service.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets a country code bias for results (ISO 3166-1 alpha-2 or alpha-3).
    /// </summary>
    public string? CountryCodeBias { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to exclude places that no longer exist.
    /// </summary>
    public bool ExcludeHistorical { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to exclude places that may not be suitable for the public.
    /// </summary>
    public bool ExcludePlacesOfWorship { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to deduplicate results that refer to the same place.
    /// </summary>
    public bool Dedupe { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable debug output.
    /// </summary>
    public bool Debug { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to include Nominatim-specific details in the response.
    /// </summary>
    public bool IncludeExtraTags { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to include details of name matches in the response.
    /// </summary>
    public bool IncludeNameDetails { get; set; } = false;
}
