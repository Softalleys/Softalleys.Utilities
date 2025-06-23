# Softalleys.Utilities.GeoToolkit

A .NET library for geocoding services, providing a flexible and extensible way to work with geocoding providers. The initial implementation includes support for Nominatim.

## Features

- **Forward Geocoding**: Search for coordinates from an address query.
- **Reverse Geocoding**: Find addresses from geographic coordinates.
- **Place Lookup**: Retrieve details of a place by its ID.
- **Structured Address Search**: Perform searches using structured address components (for supported providers like Nominatim).
- **Extensible**: Designed with interfaces to allow for other geocoding providers to be implemented.

## Installation

This library can be installed via NuGet Package Manager.

```shell
dotnet add package Softalleys.Utilities.GeoToolkit
```

## Getting Started

To get started, register the `NominatimGeocodingService` in your application's service container.

### Using `IConfiguration`

You can configure the service using the `appsettings.json` file.

**appsettings.json:**
```json
{
  "GeoToolkit": {
    "NominatimServerUrl": "https://nominatim.openstreetmap.org",
    "ShowLogs": true
  }
}
```

**Program.cs:**
```csharp
builder.Services.AddNominatimGeoToolkit(builder.Configuration.GetSection("GeoToolkit"));
```

### Using `Action<GeoToolkitNominatimOptions>`

Alternatively, you can configure the service directly in code.

**Program.cs:**
```csharp
builder.Services.AddNominatimGeoToolkit(options =>
{
    options.NominatimServerUrl = "https://nominatim.openstreetmap.org";
    options.ShowLogs = true;
});
```

## Usage

Inject the `IGeocodingService` or `INominatimGeocodingService` into your services or controllers.

```csharp
public class GeocodingController(IGeocodingService geocodingService, INominatimGeocodingService nominatimService)
{
    // ... use the services
}
```

### Forward Geocoding

To find locations for an address query:

```csharp
var results = await geocodingService.SearchAsync("Eiffel Tower, Paris, France");
foreach (var result in results)
{
    Console.WriteLine($"{result.DisplayName}: ({result.Latitude}, {result.Longitude})");
}
```

### Reverse Geocoding

To find an address for a given coordinate:

```csharp
var results = await geocodingService.ReverseGeocodeAsync(48.858370, 2.294481);
foreach (var result in results)
{
    Console.WriteLine(result.DisplayName);
}
```

### Lookup by ID

To look up a place by its OSM (OpenStreetMap) ID:

```csharp
// Format: {OSM_TYPE}{OSM_ID} where type is (N)ode, (W)ay, or (R)elation
var place = await geocodingService.LookupByIdAsync("R148106");
if (place != null)
{
    Console.WriteLine(place.DisplayName);
}
```

### Structured Search (Nominatim)

For more precise searches with Nominatim, you can use a structured address:

```csharp
var structuredAddress = new StructuredAddress
{
    Street = "1600 Amphitheatre Parkway",
    City = "Mountain View",
    State = "CA",
    Country = "USA",
    PostalCode = "94043"
};

var results = await nominatimService.StructuredSearchAsync(structuredAddress);
foreach (var result in results)
{
    Console.WriteLine(result.DisplayName);
}
```

