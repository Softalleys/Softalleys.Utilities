using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Softalleys.Utilities.Formatters.OData.Csv;

/// <summary>
/// A custom OData reader implementation that reads data from CSV format.
/// </summary>
/// <remarks>
/// This reader processes CSV data where the first line contains property names
/// and the second line contains corresponding property values.
/// </remarks>
/// <param name="txtReader">The TextReader that provides access to the CSV content.</param>
/// <param name="structuredType">The EDM structured type that describes the data structure.</param>
public class CsvODataReader(TextReader txtReader, IEdmStructuredType structuredType) : ODataReader
{
    private ODataReaderState _state = ODataReaderState.Start;
    
    private ODataItem? _item;

    /// <summary>
    /// Gets the current OData item being read.
    /// </summary>
    /// <value>The current OData item or null if no item is available.</value>
    public override ODataItem? Item => _item;

    /// <summary>
    /// Gets the current state of the reader.
    /// </summary>
    /// <value>The current <see cref="ODataReaderState"/>.</value>
    public override ODataReaderState State => _state;

    /// <summary>
    /// Reads the next OData item from the CSV data.
    /// </summary>
    /// <returns>True if an item was successfully read; otherwise, false if the end of data was reached.</returns>
    public override bool Read()
    {
        // for simplicity
        bool result;
        switch (_state)
        {
            case ODataReaderState.Start:
                result = ReadAtStart();
                break;

            case ODataReaderState.ResourceStart:
                _state = ODataReaderState.ResourceEnd;
                result = true;
                break;

            case ODataReaderState.ResourceEnd:
                _state = ODataReaderState.Completed;
                result = false;
                break;

            default:
                throw new NotImplementedException("DIY!");
        }

        return result;
    }

    /// <summary>
    /// Asynchronously reads the next OData item from the CSV data.
    /// </summary>
    /// <returns>A task that represents the asynchronous read operation. The value of the TResult parameter is true if an item was successfully read; otherwise, false.</returns>
    public override Task<bool> ReadAsync()
    {
        var ret = Read();
        return Task.FromResult(ret);
    }

    /// <summary>
    /// Reads the initial data from the CSV input.
    /// </summary>
    /// <returns>True if the data was successfully read; otherwise, false.</returns>
    /// <remarks>
    /// Processes the first two lines of CSV where the first line contains property names
    /// and the second line contains corresponding property values.
    /// </remarks>
    private bool ReadAtStart()
    {
        // for simplicity, first line is the property name
        // second line is the property value
        // and only two line
        var firstLine = txtReader.ReadLine() ?? "";
        var secondLine = txtReader.ReadLine() ?? "";

        var odataProperties = new List<ODataProperty>();

        var properties = firstLine.Split(',');
        var propertiesValue = secondLine.Split(",");
        
        for (var i = 0; i < properties.Length; ++i)
        {
            var propertyName = properties[i];
            var propertyValue = propertiesValue[i];

            var edmProperty = structuredType.FindProperty(propertyName);

            var property = new ODataProperty
            {
                Name = propertyName,
                Value = ConvertPropertyValue(edmProperty, propertyValue)
            };

            odataProperties.Add(property);
        }

        _item = new ODataResource
        {
            TypeName = structuredType.FullTypeName(),
            Properties = odataProperties
        };

        _state = ODataReaderState.ResourceStart;
        return true;
    }

    /// <summary>
    /// Converts a string property value to the appropriate type based on EDM property definition.
    /// </summary>
    /// <param name="edmProperty">The EDM property definition.</param>
    /// <param name="propertyValue">The string value to convert.</param>
    /// <returns>The converted value with the appropriate type.</returns>
    /// <exception cref="NotImplementedException">Thrown when conversion for the specific type is not implemented.</exception>
    internal static object ConvertPropertyValue(IEdmProperty edmProperty, string propertyValue)
    {
        switch (edmProperty.Type.TypeKind())
        {
            case EdmTypeKind.Primitive:
                var primitiveTypeRef = (IEdmPrimitiveTypeReference)edmProperty.Type;
                return primitiveTypeRef.PrimitiveKind() switch
                {
                    EdmPrimitiveTypeKind.String => propertyValue,
                    EdmPrimitiveTypeKind.Int32 => int.Parse(propertyValue),
                    _ => throw new NotImplementedException("IEdmPrimitiveTypeReference DIY!")
                };

            case EdmTypeKind.None:
            case EdmTypeKind.Entity:
            case EdmTypeKind.Complex:
            case EdmTypeKind.Collection:
            case EdmTypeKind.EntityReference:
            case EdmTypeKind.Enum:
            case EdmTypeKind.TypeDefinition:
            case EdmTypeKind.Untyped:
            case EdmTypeKind.Path:
            default:
                throw new NotImplementedException("edmProperty.Type DIY!");
        }
    }
}