using System.Formats.Cbor;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.OData;

namespace Softalleys.Utilities.Formatters.OData.Cbor;

/// <summary>
/// A custom OData writer that outputs data in Concise Binary Object Representation (CBOR) format.
/// </summary>
/// <remarks>
/// This class handles the conversion of OData objects into CBOR format, supporting
/// both simple and complex data structures including nested resources.
/// </remarks>
public class CborODataWriter(CustomizedOutputContext context) : CustomizedWriter(context)
{
    private readonly CborWriter _cborWriter = new();

    /// <summary>
    /// Processes and writes all accumulated OData items to the output stream in CBOR format.
    /// </summary>
    /// <remarks>
    /// This method handles the serialization of top-level OData resources or resource sets
    /// into CBOR binary format.
    /// </remarks>
    protected override void WriteItems()
    {
        if (TopLevelItem == null)
        {
            return;
        }

        // For simplicity, NOT consider the inheritance
        if (TopLevelItem is ODataResourceSetWrapper topResourceSet)
        {
            WriteResourceSet(topResourceSet);
        }
        else if (TopLevelItem is ODataResourceWrapper topResource)
        {
            WriteResource(topResource);
        }

        var encoded = _cborWriter.Encode();

        // If you want to write the byte[] directly into response, use this line code.
        Context.Stream?.Write(encoded, 0, encoded.Length);

        // Writing byte[] as base64 is just for readability, you can choose any technique to write the byte[]
        // Context.Writer.Write(Convert.ToBase64String(encoded));
    }

    /// <summary>
    /// Writes a resource set to the CBOR stream as an array.
    /// </summary>
    /// <param name="resourceSetWrapper">The resource set wrapper to serialize.</param>
    private void WriteResourceSet(ODataResourceSetWrapper? resourceSetWrapper)
    {
        if (resourceSetWrapper != null)
        {
            var count = resourceSetWrapper.Resources.Count;
            _cborWriter.WriteStartArray(count);
        }

        if (resourceSetWrapper?.Resources != null)
            foreach (var resource in resourceSetWrapper.Resources)
            {
                WriteResource(resource);
            }

        _cborWriter.WriteEndArray();
    }

    /// <summary>
    /// Writes a resource to the CBOR stream as a map.
    /// </summary>
    /// <param name="resourceWrapper">The resource wrapper to serialize.</param>
    /// <remarks>
    /// Processes both direct properties and nested resources as key-value pairs in the CBOR map.
    /// </remarks>
    private void WriteResource(ODataResourceWrapper? resourceWrapper)
    {
        if (resourceWrapper != null)
        {
            var count = resourceWrapper.Resource.Properties.Count();
            count += resourceWrapper.NestedResourceInfos.Count;

            _cborWriter.WriteStartMap(count);
        }

        foreach (var propertyInfo in resourceWrapper?.Resource?.Properties ?? [])
        {
            if (propertyInfo is not ODataProperty property) continue;

            _cborWriter.WriteTextString(property.Name); // key

            WriteValue(property.Value);
        }

        foreach (var property in resourceWrapper?.NestedResourceInfos ?? [])
        {
            _cborWriter.WriteTextString(property.NestedResourceInfo.Name); // key

            WriteNestedProperty(property);
        }

        _cborWriter.WriteEndMap();
    }

    /// <summary>
    /// Writes a nested resource property to the CBOR stream.
    /// </summary>
    /// <param name="nestedResourceInfoWrapper">The nested resource info wrapper to serialize.</param>
    /// <remarks>
    /// This method handles both nested individual resources and collections of resources.
    /// </remarks>
    private void WriteNestedProperty(ODataNestedResourceInfoWrapper nestedResourceInfoWrapper)
    {
        foreach (var childItem in nestedResourceInfoWrapper.NestedItems)
        {
            switch (childItem)
            {
                case ODataResourceSetWrapper resourceSetWrapper:
                    WriteResourceSet(resourceSetWrapper);
                    continue;
                case ODataResourceWrapper resourceWrapper:
                    WriteResource(resourceWrapper);
                    break;
            }
        }
    }

    /// <summary>
    /// Writes an OData value to the CBOR stream in the appropriate format.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <remarks>
    /// Handles various data types including enum values, collections, and primitive types
    /// by writing them in the corresponding CBOR format.
    /// </remarks>
    /// <exception cref="NotImplementedException">Thrown when an unsupported value type is encountered.</exception>
    private void WriteValue(object value)
    {
        var valueType = value.GetType();
        valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

        switch (value)
        {
            case ODataEnumValue enumValue:
                _cborWriter.WriteTextString(enumValue.Value);
                break;
            case ODataCollectionValue collectionValue:
            {
                _cborWriter.WriteStartArray(collectionValue.Items.Count());
                foreach (var item in collectionValue.Items)
                {
                    WriteValue(item);
                }

                _cborWriter.WriteEndArray();
                break;
            }
            default:
            {
                if (valueType == typeof(int))
                {
                    _cborWriter.WriteInt32((int)value);
                }
                else if (valueType == typeof(bool))
                {
                    _cborWriter.WriteBoolean((bool)value);
                }
                else if (valueType == typeof(string))
                {
                    _cborWriter.WriteTextString(value.ToString() ?? string.Empty);
                }
                else
                {
                    throw new NotImplementedException(
                        "I don't have time to implement all. You can add more if you need more.");
                }

                break;
            }
        }
    }
}