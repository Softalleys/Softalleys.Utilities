using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.UriParser;
using Microsoft.Spatial;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Geometry = NetTopologySuite.Geometries.Geometry;

namespace Softalleys.Utilities.Binders.OData;

public class GeospatialFilterBinder : FilterBinder
{
    private const string GeoDistanceFunctionName = "geo.distance";

    private const string GeoIntersectsFunctionName = "geo.intersects";

    private const string GeoContainsFunctionName = "geo.contains";

    private static readonly MethodInfo DistanceMethodDb = typeof(Geometry).GetMethod(nameof(Geometry.Distance))!;

    private static readonly MethodInfo IntersectsMethodDb = typeof(Geometry).GetMethod(nameof(Geometry.Intersects))!;

    private static readonly MethodInfo ContainsMethodDb = typeof(Geometry).GetMethod(nameof(Geometry.Contains))!;

    public override Expression? BindSingleValueFunctionCallNode(SingleValueFunctionCallNode node,
        QueryBinderContext context)
    {
        return node.Name switch
        {
            GeoDistanceFunctionName => BindGeoDistance(node, context),
            GeoIntersectsFunctionName => BindGeoIntersects(node, context),
            GeoContainsFunctionName => BindGeoContains(node, context),
            _ => base.BindSingleValueFunctionCallNode(node, context)
        };
    }

    private Expression? BindGeoDistance(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        Expression[] arguments = BindArguments(node.Parameters, context);

        string? propertyName = null;

        foreach (var queryNode in node.Parameters)
        {
            if (queryNode is SingleValuePropertyAccessNode svpan)
            {
                propertyName = svpan.Property.Name;
            }
        }

        GetPointExpressions(arguments, propertyName, out var memberExpression, out var constantExpression);
        if (constantExpression == null) return null;
        
        var ex = Expression.Call(memberExpression, DistanceMethodDb, constantExpression);
        return ex;

    }

    private Expression? BindGeoIntersects(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        Expression[] arguments = BindArguments(node.Parameters, context);

        string? propertyName = null;

        foreach (var queryNode in node.Parameters)
        {
            switch (queryNode)
            {
                case SingleValuePropertyAccessNode accessNode:
                    propertyName = accessNode.Property.Name;
                    break;
                case ConvertNode node1:
                {
                    var convertNode = node1;
                    var svpan = convertNode.Source as SingleValuePropertyAccessNode;

                    propertyName = svpan?.Property.Name;
                    break;
                }
            }
        }

        GetPointExpressions(arguments, propertyName, out var memberExpression,
            out var constantExpression);
        if (constantExpression == null) return null;
        var ex = Expression.Call(memberExpression, IntersectsMethodDb, constantExpression);

        return ex;

    }

    private Expression? BindGeoContains(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        Expression[] arguments = BindArguments(node.Parameters, context);

        string? propertyName = null;

        foreach (var queryNode in node.Parameters)
        {
            if (queryNode.GetType() != typeof(SingleValuePropertyAccessNode)) continue;
            var svpan = queryNode as SingleValuePropertyAccessNode;
            propertyName = svpan?.Property.Name;
        }

        GetPointExpressions(arguments, propertyName, out var memberExpression, out var constantExpression);
        
        if (constantExpression == null) return null;
        var ex = Expression.Call(memberExpression, ContainsMethodDb, constantExpression);

        return ex;

    }

    private static void GetPointExpressions(Expression[] expressions, string? propertyName,
        out MemberExpression? memberExpression, out ConstantExpression? constantExpression)
    {
        memberExpression = null;
        constantExpression = null;

        foreach (var expression in expressions)
        {
            if (expression is MemberExpression memberExpr)
            {
                if (memberExpr.Expression is not ConstantExpression constantExpr)
                {
                    // The expression is a property of the entity of type Edm.Geography, we need to get the 
                    // expression of the property of type NetTopologySuite.Geometries.Geometry
                    // We can get the NetTopologySuite.Geometries.Geometry property from propertyName and 
                    // the model of the entity
                    
                    
                    memberExpression = Expression.Property(memberExpr.Expression!, "Location");
                    
                    continue;
                }
                var geography = GetGeographyFromConstantExpression(constantExpr);

                constantExpression = geography switch
                {
                    GeographyPoint point => Expression.Constant(CreatePoint(point.Latitude, point.Longitude)),
                    GeographyPolygon polygon => Expression.Constant(CreatePolygon(polygon)),
                    _ => constantExpression
                };
            }
            else
            {
                if (propertyName != null) memberExpression = Expression.Property(expression, propertyName);
            }
        }
    }

    private static Geography? GetGeographyFromConstantExpression(ConstantExpression? expression)
    {
        if (expression == null) return null;
        var constantExpressionValuePropertyInfo = expression.Type.GetProperty("Property");
        return constantExpressionValuePropertyInfo?.GetValue(expression.Value) as Geography;
    }

    private static Polygon CreatePolygon(GeographyPolygon geographyPolygon)
    {
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        var coordinates = geographyPolygon.Rings[0].Points.Select(p => new Coordinate(p.Longitude, p.Latitude))
            .ToArray();

        var linearRing = geometryFactory.CreateLinearRing(coordinates);

        return geometryFactory.CreatePolygon(linearRing);
    }

    private static Point CreatePoint(double latitude, double longitude)
    {
        // 4326 is the most common coordinate system used by GPS/Maps
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        // see https://docs.microsoft.com/en-us/ef/core/modeling/spatial
        // Longitude and Latitude
        var newLocation = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

        return newLocation;
    }
}