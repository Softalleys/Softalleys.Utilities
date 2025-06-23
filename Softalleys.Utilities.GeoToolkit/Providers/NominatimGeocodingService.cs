using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Softalleys.Utilities.GeoToolkit.Configuration;
using Softalleys.Utilities.GeoToolkit.Interfaces;
using Softalleys.Utilities.GeoToolkit.Models;

namespace Softalleys.Utilities.GeoToolkit.Providers;

public class NominatimGeocodingService(
    ILogger<NominatimGeocodingService> logger,
    IHttpClientFactory httpClientFactory,
    IOptions<GeoToolkitNominatimOptions> options)
    : INominatimGeocodingService
{
    private readonly GeoToolkitNominatimOptions _options = options.Value;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IEnumerable<GeocodingResult>> SearchAsync(string query, GeocodingOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            if (_options.ShowLogs)
                logger.LogWarning("Search query is empty or null");
            throw new ArgumentException("Query cannot be empty", nameof(query));
        }

        var nominatimOptions = options as NominatimGeocodingOptions ?? new NominatimGeocodingOptions();

        var queryParams = new Dictionary<string, string>
        {
            ["q"] = query,
            ["format"] = "json",
            ["addressdetails"] = nominatimOptions.IncludeAddressDetails ? "1" : "0"
        };

        AddCommonParams(queryParams, nominatimOptions);

        var requestUrl = BuildRequestUrl("search", queryParams);

        var results = await MakeRequestAsync<List<NominatimPlace>>(requestUrl, cancellationToken);
        return results.Select(MapToGeocodingResult).ToList();
    }

    public async Task<IEnumerable<GeocodingResult>> ReverseGeocodeAsync(double latitude, double longitude,
        GeocodingOptions? options = null, CancellationToken cancellationToken = default)
    {
        var nominatimOptions = options as NominatimGeocodingOptions ?? new NominatimGeocodingOptions();

        var queryParams = new Dictionary<string, string>
        {
            ["lat"] = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["lon"] = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["format"] = "json",
            ["addressdetails"] = nominatimOptions.IncludeAddressDetails ? "1" : "0"
        };

        // Add zoom level if specified
        if (options?.ZoomLevel.HasValue == true)
        {
            queryParams["zoom"] = options.ZoomLevel.Value.ToString();
        }

        AddCommonParams(queryParams, nominatimOptions);

        var requestUrl = BuildRequestUrl("reverse", queryParams);

        var result = await MakeRequestAsync<NominatimPlace>(requestUrl, cancellationToken);
        return [MapToGeocodingResult(result)];
    }

    public async Task<GeocodingResult?> LookupByIdAsync(string placeId, LookupOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(placeId))
        {
            throw new ArgumentException("Place ID cannot be empty", nameof(placeId));
        }

        // Parse OSM type and ID from placeId format (typically: N12345 where N is node, W is way, R is relation)
        if (placeId.Length < 2)
        {
            throw new ArgumentException("Invalid place ID format", nameof(placeId));
        }

        var osmTypeChar = placeId[0];
        var osmId = placeId.Substring(1);

        var nominatimOptions = options ?? new LookupOptions();

        var queryParams = new Dictionary<string, string>
        {
            ["osm_ids"] = $"{osmTypeChar}{osmId}",
            ["format"] = "json",
            ["addressdetails"] = nominatimOptions.IncludeAddressDetails ? "1" : "0"
        };

        AddCommonParams(queryParams, nominatimOptions);

        var requestUrl = BuildRequestUrl("lookup", queryParams);

        var results = await MakeRequestAsync<List<NominatimPlace>>(requestUrl, cancellationToken);

        return results.FirstOrDefault() is { } result
            ? MapToGeocodingResult(result)
            : null;
    }

    public async Task<IEnumerable<GeocodingResult>> StructuredSearchAsync(StructuredAddress structuredAddress,
        NominatimGeocodingOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (structuredAddress == null)
        {
            throw new ArgumentNullException(nameof(structuredAddress));
        }

        var queryParams = new Dictionary<string, string>
        {
            ["format"] = "json",
            ["addressdetails"] = options?.IncludeAddressDetails ?? true ? "1" : "0"
        };

        // Add all non-null structured address components
        if (!string.IsNullOrEmpty(structuredAddress.Street))
            queryParams["street"] = structuredAddress.Street;

        if (!string.IsNullOrEmpty(structuredAddress.City))
            queryParams["city"] = structuredAddress.City;

        if (!string.IsNullOrEmpty(structuredAddress.County))
            queryParams["county"] = structuredAddress.County;

        if (!string.IsNullOrEmpty(structuredAddress.State))
            queryParams["state"] = structuredAddress.State;

        if (!string.IsNullOrEmpty(structuredAddress.Country))
            queryParams["country"] = structuredAddress.Country;

        if (!string.IsNullOrEmpty(structuredAddress.PostalCode))
            queryParams["postalcode"] = structuredAddress.PostalCode;

        if (!string.IsNullOrEmpty(structuredAddress.HouseNumber))
            queryParams["housenumber"] = structuredAddress.HouseNumber;

        AddCommonParams(queryParams, options);

        var requestUrl = BuildRequestUrl("search", queryParams);

        var results = await MakeRequestAsync<List<NominatimPlace>>(requestUrl, cancellationToken);
        return results.Select(MapToGeocodingResult).ToList();
    }

    #region Helper Methods

    private string BuildRequestUrl(string endpoint, Dictionary<string, string> parameters)
    {
        var builder = new UriBuilder($"{_options.NominatimServerUrl}/{endpoint}");
        var query = HttpUtility.ParseQueryString(builder.Query);

        foreach (var param in parameters)
        {
            query[param.Key] = param.Value;
        }

        builder.Query = query.ToString();
        return builder.Uri.ToString();
    }

    private void AddCommonParams(Dictionary<string, string> queryParams, GeocodingOptions? options)
    {
        var nominatimOptions = options as NominatimGeocodingOptions;

        // Add limit if specified
        if (options?.Limit.HasValue == true)
        {
            queryParams["limit"] = options.Limit.Value.ToString();
        }

        // Add language if specified
        if (!string.IsNullOrEmpty(options?.Language))
        {
            queryParams["accept-language"] = options.Language;
        }

        // Add bounding box if specified
        if (options?.BoundingBox != null && options.BoundingBox.Length == 4)
        {
            queryParams["viewbox"] = $"{options.BoundingBox[2]},{options.BoundingBox[0]},{options.BoundingBox[3]},{options.BoundingBox[1]}";
            queryParams["bounded"] = "1";
        }

        // Add Nominatim-specific options
        if (nominatimOptions != null)
        {
            if (!string.IsNullOrEmpty(nominatimOptions.CountryCodeBias))
            {
                queryParams["countrycodes"] = nominatimOptions.CountryCodeBias;
            }

            if (nominatimOptions.IncludeExtraTags)
            {
                queryParams["extratags"] = "1";
            }

            if (nominatimOptions.IncludeNameDetails)
            {
                queryParams["namedetails"] = "1";
            }

            if (nominatimOptions.Debug)
            {
                queryParams["debug"] = "1";
            }
        }
    }

    private async Task<T> MakeRequestAsync<T>(string url, CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient();

        // Nominatim requires a User-Agent
        client.DefaultRequestHeaders.Add("User-Agent", "GeoToolkit");

        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(content, _jsonOptions)
            ?? throw new JsonException("Failed to deserialize response from Nominatim");
    }

    private GeocodingResult MapToGeocodingResult(NominatimPlace place)
    {
        return new GeocodingResult
        {
            PlaceId = $"{place.OsmType?.Substring(0, 1).ToUpper()}{place.OsmId}",
            Latitude = double.TryParse(place.Lat, out var lat) ? lat : 0,
            Longitude = double.TryParse(place.Lon, out var lon) ? lon : 0,
            DisplayName = place.DisplayName,
            Address = MapAddress(place.Address),
            BoundingBox = place.BoundingBox?.Select(double.Parse).ToArray(),
            OsmType = place.OsmType,
            OsmId = place.OsmId.ToString(),
            Category = place.Category ?? place.Type,
            Importance = place.Importance,
            Provider = "Nominatim",
            License = place.Licence,
            AdditionalData = new Dictionary<string, object>()
        };
    }

    private AddressComponent MapAddress(NominatimAddress? address)
    {
        if (address == null)
            return null!;

        return new AddressComponent
        {
            HouseNumber = address.HouseNumber,
            Road = address.Road ?? address.Street,
            Suburb = address.Suburb,
            City = address.City ?? address.Town ?? address.Village,
            Municipality = address.Municipality,
            County = address.County,
            State = address.State,
            PostalCode = address.Postcode,
            Country = address.Country,
            CountryCode = address.CountryCode,
            // Map additional fields
            AdditionalFields = new Dictionary<string, object>()
        };
    }

    #endregion

    #region Nominatim Models

    /// <summary>
    /// Internal class for deserializing Nominatim API responses
    /// </summary>
    private record NominatimPlace
    {
        [JsonPropertyName("place_id")]
        public long? PlaceId { get; set; }
        public string? Licence { get; set; }

        [JsonPropertyName("osm_type")]
        public string? OsmType { get; set; }

        [JsonPropertyName("osm_id")]
        public long? OsmId { get; set; }
        public string? Lat { get; set; }
        public string? Lon { get; set; }

        public string? Class { get; set; }
        public string? Type { get; set; }

        [JsonPropertyName("place_rank")]
        public double? PlaceRank { get; set; }

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }
        public NominatimAddress? Address { get; set; }
        public string[]? BoundingBox { get; set; }
        public double Importance { get; set; }
        public string? Category { get; set; }
    }

    /// <summary>
    /// Internal class for deserializing Nominatim address components
    /// </summary>
    private record NominatimAddress
    {
        [JsonPropertyName("house_number")]
        public string? HouseNumber { get; set; }
        public string? Road { get; set; }

        public string? Highway { get; set; }
        public string? Suburb { get; set; }
        public string? Borough { get; set; }
        public string? Street { get; set; }
        public string? Village { get; set; }
        public string? Town { get; set; }
        public string? City { get; set; }
        public string? Municipality { get; set; }
        public string? County { get; set; }

        public string? Region { get; set; }
        public string? State { get; set; }

        [JsonPropertyName("state_district")]
        public string? StateDistrict { get; set; }
        public string? Postcode { get; set; }
        public string? Country { get; set; }

        [JsonPropertyName("country_code")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("ISO3166-2-lvl4")]
        public string? IsoCode { get; set; }
    }

    #endregion
}