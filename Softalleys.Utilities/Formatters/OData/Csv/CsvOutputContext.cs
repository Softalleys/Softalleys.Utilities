using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Softalleys.Utilities.Formatters.OData.Csv;

/// <summary>
/// Provides context for outputting OData in CSV format.
/// </summary>
/// <remarks>
/// This class extends ODataOutputContext to support writing OData resources in CSV format,
/// managing the underlying stream and writer resources.
/// </remarks>
public class CsvOutputContext : ODataOutputContext
{
    private Stream? _stream;
    
    /// <summary>
    /// Gets the text writer used to write CSV content.
    /// </summary>
    /// <value>The TextWriter instance or null if not initialized.</value>
    public TextWriter? Writer { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvOutputContext"/> class.
    /// </summary>
    /// <param name="format">The OData format to use.</param>
    /// <param name="settings">The message writer settings.</param>
    /// <param name="messageInfo">Information about the OData message.</param>
    public CsvOutputContext(ODataFormat format, ODataMessageWriterSettings settings, ODataMessageInfo messageInfo)
        : base(format, messageInfo, settings)
    {
        _stream = messageInfo.MessageStream;
        Writer = new StreamWriter(_stream);
    }
    
    /// <summary>
    /// Creates an OData resource set writer for writing collections in CSV format.
    /// </summary>
    /// <param name="entitySet">The entity set being written.</param>
    /// <param name="resourceType">The type of resources in the set.</param>
    /// <returns>A task that returns an ODataWriter configured for CSV output.</returns>
    public override Task<ODataWriter> CreateODataResourceSetWriterAsync(IEdmEntitySetBase entitySet, IEdmStructuredType resourceType)
        => Task.FromResult<ODataWriter>(new CsvWriter(this));

    /// <summary>
    /// Creates an OData resource writer for writing individual resources in CSV format.
    /// </summary>
    /// <param name="navigationSource">The navigation source for the resource.</param>
    /// <param name="resourceType">The type of resource being written.</param>
    /// <returns>A task that returns an ODataWriter configured for CSV output.</returns>
    public override Task<ODataWriter> CreateODataResourceWriterAsync(IEdmNavigationSource navigationSource, IEdmStructuredType resourceType)
        => Task.FromResult<ODataWriter>(new CsvWriter(this));

    /// <summary>
    /// Flushes any buffered content to the underlying stream.
    /// </summary>
    public void Flush() => _stream?.Flush();

    /// <summary>
    /// Releases the unmanaged resources used by the output context and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                Writer?.Dispose();
                _stream?.Dispose();
            }
            finally
            {
                Writer = null;
                _stream = null;
            }
        }

        base.Dispose(disposing);
    }
}