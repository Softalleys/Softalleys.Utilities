namespace Softalleys.Utilities.Whatsapp.ObjectValues;

/// <summary>
/// Represents a WhatsApp template with a name and language.
/// </summary>
/// <param name="Name">
/// The name of the WhatsApp template.
/// </param>
/// <param name="Language">
/// The language code for the template (default is "en_US").
/// </param>
public record WhatsappTemplate(string Name, string Language = "en_US");