using System.Text.Json.Serialization;
namespace Softalleys.Utilities.Whatsapp.ObjectValues;

/// <summary>
/// Represents a generic WhatsApp template parameter.
/// </summary>
/// <param name="Type">The type of the template parameter (e.g., text, media, button, quick_reply).</param>
public abstract record WhatsappTemplateParameter(
    [property: JsonPropertyName("type")] string Type);

/// <summary>
/// Represents a text parameter for a WhatsApp template.
/// </summary>
/// <param name="Text">The text content of the parameter.</param>
public record WhatsappTemplateParameterText(
    [property: JsonPropertyName("text")] string Text)
    : WhatsappTemplateParameter("text");

/// <summary>
/// Represents a media parameter for a WhatsApp template.
/// </summary>
/// <param name="MediaUrl">The URL of the media.</param>
/// <param name="Caption">The caption for the media.</param>
public record WhatsappTemplateParameterMedia(
    [property: JsonPropertyName("media_url")] string MediaUrl,
    [property: JsonPropertyName("caption")] string Caption)
    : WhatsappTemplateParameter("media");

/// <summary>
/// Represents a button parameter for a WhatsApp template.
/// </summary>
/// <param name="Text">The button text.</param>
/// <param name="Payload">The payload associated with the button.</param>
public record WhatsappTemplateParameterButton(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("payload")] string Payload)
    : WhatsappTemplateParameter("button");

/// <summary>
/// Represents a quick reply parameter for a WhatsApp template.
/// </summary>
/// <param name="Text">The quick reply text.</param>
/// <param name="Payload">The payload associated with the quick reply.</param>
public record WhatsappTemplateParameterQuickReply(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("payload")] string Payload)
    : WhatsappTemplateParameter("quick_reply");