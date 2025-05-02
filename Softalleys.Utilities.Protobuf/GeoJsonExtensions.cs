using Google.Protobuf;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Softalleys.Utilities.Protobuf;

/// <summary>
/// Provides extension methods for converting between NetTopologySuite geometry types and WKB/ByteString representations.
/// </summary>
public static class GeoJsonExtensions
{
    /// <summary>
    /// Converts a <see cref="ByteString"/> containing WKB data to a NetTopologySuite <see cref="Geometry"/> instance.
    /// </summary>
    /// <param name="byteString">The <see cref="ByteString"/> containing WKB data.</param>
    /// <returns>The deserialized <see cref="Geometry"/> instance, or null if conversion fails.</returns>
    public static Geometry? ToNetTopologyGeometry(this ByteString byteString)
    {
        var bytes = byteString.ToByteArray();

        var wkbReader = new WKBReader();
        return wkbReader.Read(bytes);
    }

    /// <summary>
    /// Converts a NetTopologySuite <see cref="Geometry"/> to a WKB byte array.
    /// </summary>
    /// <param name="geometry">The <see cref="Geometry"/> to serialize.</param>
    /// <returns>A byte array containing the WKB representation.</returns>
    public static byte[] ToByteArray(this Geometry geometry)
    {
        var wkbWriter = new WKBWriter();
        return wkbWriter.Write(geometry);
    }

    /// <summary>
    /// Converts a NetTopologySuite <see cref="Geometry"/> to a <see cref="ByteString"/> containing WKB data.
    /// </summary>
    /// <param name="geometry">The <see cref="Geometry"/> to serialize.</param>
    /// <returns>A <see cref="ByteString"/> containing the WKB representation.</returns>
    public static ByteString ToByteString(this Geometry geometry)
    {
        var wkbWriter = new WKBWriter();
        var bytes = wkbWriter.Write(geometry);
        return ByteString.CopyFrom(bytes);
    }

    /// <summary>
    /// Converts a NetTopologySuite <see cref="GeometryCollection"/> to a <see cref="ByteString"/> containing WKB data.
    /// </summary>
    /// <param name="geometryCollection">The <see cref="GeometryCollection"/> to serialize.</param>
    /// <returns>A <see cref="ByteString"/> containing the WKB representation.</returns>
    public static ByteString ToByteString(this GeometryCollection geometryCollection)
    {
        var wkbWriter = new WKBWriter();
        var bytes = wkbWriter.Write(geometryCollection);
        return ByteString.CopyFrom(bytes);
    }

    /// <summary>
    /// Converts a WKB byte array to a NetTopologySuite <see cref="Point"/>.
    /// </summary>
    /// <param name="wkb">The WKB byte array.</param>
    /// <returns>The deserialized <see cref="Point"/>, or null if conversion fails.</returns>
    public static Point? ToNetTopologyPoint(this byte[] wkb)
    {
        var wkbReader = new WKBReader();
        var geometry = wkbReader.Read(wkb);
        return geometry as Point;
    }

    /// <summary>
    /// Converts a <see cref="ByteString"/> containing WKB data to a NetTopologySuite <see cref="Point"/>.
    /// </summary>
    /// <param name="byteString">The <see cref="ByteString"/> containing WKB data.</param>
    /// <returns>The deserialized <see cref="Point"/>, or null if conversion fails.</returns>
    public static Point? ToNetTopologyPoint(this ByteString byteString)
    {
        var bytes = byteString.ToByteArray();
        var wkbReader = new WKBReader();
        var geometry = wkbReader.Read(bytes);
        return geometry as Point;
    }

    /// <summary>
    /// Converts a WKB byte array to a NetTopologySuite <see cref="LineString"/>.
    /// </summary>
    /// <param name="wkb">The WKB byte array.</param>
    /// <returns>The deserialized <see cref="LineString"/>, or null if conversion fails.</returns>
    public static LineString? ToNetTopologyLineString(this byte[] wkb)
    {
        var wkbReader = new WKBReader();
        var geometry = wkbReader.Read(wkb);
        return geometry as LineString;
    }

    /// <summary>
    /// Converts a <see cref="ByteString"/> containing WKB data to a NetTopologySuite <see cref="LineString"/>.
    /// </summary>
    /// <param name="byteString">The <see cref="ByteString"/> containing WKB data.</param>
    /// <returns>The deserialized <see cref="LineString"/>, or null if conversion fails.</returns>
    public static LineString? ToNetTopologyLineString(this ByteString byteString)
    {
        var bytes = byteString.ToByteArray();
        var wkbReader = new WKBReader();
        var geometry = wkbReader.Read(bytes);
        return geometry as LineString;
    }

    /// <summary>
    /// Converts a WKB byte array to a NetTopologySuite <see cref="Polygon"/>.
    /// </summary>
    /// <param name="wkb">The WKB byte array.</param>
    /// <returns>The deserialized <see cref="Polygon"/>, or null if conversion fails.</returns>
    public static Polygon? ToNetTopologyPolygon(this byte[] wkb)
    {
        var wkbReader = new WKBReader();
        var geometry = wkbReader.Read(wkb);
        return geometry as Polygon;
    }

    /// <summary>
    /// Converts a <see cref="ByteString"/> containing WKB data to a NetTopologySuite <see cref="Polygon"/>.
    /// </summary>
    /// <param name="byteString">The <see cref="ByteString"/> containing WKB data.</param>
    /// <returns>The deserialized <see cref="Polygon"/>, or null if conversion fails.</returns>
    public static Polygon? ToNetTopologyPolygon(this ByteString byteString)
    {
        var bytes = byteString.ToByteArray();
        var wkbReader = new WKBReader();
        var geometry = wkbReader.Read(bytes);
        return geometry as Polygon;
    }

    /// <summary>
    /// Converts a WKB byte array to a NetTopologySuite <see cref="MultiLineString"/>.
    /// </summary>
    /// <param name="wkb">The WKB byte array.</param>
    /// <returns>The deserialized <see cref="MultiLineString"/>, or null if conversion fails.</returns>
    public static MultiLineString? ToNetTopologyMultiLineString(this byte[] wkb)
    {
        var wkbReader = new WKBReader();
        var geometry = wkbReader.Read(wkb);
        return geometry as MultiLineString;
    }

    /// <summary>
    /// Converts a <see cref="ByteString"/> containing WKB data to a NetTopologySuite <see cref="MultiLineString"/>.
    /// </summary>
    /// <param name="byteString">The <see cref="ByteString"/> containing WKB data.</param>
    /// <returns>The deserialized <see cref="MultiLineString"/>, or null if conversion fails.</returns>
    public static MultiLineString? ToNetTopologyMultiLineString(this ByteString byteString)
    {
        var bytes = byteString.ToByteArray();
        var wkbReader = new WKBReader();
        var geometry = wkbReader.Read(bytes);
        return geometry as MultiLineString;
    }

    /// <summary>
    /// Converts a WKB byte array to a NetTopologySuite <see cref="MultiPolygon"/>.
    /// </summary>
    /// <param name="wkb">The WKB byte array.</param>
    /// <returns>The deserialized <see cref="MultiPolygon"/>, or null if conversion fails.</returns>
    public static MultiPolygon? ToNetTopologyMultiPolygon(this byte[] wkb)
    {
        var wkbReader = new WKBReader();
        var geometry = wkbReader.Read(wkb);
        return geometry as MultiPolygon;
    }

    /// <summary>
    /// Converts a <see cref="ByteString"/> containing WKB data to a NetTopologySuite <see cref="MultiPolygon"/>.
    /// </summary>
    /// <param name="byteString">The <see cref="ByteString"/> containing WKB data.</param>
    /// <returns>The deserialized <see cref="MultiPolygon"/>, or null if conversion fails.</returns>
    public static MultiPolygon? ToNetTopologyMultiPolygon(this ByteString byteString)
    {
        var bytes = byteString.ToByteArray();
        var wkbReader = new WKBReader();
        var geometry = wkbReader.Read(bytes);
        return geometry as MultiPolygon;
    }

    /// <summary>
    /// Converts a WKB byte array to a NetTopologySuite <see cref="MultiPoint"/>.
    /// </summary>
    /// <param name="wkb">The WKB byte array.</param>
    /// <returns>The deserialized <see cref="MultiPoint"/>, or null if conversion fails.</returns>
    public static MultiPoint? ToNetTopologyMultiPoint(this byte[] wkb)
    {
        var wkbReader = new WKBReader();
        var geometry = wkbReader.Read(wkb);
        return geometry as MultiPoint;
    }

    /// <summary>
    /// Converts a <see cref="ByteString"/> containing WKB data to a NetTopologySuite <see cref="MultiPoint"/>.
    /// </summary>
    /// <param name="byteString">The <see cref="ByteString"/> containing WKB data.</param>
    /// <returns>The deserialized <see cref="MultiPoint"/>, or null if conversion fails.</returns>
    public static MultiPoint? ToNetTopologyMultiPoint(this ByteString byteString)
    {
        var bytes = byteString.ToByteArray();
        var wkbReader = new WKBReader();
        var geometry = wkbReader.Read(bytes);
        return geometry as MultiPoint;
    }

    /// <summary>
    /// Converts a WKB byte array to a NetTopologySuite <see cref="GeometryCollection"/>.
    /// </summary>
    /// <param name="wkb">The WKB byte array.</param>
    /// <returns>The deserialized <see cref="GeometryCollection"/>, or null if conversion fails.</returns>
    public static GeometryCollection? ToNetTopologyGeometryCollection(this byte[] wkb)
    {
        var wkbReader = new WKBReader();
        var geometry = wkbReader.Read(wkb);
        return geometry as GeometryCollection;
    }

    /// <summary>
    /// Converts a <see cref="ByteString"/> containing WKB data to a NetTopologySuite <see cref="GeometryCollection"/>.
    /// </summary>
    /// <param name="byteString">The <see cref="ByteString"/> containing WKB data.</param>
    /// <returns>The deserialized <see cref="GeometryCollection"/>, or null if conversion fails.</returns>
    public static GeometryCollection? ToNetTopologyGeometryCollection(this ByteString byteString)
    {
        var bytes = byteString.ToByteArray();
        var wkbReader = new WKBReader();
        var geometry = wkbReader.Read(bytes);
        return geometry as GeometryCollection;
    }
}