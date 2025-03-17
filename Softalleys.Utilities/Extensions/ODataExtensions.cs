using System.Linq.Expressions;
using Microsoft.OData.ModelBuilder;

namespace Softalleys.Utilities.Extensions;

/// <summary>
/// Provides extension methods for OData model building.
/// </summary>
public static class ODataExtensions
{
    /// <summary>
    /// Replaces a NetTopologySuite property with a Microsoft.Spatial property in the OData model.
    /// </summary>
    /// <typeparam name="TStructuralType">The type of the structural entity.</typeparam>
    /// <typeparam name="TTopologySuiteProperty">The type of the NetTopologySuite property.</typeparam>
    /// <param name="modelBuilder">The OData convention model builder.</param>
    /// <param name="topologySuitePropertyExpression">The expression representing the NetTopologySuite property.</param>
    /// <param name="topologySuitePropertyName">The name of the NetTopologySuite property.</param>
    /// <param name="microsoftSpatialPropertyName">The name of the Microsoft.Spatial property.</param>
    /// <returns>The updated OData convention model builder.</returns>
    public static ODataConventionModelBuilder ReplaceNetTopologySuiteWithMicrosoftSpatial
        <TStructuralType,TTopologySuiteProperty>
        (this ODataConventionModelBuilder modelBuilder,  
            Expression<Func<TStructuralType,TTopologySuiteProperty>> topologySuitePropertyExpression,
            string topologySuitePropertyName,
            string microsoftSpatialPropertyName)
        where TStructuralType : class
    {
        modelBuilder.EntityType<TStructuralType>().Ignore(topologySuitePropertyExpression);

        var locationType = modelBuilder.StructuralTypes.First(t => t.ClrType == typeof(TStructuralType));
        
        locationType.AddProperty(typeof(TStructuralType)
                .GetProperty(microsoftSpatialPropertyName)).Name 
            = topologySuitePropertyName;

        return modelBuilder;
    }
}