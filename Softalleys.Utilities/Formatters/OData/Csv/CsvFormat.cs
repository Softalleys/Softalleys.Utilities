using Microsoft.OData;

namespace Softalleys.Utilities.Formatters.OData.Csv;

/// <summary>
/// Provides OData formatting functionality for CSV (Comma-Separated Values) format.
/// This class handles the creation of input and output contexts for processing OData in CSV format.
/// </summary>
public class CsvFormat : ODataFormat
{
    /// <summary>
    /// Asynchronously creates an output context for writing OData in CSV format.
    /// </summary>
    /// <param name="messageInfo">Information about the OData message to be processed.</param>
    /// <param name="messageWriterSettings">Settings for the OData message writer.</param>
    /// <returns>A task that returns an OData output context for CSV format.</returns>
    public override Task<ODataOutputContext> CreateOutputContextAsync(
        ODataMessageInfo messageInfo, ODataMessageWriterSettings messageWriterSettings)
    {
        return Task.FromResult<ODataOutputContext>(
            new CsvOutputContext(this, messageWriterSettings, messageInfo));
    }

    /// <summary>
    /// Creates an input context for reading OData in CSV format.
    /// </summary>
    /// <param name="messageInfo">Information about the OData message to be processed.</param>
    /// <param name="messageReaderSettings">Settings for the OData message reader.</param>
    /// <returns>An OData input context.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented.</exception>
    public override ODataInputContext CreateInputContext(
        ODataMessageInfo messageInfo, ODataMessageReaderSettings messageReaderSettings)
        => throw new NotImplementedException();

    /// <summary>
    /// Asynchronously creates an input context for reading OData in CSV format.
    /// </summary>
    /// <param name="messageInfo">Information about the OData message to be processed.</param>
    /// <param name="messageReaderSettings">Settings for the OData message reader.</param>
    /// <returns>A task that returns an OData input context for CSV format.</returns>
    public override Task<ODataInputContext> CreateInputContextAsync(
        ODataMessageInfo messageInfo, ODataMessageReaderSettings messageReaderSettings)
    {
        return Task.FromResult<ODataInputContext>(
            new CustomizedInputContext(this, messageReaderSettings, messageInfo));
    }

    /// <summary>
    /// Creates an output context for writing OData in CSV format.
    /// </summary>
    /// <param name="messageInfo">Information about the OData message to be processed.</param>
    /// <param name="messageWriterSettings">Settings for the OData message writer.</param>
    /// <returns>An OData output context.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented.</exception>
    public override ODataOutputContext CreateOutputContext(
        ODataMessageInfo messageInfo, ODataMessageWriterSettings messageWriterSettings)
        => throw new NotImplementedException();

    /// <summary>
    /// Detects the payload kind of an OData message in CSV format.
    /// </summary>
    /// <param name="messageInfo">Information about the OData message to be processed.</param>
    /// <param name="settings">Settings for the OData message reader.</param>
    /// <returns>An enumeration of OData payload kinds.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented.</exception>
    public override IEnumerable<ODataPayloadKind> DetectPayloadKind(
        ODataMessageInfo messageInfo, ODataMessageReaderSettings settings)
        => throw new NotImplementedException();

    /// <summary>
    /// Asynchronously detects the payload kind of an OData message in CSV format.
    /// </summary>
    /// <param name="messageInfo">Information about the OData message to be processed.</param>
    /// <param name="settings">Settings for the OData message reader.</param>
    /// <returns>A task that returns an enumeration of OData payload kinds.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented.</exception>
    public override Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindAsync(
        ODataMessageInfo messageInfo, ODataMessageReaderSettings settings)
        => throw new NotImplementedException();
}