using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.OData;

namespace Softalleys.Utilities.Formatters.OData;

/// <summary>
/// Provides a base abstraction for customizing the OData writing process.
/// </summary>
public abstract class CustomizedWriter : ODataWriter
{
    // Private field to hold the top-level OData item.
    private ODataItemWrapper? _topLevelItem;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomizedWriter"/> class with the specified context.
    /// </summary>
    /// <param name="context">The customized output context.</param>
    protected CustomizedWriter(CustomizedOutputContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Gets the top-level OData item.
    /// </summary>
    protected ODataItemWrapper? TopLevelItem => _topLevelItem;

    /// <summary>
    /// Gets the stack that holds OData items during the writing process.
    /// </summary>
    protected Stack<ODataItemWrapper> ItemsStack { get; } = new();

    /// <summary>
    /// Gets the customized output context.
    /// </summary>
    protected CustomizedOutputContext Context { get; }

    /// <summary>
    /// Flushes any buffered output by delegating to the output context.
    /// </summary>
    public override void Flush()
    {
        Context.Flush();
    }

    /// <summary>
    /// Writes the end of an OData item. If the items stack is completed, writes the accumulated items and flushes the context.
    /// </summary>
    public override void WriteEnd()
    {
        ItemsStack.Pop();

        if (ItemsStack.Count == 0)
        {
            // We finished the process, so write the items and flush the context.
            WriteItems();
            Flush();
        }
    }

    /// <summary>
    /// When implemented in a derived class, writes the accumulated OData items into the output stream.
    /// </summary>
    protected abstract void WriteItems();

    /// <summary>
    /// Writes the start of an OData resource set asynchronously.
    /// </summary>
    /// <param name="resourceSet">The OData resource set to write.</param>
    /// <returns>A completed task.</returns>
    public override Task WriteStartAsync(ODataResourceSet resourceSet)
    {
        ODataResourceSetWrapper resourceSetWrapper = new ODataResourceSetWrapper(resourceSet);

        if (_topLevelItem == null)
        {
            _topLevelItem = resourceSetWrapper;
        }
        else
        {
            // It must be under nested resource info.
            ODataNestedResourceInfoWrapper parentNestedResourceInfo = (ODataNestedResourceInfoWrapper)ItemsStack.Peek();
            parentNestedResourceInfo.NestedItems.Add(resourceSetWrapper);
        }

        ItemsStack.Push(resourceSetWrapper);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Writes the start of an OData resource asynchronously.
    /// </summary>
    /// <param name="resource">The OData resource to write.</param>
    /// <returns>A completed task.</returns>
    public override Task WriteStartAsync(ODataResource resource)
    {
        ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(resource);
        if (_topLevelItem == null)
        {
            _topLevelItem = resourceWrapper;
        }
        else
        {
            ODataItemWrapper parentItem = ItemsStack.Peek();

            if (parentItem is ODataResourceSetWrapper parentResourceSet)
            {
                parentResourceSet.Resources.Add(resourceWrapper);
            }
            else
            {
                ODataNestedResourceInfoWrapper parentNestedResource = (ODataNestedResourceInfoWrapper)parentItem;
                parentNestedResource.NestedItems.Add(resourceWrapper);
            }
        }

        ItemsStack.Push(resourceWrapper);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Writes the start of an OData nested resource information asynchronously.
    /// </summary>
    /// <param name="nestedResourceInfo">The nested resource information to write.</param>
    /// <returns>A completed task.</returns>
    public override Task WriteStartAsync(ODataNestedResourceInfo nestedResourceInfo)
    {
        ODataNestedResourceInfoWrapper nestedResourceInfoWrapper =
            new ODataNestedResourceInfoWrapper(nestedResourceInfo);
        ODataResourceWrapper parentResource = (ODataResourceWrapper)ItemsStack.Peek();
        parentResource.NestedResourceInfos.Add(nestedResourceInfoWrapper);
        ItemsStack.Push(nestedResourceInfoWrapper);
        return Task.CompletedTask;
    }

    #region Synchronization not used

    /// <summary>
    /// This method is not implemented.
    /// </summary>
    /// <param name="resource">The OData resource.</param>
    public override void WriteStart(ODataResource resource) => throw new NotImplementedException();

    /// <summary>
    /// This method is not implemented.
    /// </summary>
    /// <param name="resourceSet">The OData resource set.</param>
    public override void WriteStart(ODataResourceSet resourceSet) => throw new NotImplementedException();

    /// <summary>
    /// This method is not implemented.
    /// </summary>
    /// <param name="nestedResourceInfo">The OData nested resource information.</param>
    public override void WriteStart(ODataNestedResourceInfo nestedResourceInfo) => throw new NotImplementedException();

    /// <summary>
    /// This method is not implemented.
    /// </summary>
    /// <param name="entityReferenceLink">The OData entity reference link.</param>
    public override void WriteEntityReferenceLink(ODataEntityReferenceLink entityReferenceLink) =>
        throw new NotImplementedException();

    #endregion
}