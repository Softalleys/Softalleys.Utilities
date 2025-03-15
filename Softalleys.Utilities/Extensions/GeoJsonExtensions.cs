using Microsoft.Spatial;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Softalleys.Utilities.Extensions;

public static class GeoJsonExtensions
{
    public static Point ToNetTopologyPoint(this GeographyPoint geographyPoint)
    {
        return new Point(geographyPoint.Longitude, geographyPoint.Latitude) { SRID = 4326 };
    }
    
    public static GeographyPoint ToGeographyPoint(this Point point)
    {
        return GeographyPoint.Create(point.Y, point.X);
    }
    
    public static GeometryPoint ToGeometryPoint(this Point geographyPoint)
    {
        return GeometryPoint.Create(geographyPoint.X, geographyPoint.Y);
    }
    
    public static byte[] ToWellKnownBinary(this Point point)
    {
        return point.AsBinary();
    }
    
    public static Point FromWellKnownBinaryToPoint(this byte[] wkb)
    {
        var reader = new WKBReader();
        NetTopologySuite.Geometries.Geometry geometry = reader.Read(wkb);
        
        if (geometry is Point point)
        {
            return point;
        }
        
        throw new ArgumentException("The provided WKB does not represent a Point geometry.");
    }
    
    public static Polygon FromWellKnownBinaryToPolygon(this byte[] wkb)
    {
        var reader = new WKBReader();
        NetTopologySuite.Geometries.Geometry geometry = reader.Read(wkb);
        
        if (geometry is Polygon polygon)
        {
            return polygon;
        }
        
        throw new ArgumentException("The provided WKB does not represent a Polygon geometry.");
    }

    public static LineString FromWellKnownBinaryToLineString(this byte[] wkb)
    {
        var reader = new WKBReader();
        NetTopologySuite.Geometries.Geometry geometry = reader.Read(wkb);
        
        if (geometry is LineString lineString)
        {
            return lineString;
        }
        
        throw new ArgumentException("The provided WKB does not represent a LineString geometry.");
    }
    
    public static NetTopologySuite.Geometries.Geometry FromWellKnownBinaryToGeometry(this byte[] wkb)
    {
        var reader = new WKBReader();
        return reader.Read(wkb);
    }
    
    public static NetTopologySuite.Geometries.Polygon ToNetTopologyPolygon(this GeographyPolygon geographyPolygon)
    {
        var linearRings = geographyPolygon.Rings.Select(ring => 
                new LinearRing(ring.Points.Select(x => 
                        new Coordinate(x.Longitude, x.Latitude))
                    .ToArray()))
            .ToList();

        var polygon = new Polygon(linearRings.First());

        return polygon;
    }
    
    public static GeographyPolygon ToGeographyPolygon(this NetTopologySuite.Geometries.Polygon polygon)
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