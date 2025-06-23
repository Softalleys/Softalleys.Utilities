using Softalleys.Utilities.GeoToolkit.Models;

namespace Softalleys.Utilities.GeoToolkit.Interfaces;

/// <summary>
/// Interface for geocoding services that provide forward geocoding, reverse geocoding,
/// and place lookup functionality.
/// </summary>
public interface IGeocodingService
{
    /// <summary>
    /// Performs forward geocoding to find locations based on an address query.
    /// </summary>
    /// <param name="query">The address or place name to search for</param>
    /// <param name="options">Additional options to refine the search</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A collection of location results</returns>
    Task<IEnumerable<GeocodingResult>> SearchAsync(string query, GeocodingOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs reverse geocoding to find addresses and places at a specific geographic coordinate.
    /// </summary>
    /// <param name="latitude">The latitude coordinate</param>
    /// <param name="longitude">The longitude coordinate</param>
    /// <param name="options">Additional options to refine the search</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A collection of location results</returns>
    Task<IEnumerable<GeocodingResult>> ReverseGeocodeAsync(double latitude, double longitude, GeocodingOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Looks up a specific place by its unique identifier.
    /// </summary>
    /// <param name="placeId">The unique identifier of the place to look up</param>
    /// <param name="options">Additional options for the lookup</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The requested place details or null if not found</returns>
    Task<GeocodingResult?> LookupByIdAsync(string placeId, LookupOptions? options = null, CancellationToken cancellationToken = default);
}
