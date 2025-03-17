using Microsoft.Spatial;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Linq;

namespace Softalleys.Utilities.Extensions;

/// <summary>
/// Provides extension methods for converting between Geography, Geometry, and Well‑Known Binary (WKB) representations.
/// </summary>
public static class GeoJsonExtensions
{
    /// <summary>
    /// Converts a <see cref="GeographyPoint"/> to a NetTopologySuite <see cref="Point"/>.
    /// </summary>
    /// <param name="geographyPoint">The geography point to convert.</param>
    /// <returns>A <see cref="Point"/> with SRID set to 4326.</returns>
    public static Point ToNetTopologyPoint(this GeographyPoint geographyPoint)
    {
        return new Point(geographyPoint.Longitude, geographyPoint.Latitude) { SRID = 4326 };
    }

    /// <summary>
    /// Converts a NetTopologySuite <see cref="Point"/> to a <see cref="GeographyPoint"/>.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    /// <returns>A <see cref="GeographyPoint"/> created from the point's coordinates.</returns>
    public static GeographyPoint ToGeographyPoint(this Point point)
    {
        return GeographyPoint.Create(point.Y, point.X);
    }

    /// <summary>
    /// Converts a NetTopologySuite <see cref="Point"/> to a <see cref="GeometryPoint"/>.
    /// </summary>
    /// <param name="geographyPoint">The point to convert.</param>
    /// <returns>A <see cref="GeometryPoint"/> representing the same coordinates.</returns>
    public static GeometryPoint ToGeometryPoint(this Point geographyPoint)
    {
        return GeometryPoint.Create(geographyPoint.X, geographyPoint.Y);
    }

    /// <summary>
    /// Converts a NetTopologySuite <see cref="Point"/> to its Well‑Known Binary (WKB) representation.
    /// </summary>
    /// <param name="point">The point to convert.</param>
    /// <returns>A byte array containing the WKB representation.</returns>
    public static byte[] ToWellKnownBinary(this Point point)
    {
        return point.AsBinary();
    }

    /// <summary>
    /// Converts a Well‑Known Binary (WKB) representation to a NetTopologySuite <see cref="Point"/>.
    /// </summary>
    /// <param name="wkb">The byte array representing the WKB.</param>
    /// <returns>A <see cref="Point"/> if the WKB represents a point.</returns>
    /// <exception cref="ArgumentException">Thrown when the WKB does not represent a point geometry.</exception>
    public static Point FromWellKnownBinaryToPoint(this byte[] wkb)
    {
        var reader = new WKBReader();
        var geometry = reader.Read(wkb);

        if (geometry is Point point)
        {
            return point;
        }

        throw new ArgumentException("The provided WKB does not represent a Point geometry.");
    }

    /// <summary>
    /// Converts a Well‑Known Binary (WKB) representation to a NetTopologySuite <see cref="Polygon"/>.
    /// </summary>
    /// <param name="wkb">The byte array representing the WKB.</param>
    /// <returns>A <see cref="Polygon"/> if the WKB represents a polygon.</returns>
    /// <exception cref="ArgumentException">Thrown when the WKB does not represent a polygon geometry.</exception>
    public static Polygon FromWellKnownBinaryToPolygon(this byte[] wkb)
    {
        var reader = new WKBReader();
        var geometry = reader.Read(wkb);

        if (geometry is Polygon polygon)
        {
            return polygon;
        }

        throw new ArgumentException("The provided WKB does not represent a Polygon geometry.");
    }

    /// <summary>
    /// Converts a Well‑Known Binary (WKB) representation to a NetTopologySuite <see cref="LineString"/>.
    /// </summary>
    /// <param name="wkb">The byte array representing the WKB.</param>
    /// <returns>A <see cref="LineString"/> if the WKB represents a linestring.</returns>
    /// <exception cref="ArgumentException">Thrown when the WKB does not represent a LineString geometry.</exception>
    public static LineString FromWellKnownBinaryToLineString(this byte[] wkb)
    {
        var reader = new WKBReader();
        var geometry = reader.Read(wkb);

        if (geometry is LineString lineString)
        {
            return lineString;
        }

        throw new ArgumentException("The provided WKB does not represent a LineString geometry.");
    }

    /// <summary>
    /// Converts a Well‑Known Binary (WKB) representation to a NetTopologySuite <see cref="NetTopologySuite.Geometries.Geometry"/>.
    /// </summary>
    /// <param name="wkb">The byte array representing the WKB.</param>
    /// <returns>A <see cref="NetTopologySuite.Geometries.Geometry"/> parsed from the WKB.</returns>
    public static NetTopologySuite.Geometries.Geometry FromWellKnownBinaryToGeometry(this byte[] wkb)
    {
        var reader = new WKBReader();
        return reader.Read(wkb);
    }

    /// <summary>
    /// Converts a <see cref="GeographyPolygon"/> to a NetTopologySuite <see cref="Polygon"/>.
    /// </summary>
    /// <param name="geographyPolygon">The geography polygon to convert.</param>
    /// <returns>A <see cref="Polygon"/> created from the geography polygon's rings.</returns>
    public static Polygon ToNetTopologyPolygon(this GeographyPolygon geographyPolygon)
    {
        var linearRings = geographyPolygon.Rings.Select(ring =>
                new LinearRing(ring.Points.Select(x =>
                        new Coordinate(x.Longitude, x.Latitude))
                    .ToArray()))
            .ToList();

        var polygon = new Polygon(linearRings.First());
        return polygon;
    }

    /// <summary>
    /// Converts a NetTopologySuite <see cref="Polygon"/> to a <see cref="GeographyPolygon"/>.
    /// </summary>
    /// <param name="polygon">The polygon to convert.</param>
    /// <returns>A <see cref="GeographyPolygon"/> constructed from the polygon's coordinates.</returns>
    public static GeographyPolygon ToGeographyPolygon(this Polygon polygon)
    {
        var factory = GeographyFactory.Polygon(CoordinateSystem.Geography(null));
        factory.Ring(polygon.Coordinate.Y, polygon.Coordinate.X);

        foreach (var c in polygon.Coordinates)
        {
            factory.LineTo(c.Y, c.X);
        }

        return factory.Build();
    }
}