using Microsoft.OData;
using Microsoft.OData.Edm;
using Softalleys.Utilities.Formatters.OData.Cbor;
using Softalleys.Utilities.Formatters.OData.Yaml;

namespace Softalleys.Utilities.Formatters.OData;

    /// <summary>
    /// Represents a customized OData output context that supports additional media types.
    /// </summary>
    public class CustomizedOutputContext : ODataOutputContext
    {
        /// <summary>
        /// Holds the media type information for determining the OData writer.
        /// </summary>
        private ODataMediaType? _mediaType;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomizedOutputContext"/> class.
        /// </summary>
        /// <param name="format">The OData format.</param>
        /// <param name="settings">The writer settings.</param>
        /// <param name="messageInfo">The message information including the output stream and media type.</param>
        public CustomizedOutputContext(ODataFormat format, ODataMessageWriterSettings settings, ODataMessageInfo messageInfo)
            : base(format, messageInfo, settings)
        {
            Stream = messageInfo.MessageStream;
            _mediaType = messageInfo.MediaType;
            Writer = new StreamWriter(Stream);
        }

        /// <summary>
        /// Gets the output stream associated with this context.
        /// </summary>
        public Stream? Stream { get; private set; }

        /// <summary>
        /// Gets the text writer used for writing output.
        /// </summary>
        public TextWriter? Writer { get; }

        /// <summary>
        /// Asynchronously creates an OData writer for writing a resource set.
        /// </summary>
        /// <param name="entitySet">The entity set representing the resource set.</param>
        /// <param name="resourceType">The structured type of the resources.</param>
        /// <returns>A task that represents the asynchronous operation, containing the OData writer.</returns>
        public override Task<ODataWriter> CreateODataResourceSetWriterAsync(IEdmEntitySetBase entitySet, IEdmStructuredType resourceType)
        {
            var writer = CreateWriter();
            return Task.FromResult(writer);
        }

        /// <summary>
        /// Asynchronously creates an OData writer for writing a single resource.
        /// </summary>
        /// <param name="navigationSource">The navigation source for the resource.</param>
        /// <param name="resourceType">The structured type of the resource.</param>
        /// <returns>A task that represents the asynchronous operation, containing the OData writer.</returns>
        public override Task<ODataWriter> CreateODataResourceWriterAsync(IEdmNavigationSource navigationSource, IEdmStructuredType resourceType)
        {
            var writer = CreateWriter();
            return Task.FromResult(writer);
        }

        /// <summary>
        /// Flushes the underlying stream.
        /// </summary>
        public void Flush()
        {
            Stream?.Flush();
        }

        /// <summary>
        /// Disposes the managed resources used by the context.
        /// </summary>
        /// <param name="disposing">A value indicating whether to dispose managed resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    Writer?.Dispose();
                    Stream?.Dispose();
                }
                finally
                {
                    Stream = null;
                    _mediaType = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Creates an appropriate OData writer based on the configured media type.
        /// </summary>
        /// <returns>An instance of <see cref="ODataWriter"/> for the specified media type.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the media type is not valid for this output context.
        /// </exception>
        private ODataWriter CreateWriter()
        {
            switch (_mediaType?.Type)
            {
                case "text" when _mediaType.SubType == "csv":
                    // return new CsvWriter(this, resourceType);
                    // Keep Csv separated for clear post
                    break;
                case "application" when _mediaType.SubType == "yaml":
                    return new YamlODataWriter(this);
                case "application" when _mediaType.SubType == "cbor":
                    return new CborODataWriter(this);
            }

            throw new InvalidOperationException($"Not valid '{_mediaType?.Type}/{_mediaType?.SubType}' for this output context.");
        }
    }
