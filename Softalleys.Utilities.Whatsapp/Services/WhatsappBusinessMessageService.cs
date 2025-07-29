using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));
        }

        if (string.IsNullOrWhiteSpace(options.Value.WhatsAppBusinessAccountId))
        {
            throw new InvalidOperationException("WhatsApp Business Account ID is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.Value.DefaultRecipient))
        {
            throw new InvalidOperationException("Default recipient is not configured. Set WhatsappBusinessOptions.DefaultRecipient.");
        }

        var httpClient = httpClientFactory.CreateClient("WhatsappBusinessApi");

        var request = new WhatsappTextMessageRequest(
            MessagingProduct: "whatsapp",
            To: options.Value.DefaultRecipient,
            Type: "text",
            Text: new WhatsappTextContent(Body: message)
        );

        try
        {
            var response = await httpClient.PostAsJsonAsync(
                $"{options.Value.ApiVersion}/{options.Value.WhatsAppBusinessAccountId}/messages",
                request,
                cancellationToken);

            return await HandleResponseAsync(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to send WhatsApp text message: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new OperationCanceledException("The WhatsApp message send operation was cancelled.", ex);
        }
    }

    public async Task<WhatsappMessageResult> SendMessageAsync(
        WhatsappTemplateMessage message,
        CancellationToken cancellationToken = default)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (string.IsNullOrWhiteSpace(message.To))
        {
            throw new ArgumentException("Recipient phone number cannot be null or empty.", nameof(message));
        }

        if (message.Template == null)
        {
            throw new ArgumentException("Template cannot be null.", nameof(message));
        }

        if (string.IsNullOrWhiteSpace(message.Template.Name))
        {
            throw new ArgumentException("Template name cannot be null or empty.", nameof(message));
        }

        if (string.IsNullOrWhiteSpace(options.Value.WhatsAppBusinessAccountId))
        {
            throw new InvalidOperationException("WhatsApp Business Account ID is not configured.");
        }

        if (!IsValidPhoneNumber(message.To))
        {
            throw new ArgumentException("Invalid phone number format. Phone number should contain only digits.", nameof(message));
        }

        var httpClient = httpClientFactory.CreateClient("WhatsappBusinessApi");

        var request = new WhatsappTemplateMessageRequest(
            MessagingProduct: "whatsapp",
            To: message.To,
            Type: "template",
            Template: new WhatsappTemplate(
                Name: message.Template.Name,
                Language: new WhatsappLanguage(Code: message.Template.Language),
                Components: message.Components?.Select(c => new WhatsappComponent(
                    Type: c.Type,
                    Parameters: c.Parameters?.ToList() ?? new List<WhatsappTemplateParameter>()
                )).ToList() ?? new List<WhatsappComponent>()
            ));

        try
        {
            var response = await httpClient.PostAsJsonAsync(
                $"{options.Value.ApiVersion}/{options.Value.WhatsAppBusinessAccountId}/messages",
                request,
                cancellationToken);

            return await HandleResponseAsync(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to send WhatsApp template message: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new OperationCanceledException("The WhatsApp message send operation was cancelled.", ex);
        }
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Allow phone numbers with optional + prefix and only digits
        return Regex.IsMatch(phoneNumber, @"^\+?\d{10,15}$");
    }

    private async Task<WhatsappMessageResult> HandleResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var successResponse = await response.Content.ReadFromJsonAsync<WhatsappApiSuccessResponse>(cancellationToken: cancellationToken);
            if (successResponse?.Messages?.FirstOrDefault() is var messageInfo && messageInfo != null)
            {
                return new WhatsappMessageResult(messageInfo.Id, WhatsappMessageStatus.Accepted);
            }
            throw new InvalidOperationException("Invalid success response format from WhatsApp API");
        }

        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        WhatsappApiErrorResponse? errorResponse = null;
        
        try
        {
            errorResponse = await response.Content.ReadFromJsonAsync<WhatsappApiErrorResponse>(cancellationToken: cancellationToken);
        }
        catch
        {
            // If we can't parse the error response, fall back to the raw content
        }

        var errorMessage = errorResponse?.Error?.Message ?? $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
        var status = response.StatusCode == System.Net.HttpStatusCode.Unauthorized ? WhatsappMessageStatus.AuthError : WhatsappMessageStatus.Failed;
        
        throw new InvalidOperationException($"WhatsApp API error: {errorMessage}. Response: {errorContent}");
    }

    // DTOs for text messages
    public record WhatsappTextMessageRequest(
        [property: JsonPropertyName("messaging_product")] string MessagingProduct,
        [property: JsonPropertyName("to")] string To,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("text")] WhatsappTextContent Text
    );

    public record WhatsappTextContent(
        [property: JsonPropertyName("body")] string Body
    );

    // Updated DTO for template messages
    public record WhatsappTemplateMessageRequest(
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

    // API Response DTOs
    public record WhatsappApiSuccessResponse(
        [property: JsonPropertyName("messaging_product")] string MessagingProduct,
        [property: JsonPropertyName("contacts")] List<WhatsappContact>? Contacts,
        [property: JsonPropertyName("messages")] List<WhatsappMessageInfo>? Messages
    );

    public record WhatsappContact(
        [property: JsonPropertyName("input")] string Input,
        [property: JsonPropertyName("wa_id")] string WaId
    );

    public record WhatsappMessageInfo(
        [property: JsonPropertyName("id")] string Id
    );

    public record WhatsappApiErrorResponse(
        [property: JsonPropertyName("error")] WhatsappErrorDetails? Error
    );

    public record WhatsappErrorDetails(
        [property: JsonPropertyName("message")] string? Message,
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("code")] int? Code,
        [property: JsonPropertyName("error_subcode")] int? ErrorSubcode,
        [property: JsonPropertyName("fbtrace_id")] string? FbtraceId
    );
}