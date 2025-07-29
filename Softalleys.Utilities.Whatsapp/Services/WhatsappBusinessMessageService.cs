using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Softalleys.Utilities.Whatsapp.ObjectValues;
using Softalleys.Utilities.Whatsapp.Options;
using Softalleys.Utilities.Whatsapp.Results;

namespace Softalleys.Utilities.Whatsapp.Services;

public class WhatsappBusinessMessageService(
    IOptions<WhatsappBusinessOptions> options,
    IHttpClientFactory httpClientFactory) : IWhatsappMessageService
{
    public async Task<WhatsappMessageResult> SendMessageAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<WhatsappMessageResult> SendMessageAsync(
        WhatsappTemplateMessage message,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("WhatsappBusinessApi");

        var request = new WhatsappMessageRequest(
            MessagingProduct: "whatsapp",
            To: message.To,
            Type: "template",
            Template: new WhatsappTemplate(
                Name: message.Template.Name,
                Language: new WhatsappLanguage(Code: message.Template.Language),
                Components: message.Components.Select(c => new WhatsappComponent(
                    Type: c.Type,
                    Parameters: c.Parameters.ToList()
                )).ToList()
            ));

        var response = await httpClient.PostAsJsonAsync(
            $"{options.Value.ApiVersion}/{options.Value.WhatsAppBusinessAccountId}/messages",
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<WhatsappMessageResult>(cancellationToken: cancellationToken);
            return result ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        throw new NotImplementedException("Response handling not implemented yet");
    }

    public record WhatsappMessageRequest(
        [property: JsonPropertyName("messaging_product")] string MessagingProduct,
        [property: JsonPropertyName("to")] string To,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("template")] WhatsappTemplate Template
    );

    public record WhatsappTemplate(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("language")] WhatsappLanguage Language,
        [property: JsonPropertyName("components")] List<WhatsappComponent> Components
    );

    public record WhatsappLanguage(
        [property: JsonPropertyName("code")] string Code
    );

    public record WhatsappComponent(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("parameters")] List<WhatsappTemplateParameter> Parameters
    );
}