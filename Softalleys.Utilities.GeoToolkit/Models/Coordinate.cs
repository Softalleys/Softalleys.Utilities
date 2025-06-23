namespace Softalleys.Utilities.GeoToolkit.Models;

/// <summary>
/// Represents a geographic coordinate (latitude and longitude).
/// </summary>
public record Coordinate
{
    /// <summary>
    /// Gets or sets the latitude component of the coordinate.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude component of the coordinate.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Coordinate"/> class.
    /// </summary>
    public Coordinate() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Coordinate"/> class with the specified latitude and longitude.
    /// </summary>
    /// <param name="latitude">The latitude component.</param>
    /// <param name="longitude">The longitude component.</param>
    public Coordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Returns a string representation of the coordinate in the format "latitude,longitude".
    /// </summary>
    /// <returns>A string representation of the coordinate.</returns>
    public override string ToString() => $"{Latitude},{Longitude}";
}
