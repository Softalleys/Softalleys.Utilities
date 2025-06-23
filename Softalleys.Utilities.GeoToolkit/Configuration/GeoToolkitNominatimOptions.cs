namespace Softalleys.Utilities.GeoToolkit.Configuration;

/// <summary>
/// Represents configuration options for the GeoToolkit Nominatim integration.
/// </summary>
public record GeoToolkitNominatimOptions
{
    /// <summary>
    /// Gets or sets the base URL of the Nominatim server.
    /// Defaults to "https://nominatim.openstreetmap.org".
    /// </summary>
    public string NominatimServerUrl { get; set; } = "https://nominatim.openstreetmap.org";

    public bool ShowLogs { get; set; }
}