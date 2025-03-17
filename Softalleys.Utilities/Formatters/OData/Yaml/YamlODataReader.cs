using Microsoft.OData;
using Microsoft.OData.Edm;
using Softalleys.Utilities.Formatters.OData.Csv;

namespace Softalleys.Utilities.Formatters.OData.Yaml;

/// <summary>
/// Provides functionality to read OData from YAML format.
/// </summary>
/// <param name="txtReader">The TextReader instance used to read YAML content.</param>
/// <param name="structuredType">The EDM structured type that defines the schema.</param>
public class YamlODataReader(TextReader txtReader, IEdmStructuredType structuredType) : ODataReader
{
    private ODataReaderState _state = ODataReaderState.Start;
    private ODataItem? _item;

    /// <inheritdoc/>
    public override ODataItem? Item => _item;

    /// <inheritdoc/>
    public override ODataReaderState State => _state;

    /// <summary>
    /// Reads the next item from the YAML input.
    /// </summary>
    /// <returns>
    /// true if there is more data to read; false if the end of the data has been reached.
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// Thrown when attempting to read unsupported OData states.
    /// </exception>
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

            case ODataReaderState.ResourceSetStart:
            case ODataReaderState.ResourceSetEnd:
            case ODataReaderState.NestedResourceInfoStart:
            case ODataReaderState.NestedResourceInfoEnd:
            case ODataReaderState.EntityReferenceLink:
            case ODataReaderState.Exception:
            case ODataReaderState.Completed:
            case ODataReaderState.Primitive:
            case ODataReaderState.DeltaResourceSetStart:
            case ODataReaderState.DeltaResourceSetEnd:
            case ODataReaderState.DeletedResourceStart:
            case ODataReaderState.DeletedResourceEnd:
            case ODataReaderState.DeltaLink:
            case ODataReaderState.DeltaDeletedLink:
            case ODataReaderState.NestedProperty:
            case ODataReaderState.Stream:
            default:
                throw new NotImplementedException("DIY!");
        }

        return result;
    }

    /// <inheritdoc/>
    public override Task<bool> ReadAsync()
    {
        bool ret = Read();
        return Task.FromResult(ret);
    }

    /// <summary>
    /// Reads and processes the initial YAML content.
    /// </summary>
    /// <returns>
    /// true if the content was successfully read; false otherwise.
    /// </returns>
    /// <remarks>
    /// This method reads the YAML content line by line, parsing each line as a property-value pair.
    /// The pairs are converted to ODataProperties and added to an ODataResource.
    /// </remarks>
    private bool ReadAtStart()
    {
        // for simplicity
        IList<ODataProperty> odataProperties = new List<ODataProperty>();

        var line = txtReader.ReadLine();
        while (line != null)
        {
            var properties = line.Split(':');

            var propertyName = properties[0].Trim();
            var propertyValue = properties[1].Trim();

            var edmProperty = structuredType.FindProperty(propertyName);

            var property = new ODataProperty
            {
                Name = propertyName,
                Value = CsvODataReader.ConvertPropertyValue(edmProperty, propertyValue)
            };

            odataProperties.Add(property);

            line = txtReader.ReadLine();
        }

        _item = new ODataResource
        {
            TypeName = structuredType.FullTypeName(),
            Properties = odataProperties
        };

        _state = ODataReaderState.ResourceStart;
        return true;
    }
}