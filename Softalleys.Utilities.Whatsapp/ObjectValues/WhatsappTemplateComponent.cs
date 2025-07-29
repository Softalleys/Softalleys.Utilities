namespace Softalleys.Utilities.Whatsapp.ObjectValues;

/// <summary>
/// Represents a component of a WhatsApp template, such as header, body, or footer.
/// </summary>
/// <param name="Type">
/// The type of the template component (e.g., header, body, footer, button).
/// </param>
/// <param name="Parameters">
/// The list of parameters associated with this component.
/// </param>
public record WhatsappTemplateComponent(string Type, IList<WhatsappTemplateParameter> Parameters);