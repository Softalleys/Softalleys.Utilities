using System.Text;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.OData;
using Microsoft.Spatial;

namespace Softalleys.Utilities.Formatters.OData.Csv;

/// <summary>
/// A custom OData writer that outputs data in CSV format.
/// </summary>
/// <remarks>
/// This class handles the conversion of OData objects into CSV format, supporting
/// both simple and complex data structures including nested resources.
/// </remarks>
public class CsvWriter : ODataWriter
{
    private ODataItemWrapper? _topLevelItem;
    private readonly Stack<ODataItemWrapper> _itemsStack = new();

    private readonly CsvOutputContext? _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvWriter"/> class.
    /// </summary>
    /// <param name="context">The output context that provides access to the underlying writer.</param>
    public CsvWriter(CsvOutputContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Flushes any buffered content to the underlying stream.
    /// </summary>
    public override void Flush() => _context?.Flush();

    /// <summary>
    /// Completes the current OData item and pops it from the stack.
    /// </summary>
    /// <remarks>
    /// When the last item is popped, writes all accumulated content to the output stream.
    /// </remarks>
    public override void WriteEnd()
    {
        _itemsStack.Pop();

        if (_itemsStack.Count != 0) return;
        // we finished the process, let's write the value into stream
        WriteItems();
        Flush();
    }

    /// <summary>
    /// Begins writing an OData resource set (collection of resources).
    /// </summary>
    /// <param name="resourceSet">The resource set to write.</param>
    /// <returns>A completed task.</returns>
    public override Task WriteStartAsync(ODataResourceSet resourceSet)
    {
        var resourceSetWrapper = new ODataResourceSetWrapper(resourceSet);

        if (_topLevelItem == null)
        {
            _topLevelItem = resourceSetWrapper;
        }
        else
        {
            // It must be under nested resource info
            var parentNestedResourceInfo = (ODataNestedResourceInfoWrapper)_itemsStack.Peek();
            parentNestedResourceInfo.NestedItems.Add(resourceSetWrapper);
        }

        _itemsStack.Push(resourceSetWrapper);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Begins writing an OData resource.
    /// </summary>
    /// <param name="resource">The resource to write.</param>
    /// <returns>A completed task.</returns>
    public override Task WriteStartAsync(ODataResource resource)
    {
        var resourceWrapper = new ODataResourceWrapper(resource);
        if (_topLevelItem == null)
        {
            _topLevelItem = resourceWrapper;
        }
        else
        {
            var parentItem = _itemsStack.Peek();

            if (parentItem is ODataResourceSetWrapper parentResourceSet)
            {
                parentResourceSet.Resources.Add(resourceWrapper);
            }
            else
            {
                var parentNestedResource = (ODataNestedResourceInfoWrapper)parentItem;
                parentNestedResource.NestedItems.Add(resourceWrapper);
            }
        }

        _itemsStack.Push(resourceWrapper);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Begins writing a nested resource information.
    /// </summary>
    /// <param name="nestedResourceInfo">The nested resource information to write.</param>
    /// <returns>A completed task.</returns>
    public override Task WriteStartAsync(ODataNestedResourceInfo nestedResourceInfo)
    {
        var nestedResourceInfoWrapper = new ODataNestedResourceInfoWrapper(nestedResourceInfo);
        var parentResource = (ODataResourceWrapper)_itemsStack.Peek();
        parentResource.NestedResourceInfos.Add(nestedResourceInfoWrapper);
        _itemsStack.Push(nestedResourceInfoWrapper);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Writes all accumulated OData items to the output stream in CSV format.
    /// </summary>
    private void WriteItems()
    {
        if (_topLevelItem == null)
        {
            return;
        }

        IList<string>? headers = null;
        var topResourceSet = _topLevelItem as ODataResourceSetWrapper;
        var topResource = _topLevelItem as ODataResourceWrapper;

        // For simplicity, NOT consider the inheritance
        if (topResourceSet != null)
        {
            var firstResource = topResourceSet.Resources.FirstOrDefault();
            headers = BuildHeaders(firstResource);
        }
        else if (topResource != null)
        {
            headers = BuildHeaders(topResource);
        }

        if (headers == null)
        {
            return;
        }

        // write the head
        WriteHeader(headers);

        if (topResourceSet != null)
        {
            foreach (var resource in topResourceSet.Resources)
            {
                WriteResource(headers, resource);
            }
        }
        else if (topResource != null)
        {
            WriteResource(headers, topResource);
        }
    }

    /// <summary>
    /// Builds the list of headers based on resource properties and nested resources.
    /// </summary>
    /// <param name="topResource">The resource to extract headers from.</param>
    /// <returns>A list of header names or null if no resource is provided.</returns>
    private static IList<string>? BuildHeaders(ODataResourceWrapper? topResource)
    {
        if (topResource == null)
        {
            return null;
        }

        var headers = topResource.Resource.Properties
            .Select(property => property.Name).ToList();

        headers.AddRange(topResource.NestedResourceInfos
            .Select(nestedProperty => nestedProperty.NestedResourceInfo.Name));

        return headers;
    }

    /// <summary>
    /// Writes the header row to the CSV output.
    /// </summary>
    /// <param name="headers">The collection of header names to write.</param>
    private void WriteHeader(IEnumerable<string> headers)
    {
        var index = 0;
        foreach (var header in headers)
        {
            if (index == 0)
            {
                index = 1;
                _context?.Writer?.Write("{0}", header);
            }
            else
            {
                _context?.Writer?.Write(",{0}", header);
            }
        }

        _context?.Writer?.WriteLine();
    }

    /// <summary>
    /// Writes a resource as a row in the CSV output.
    /// </summary>
    /// <param name="headers">The headers that define the columns to write.</param>
    /// <param name="resource">The resource containing the data to write.</param>
    private void WriteResource(IEnumerable<string> headers, ODataResourceWrapper resource)
    {
        var index = 0;
        foreach (var header in headers)
        {
            if (index != 0)
            {
                _context?.Writer?.Write(",");
            }

            ++index;

            var propertyName = header;
            var propertyInfo = resource.Resource.Properties.SingleOrDefault(p => p.Name == propertyName);
            
            if (propertyInfo is ODataProperty property)
            {
                var propertyValueString = GetValueString(property.Value);
                _context?.Writer?.Write(propertyValueString);
                continue;
            }

            var nestedResourceInfoWrapper =
                resource.NestedResourceInfos.SingleOrDefault(n => n.NestedResourceInfo.Name == propertyName);
            if (nestedResourceInfoWrapper == null) continue;
            var sb = new StringBuilder();
            WriteResourceString(sb, nestedResourceInfoWrapper);
            _context?.Writer?.Write("\"");
            _context?.Writer?.Write(sb.ToString());
            _context?.Writer?.Write("\"");
        }

        _context?.Writer?.WriteLine();
    }

    /// <summary>
    /// Converts an OData value to its string representation suitable for CSV.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A string representation of the value with appropriate escaping for CSV.</returns>
    private static string GetValueString(object? value)
    {
        switch (value)
        {
            case null:
                return "\"\"";
            case ODataEnumValue enumValue:
                return enumValue.Value;
            case ODataCollectionValue collectionValue:
            {
                var sb = new StringBuilder("\"[");
                var index = 0;
                foreach (var item in collectionValue.Items)
                {
                    var itemStr = GetValueString(item);
                    if (index != 0)
                    {
                        sb.Append(',');
                    }

                    sb.Append(itemStr);
                    ++index;
                }

                sb.Append("]\"");

                return sb.ToString();
            }
            default:
            {
                // If the value is of type Microsoft.Spatial.GeographyPoint, convert it to a string
                if (value is GeographyPoint geographyPoint)
                {
                    return "POINT(" + geographyPoint.Longitude + " " + geographyPoint.Latitude + ")";
                }

                var str = value.ToString() ?? string.Empty;

                return str.Contains(',') ? $"\"{str.Replace("\"", "\"\"")}\"" : str;
            }
        }
    }

    /// <summary>
    /// Writes a nested resource information to a string builder.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="nestedResourceInfoWrapper">The nested resource information to write.</param>
    private void WriteResourceString(StringBuilder sb, ODataNestedResourceInfoWrapper nestedResourceInfoWrapper)
    {
        foreach (var childItem in nestedResourceInfoWrapper.NestedItems)
        {
            switch (childItem)
            {
                case ODataResourceSetWrapper resourceSetWrapper:
                {
                    sb.Append('[');
                    var index = 0;
                    foreach (var resource in resourceSetWrapper.Resources)
                    {
                        if (index != 0)
                        {
                            sb.Append(',');
                        }

                        ++index;
                        WriteResourceString(sb, resource);
                    }

                    sb.Append(']');

                    continue;
                }
                case ODataResourceWrapper resourceWrapper:
                    WriteResourceString(sb, resourceWrapper);
                    break;
            }
        }
    }

    /// <summary>
    /// Writes a resource to a string builder.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="resourceWrapper">The resource to write.</param>
    private void WriteResourceString(StringBuilder sb, ODataResourceWrapper resourceWrapper)
    {
        sb.Append('{');
        var index = 0;
        foreach (var propertyInfo in resourceWrapper.Resource.Properties)
        {
            if (propertyInfo is not ODataProperty property) continue;

            if (index != 0)
            {
                sb.Append(',');
            }

            ++index;
            sb.Append(property.Name);
            sb.Append('=');
            sb.Append(GetValueString(property.Value));
        }

        foreach (var nestedProperty in resourceWrapper.NestedResourceInfos)
        {
            sb.Append(nestedProperty.NestedResourceInfo.Name);
            sb.Append('=');
            WriteResourceString(sb, nestedProperty);
        }

        sb.Append('}');
    }

    #region Synchronization not used

    /// <summary>
    /// Not implemented. Use WriteStartAsync instead.
    /// </summary>
    /// <param name="resource">The resource to write.</param>
    /// <exception cref="NotImplementedException">Always thrown as this method is not implemented.</exception>
    public override void WriteStart(ODataResource resource)
        => throw new NotImplementedException();

    /// <summary>
    /// Not implemented. Use WriteStartAsync instead.
    /// </summary>
    /// <param name="resourceSet">The resource set to write.</param>
    /// <exception cref="NotImplementedException">Always thrown as this method is not implemented.</exception>
    public override void WriteStart(ODataResourceSet resourceSet)
        => throw new NotImplementedException();

    /// <summary>
    /// Not implemented. Use WriteStartAsync instead.
    /// </summary>
    /// <param name="nestedResourceInfo">The nested resource info to write.</param>
    /// <exception cref="NotImplementedException">Always thrown as this method is not implemented.</exception>
    public override void WriteStart(ODataNestedResourceInfo nestedResourceInfo)
        => throw new NotImplementedException();

    /// <summary>
    /// Not implemented. This writer does not support entity reference links.
    /// </summary>
    /// <param name="entityReferenceLink">The entity reference link.</param>
    /// <exception cref="NotImplementedException">Always thrown as this method is not implemented.</exception>
    public override void WriteEntityReferenceLink(ODataEntityReferenceLink entityReferenceLink)
        => throw new NotImplementedException();

    #endregion
}