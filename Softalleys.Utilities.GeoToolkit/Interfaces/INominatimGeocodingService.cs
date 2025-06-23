using Softalleys.Utilities.GeoToolkit.Models;

namespace Softalleys.Utilities.GeoToolkit.Interfaces;

/// <summary>
/// Nominatim-specific geocoding service interface that extends the generic IGeocodingService.
/// </summary>
public interface INominatimGeocodingService : IGeocodingService
{
    /// <summary>
    /// Performs a structured address search using Nominatim's specific parameters.
    /// </summary>
    /// <param name="structuredAddress">The address components in a structured format</param>
    /// <param name="options">Additional options to refine the search</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A collection of location results</returns>
    Task<IEnumerable<GeocodingResult>> StructuredSearchAsync(
        StructuredAddress structuredAddress,
        NominatimGeocodingOptions? options = null,
        CancellationToken cancellationToken = default);
}
