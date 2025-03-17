using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.OData;

namespace Softalleys.Utilities.Formatters.OData.Yaml;

/// <summary>
/// Implements an OData writer that formats output in YAML format.
/// Inherits from CustomizedWriter to provide YAML-specific writing functionality.
/// </summary>
public class YamlODataWriter : CustomizedWriter
{
    /// <summary>
    /// The string used for indentation in YAML output.
    /// </summary>
    protected const string IndentationString = "  ";

    /// <summary>
    /// Initializes a new instance of the <see cref="YamlODataWriter"/> class.
    /// </summary>
    /// <param name="context">The customized output context for writing YAML.</param>
    public YamlODataWriter(CustomizedOutputContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Writes the accumulated OData items in YAML format.
    /// </summary>
    protected override void WriteItems()
    {
        if (TopLevelItem == null)
        {
            return;
        }

        // For simplicity, NOT consider the inheritance
        if (TopLevelItem is ODataResourceSetWrapper topResourceSet)
        {
            WriteResourceSet(0, topResourceSet);
        }
        else if (TopLevelItem is ODataResourceWrapper topResource)
        {
            WriteResource(0, topResource);
        }
    }

    /// <summary>
    /// Writes a resource set in YAML format.
    /// When entering this method, the cursor is positioned at the first place to write the resource set.
    /// </summary>
    /// <param name="indentLevel">The current indentation level.</param>
    /// <param name="resourceSetWrapper">The resource set to write.</param>
    private void WriteResourceSet(int indentLevel, ODataResourceSetWrapper resourceSetWrapper)
    {
        if (resourceSetWrapper.Resources.Count == 0)
        {
            Context.Writer?.WriteLine("[ ]"); // write the empty
            return;
        }

        // @odata.count: 4  if we have the count
        if (resourceSetWrapper.ResourceSet.Count != null)
        {
            Context.Writer?.Write("@odata.count: ");
            Context.Writer?.WriteLine(resourceSetWrapper.ResourceSet.Count.Value);
        }

        var first = true;
        foreach (var resource in resourceSetWrapper.Resources)
        {
            if (!first)
            {
                Context.Writer?.WriteLine();
                WriteIndentation(indentLevel);
            }
            else
            {
                first = false;
            }

            Context.Writer?.Write("- ");
            WriteResource(indentLevel + 1, resource);
        }
    }

    /// <summary>
    /// Writes a single resource in YAML format.
    /// When entering this method, the cursor is positioned at the first place to write the resource.
    /// </summary>
    /// <param name="indentLevel">The current indentation level.</param>
    /// <param name="resourceWrapper">The resource to write.</param>
    private void WriteResource(int indentLevel, ODataResourceWrapper resourceWrapper)
    {
        bool first = true;
        foreach (var property in resourceWrapper.Resource.Properties)
        {
            if (!first)
            {
                Context.Writer?.WriteLine();
                WriteIndentation(indentLevel);
            }
            else
            {
                first = false;
            }

            Context.Writer?.Write($"{property.Name}: ");

            // Cast or convert ODataPropertyInfo to ODataProperty to access the Value property
            if (property is ODataProperty prop)
            {
                WriteValue(prop.Value, indentLevel);
            }
            else
            {
                // Handle the case where propertyInfo is not an ODataProperty
                Context.Writer?.Write("null");
            }
        }

        foreach (var property in resourceWrapper.NestedResourceInfos)
        {
            if (!first)
            {
                Context.Writer?.WriteLine();
                WriteIndentation(indentLevel);
            }
            else
            {
                first = false;
            }

            Context.Writer?.Write($"{property.NestedResourceInfo.Name}: ");

            WriteNestedProperty(indentLevel + 1, property);
        }

        Context.Writer?.WriteLine();
    }

    /// <summary>
    /// Writes a nested property in YAML format.
    /// </summary>
    /// <param name="indentLevel">The current indentation level.</param>
    /// <param name="nestedResourceInfoWrapper">The nested resource information to write.</param>
    private void WriteNestedProperty(int indentLevel, ODataNestedResourceInfoWrapper nestedResourceInfoWrapper)
    {
        foreach (var childItem in nestedResourceInfoWrapper.NestedItems)
        {
            switch (childItem)
            {
                // if it's empty collection, write [] and return;
                case ODataResourceSetWrapper resourceSetWrapper when resourceSetWrapper.Resources.Count == 0:
                    Context.Writer?.WriteLine("[ ]"); // write the empty
                    continue;
                case ODataResourceSetWrapper resourceSetWrapper:
                    Context.Writer?.WriteLine();
                    WriteIndentation(indentLevel);
                    WriteResourceSet(indentLevel, resourceSetWrapper);
                    continue;
                case ODataResourceWrapper resourceWrapper:
                    Context.Writer?.WriteLine();
                    WriteIndentation(indentLevel);
                    WriteResource(indentLevel, resourceWrapper);
                    break;
            }
        }
    }

    /// <summary>
    /// Writes a value in YAML format based on its type.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="indentLevel">The current indentation level.</param>
    /// <exception cref="NotImplementedException">Thrown when an unsupported value type is encountered.</exception>
    protected void WriteValue(object value, int indentLevel)
    {
        var valueType = value.GetType();
        valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

        switch (value)
        {
            case ODataEnumValue enumValue:
                Context.Writer?.Write(enumValue.Value);
                break;
            case ODataCollectionValue collectionValue when collectionValue.Items.Count() == 0:
                Context.Writer?.Write("[ ]");
                break;
            case ODataCollectionValue collectionValue:
            {
                Context.Writer?.WriteLine();
                var first = true;
                foreach (var item in collectionValue.Items)
                {
                    if (!first)
                    {
                        Context.Writer?.WriteLine();
                    }
                    else
                    {
                        first = false;
                    }

                    WriteIndentation(indentLevel + 1);
                    Context.Writer?.Write("- ");
                    WriteValue(item, indentLevel + 1);
                }

                break;
            }
            default:
            {
                if (valueType == typeof(int))
                {
                    Context.Writer?.Write((int)value);
                }
                else if (valueType == typeof(bool))
                {
                    Context.Writer?.Write((bool)value);
                }
                else if (valueType == typeof(string))
                {
                    Context.Writer?.Write(value.ToString());
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

    /// <summary>
    /// Writes indentation to the output based on the specified level.
    /// </summary>
    /// <param name="indentLevel">The number of indentation levels to write.</param>
    public virtual void WriteIndentation(int indentLevel)
    {
        for (var i = 0; i < indentLevel; i++)
        {
            Context.Writer?.Write(IndentationString);
        }
    }
}