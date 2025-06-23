using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Softalleys.Utilities.GeoToolkit.Configuration;
using Softalleys.Utilities.GeoToolkit.Models;
using Softalleys.Utilities.GeoToolkit.Providers;
using System.Net.Http;
using System.Reflection;
using Xunit.Abstractions;

namespace Softalleys.Utilities.GeoToolkit.Tests.Providers;

public class NominatimGeocodingServiceTests : IDisposable
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ServiceProvider _serviceProvider;
    private readonly NominatimGeocodingService _service;

    public NominatimGeocodingServiceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        // Setup DI services
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add HTTP client factory
        services.AddHttpClient();

        // Configure Nominatim options
        services.Configure<GeoToolkitNominatimOptions>(options =>
        {
            options.NominatimServerUrl = "http://localhost:8080";
            options.ShowLogs = true;
        });

        // Build service provider
        _serviceProvider = services.BuildServiceProvider();

        // Create the service to test
        var logger = _serviceProvider.GetRequiredService<ILogger<NominatimGeocodingService>>();
        var httpClientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
        var options = _serviceProvider.GetRequiredService<IOptions<GeoToolkitNominatimOptions>>();

        _service = new NominatimGeocodingService(logger, httpClientFactory, options);
        //
        // // Be considerate of the Nominatim API by adding delay between tests
        // Thread.Sleep(1000);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    [Fact]
    public async Task SearchAsync_WithValidQuery_ShouldReturnResults()
    {
        // Arrange
        var query = "Monterrey, Nuevo León";

        // Act
        var results = (await _service.SearchAsync(query)).ToList();

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Address?.County?.Contains("Monterrey", StringComparison.OrdinalIgnoreCase) == true);
        Assert.Contains(results, r => r.Address?.State?.Contains("Nuevo León", StringComparison.OrdinalIgnoreCase) == true ||
                                      r.Address?.Country == "Mexico" ||
                                      r.Address?.CountryCode?.Equals("mx", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public async Task ReverseGeocodeAsync_WithValidCoordinates_ShouldReturnResult()
    {
        // Arrange - Coordinates for Mexico City (Zócalo)
        var latitude = 19.4326;
        var longitude = -99.1332;

        // Act
        var results = (await _service.ReverseGeocodeAsync(latitude, longitude)).ToList();

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);

        var firstResult = results.First();
        Assert.NotNull(firstResult);
        Assert.NotNull(firstResult.Address);
        // Check for either "Ciudad de México", "Mexico City", "CDMX", or "México" in the display name
        Assert.True(
            firstResult.Address?.City?.Contains("Ciudad de México", StringComparison.OrdinalIgnoreCase) == true ||
            firstResult.Address?.City?.Contains("Mexico City", StringComparison.OrdinalIgnoreCase) == true ||
            firstResult.DisplayName?.Contains("CDMX", StringComparison.OrdinalIgnoreCase) == true ||
            firstResult.DisplayName?.Contains("México", StringComparison.OrdinalIgnoreCase) == true
        );
    }

    [Fact]
    public async Task StructuredSearchAsync_WithValidAddress_ShouldReturnResults()
    {
        // Arrange
        var structuredAddress = new StructuredAddress
        {
            City = "Guadalajara",
            State = "Jalisco",
            Country = "Mexico"
        };

        // Act
        var results = (await _service.StructuredSearchAsync(structuredAddress)).ToList();

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.Contains(results, r =>
            r.DisplayName?.Contains("Guadalajara", StringComparison.OrdinalIgnoreCase) == true ||
            r.Address?.City?.Contains("Guadalajara", StringComparison.OrdinalIgnoreCase) == true);
        Assert.Contains(results, r =>
            r.Address?.City?.Contains("Guadalajara", StringComparison.OrdinalIgnoreCase) == true ||
            r.Address?.State?.Contains("Jalisco", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public async Task SearchAsync_WithLimitOption_ShouldRespectLimit()
    {
        // Arrange
        var query = "Oaxaca";
        var options = new NominatimGeocodingOptions { Limit = 2 };

        // Act
        var results = await _service.SearchAsync(query, options);

        // Assert
        Assert.NotNull(results);
        Assert.True(results.Count() <= 2);
    }

    [Fact]
    public async Task SearchAsync_WithBoundingBox_ShouldReturnResultsInBoundingBox()
    {
        // Arrange - Bounding box around Cancún area
        var query = "hotel";
        var options = new NominatimGeocodingOptions
        {
            BoundingBox = [20.6994000, -88.2032371, 21.6995100, -88.2030071], // South, West, North, East (Cancún area)
            Limit = 5
        };

        // Act
        var results = await _service.SearchAsync(query, options);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
    }


    [Theory]
    [InlineData("Puebla", "es", "Puebla")]
    [InlineData("Mexico City", "en", "Mexico")]
    public async Task SearchAsync_WithLanguageOption_ShouldReturnLocalizedResults(string city, string language, string expectedStringContains)
    {
        // Arrange
        var options = new NominatimGeocodingOptions
        {
            Language = language,
            Limit = 3
        };

        // Act
        var results = (await _service.SearchAsync(city, options)).ToList();

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.DisplayName?.Contains(expectedStringContains, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    /// <summary>
    /// Debug test to capture the exact URL and response when errors occur
    /// </summary>
    [Fact(Skip = "Only run this test when debugging HTTP 400 errors")]
    public async Task Debug_CaptureRequestAndResponse()
    {
        try
        {
            // Create a custom HttpClient for debugging
            var clientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(clientHandler);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "GeoToolkit-Debugging");

            // Setup a request
            var testQuery = "Monterrey, Nuevo León";

            // Build the request URL manually using reflection to access private method
            var buildRequestUrlMethod = _service.GetType().GetMethod(
                "BuildRequestUrl",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var parameters = new Dictionary<string, string>
            {
                ["q"] = testQuery,
                ["format"] = "json",
                ["addressdetails"] = "1"
            };

            var fullUrl = buildRequestUrlMethod?.Invoke(_service, ["search", parameters]) as string;

            // Make a direct request to capture the response
            try
            {
                var response = await httpClient.GetAsync(fullUrl);
                var responseBody = await response.Content.ReadAsStringAsync();

                _testOutputHelper.WriteLine($"Status Code: {response.StatusCode}");
                _testOutputHelper.WriteLine($"Response Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
                _testOutputHelper.WriteLine($"Response Body: {responseBody}");

                // If we didn't get a success status code, make this test fail with useful information
                if (!response.IsSuccessStatusCode)
                {
                    Assert.Fail($"Request to {fullUrl} failed with status {response.StatusCode}. Response: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Request to {fullUrl} threw an exception: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Assert.Fail($"Setup failed with exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper method to test any specific URL that's causing issues and get detailed response
    /// </summary>
    [Fact(Skip = "Only run manually to test a specific URL")]
    public async Task Debug_TestSpecificUrl()
    {
        var testUrl = "http://localhost:8080/search?q=Monterrey%2C+Nuevo+Le%C3%B3n&format=json&addressdetails=1";

        _testOutputHelper.WriteLine($"Testing URL: {testUrl}");

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "GeoToolkit-Debugging");

        try
        {
            var response = await httpClient.GetAsync(testUrl);
            var responseBody = await response.Content.ReadAsStringAsync();

            _testOutputHelper.WriteLine($"Status Code: {response.StatusCode}");
            _testOutputHelper.WriteLine($"Response Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
            _testOutputHelper.WriteLine($"Response Body: {responseBody}");

            // Test will pass regardless of response status to allow inspection of the output
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine($"Exception: {ex}");
        }
    }
}
