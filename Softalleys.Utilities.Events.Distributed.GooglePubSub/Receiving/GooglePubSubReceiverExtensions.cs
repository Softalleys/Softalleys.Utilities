using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Softalleys.Utilities.Events.Distributed.GooglePubSub.Options;
using Softalleys.Utilities.Events.Distributed.Receiving;

namespace Softalleys.Utilities.Events.Distributed.GooglePubSub.Receiving;

public static class GooglePubSubReceiverExtensions
{
    public static IEndpointRouteBuilder MapGooglePubSubReceiver(this IEndpointRouteBuilder endpoints, string? route = null)
    {
        var appServices = endpoints.ServiceProvider;
        var options = appServices.GetRequiredService<IOptions<GooglePubSubDistributedEventsOptions>>().Value;
        route ??= options.SubscribePath;

        endpoints.MapPost(route, async (HttpContext ctx, IDistributedEventReceiver receiver, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("GooglePubSubReceiver");
            logger.LogDebug("Received PubSub push request at {Path}", ctx.Request.Path);
            // Validate JWT if required
            if (options.RequireJwtValidation)
            {
                var token = ctx.Request.Headers["Authorization"].ToString();
                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    token = token.Substring("Bearer ".Length);
                if (string.IsNullOrWhiteSpace(token))
                    return Results.Unauthorized();

                if (options.CustomJwtValidator != null)
                {
                    var ok = await options.CustomJwtValidator(token);
                    if (!ok) return Results.Unauthorized();
                }
                else
                {
                    var handler = new JwtSecurityTokenHandler();
                    var parameters = new TokenValidationParameters
                    {
                        ValidateAudience = !string.IsNullOrWhiteSpace(options.Audience),
                        ValidAudience = options.Audience,
                        ValidateIssuer = !string.IsNullOrWhiteSpace(options.Issuer),
                        ValidIssuer = options.Issuer,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = false // Can be enhanced with JWKS retrieval if configured
                    };
                    try
                    {
                        handler.ValidateToken(token, parameters, out _);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "JWT validation failed");
                        return Results.Unauthorized();
                    }
                }
            }

            // Parse Google PubSub push JSON { message: { data: base64, attributes: {...} }, subscription: "..." }
            using var doc = await JsonDocument.ParseAsync(ctx.Request.Body);
            if (!doc.RootElement.TryGetProperty("message", out var message))
                return Results.BadRequest();

            var dataB64 = message.GetProperty("data").GetString();
            var bytes = string.IsNullOrWhiteSpace(dataB64) ? Array.Empty<byte>() : Convert.FromBase64String(dataB64);

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (message.TryGetProperty("attributes", out var attrs) && attrs.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in attrs.EnumerateObject())
                {
                    headers[prop.Name] = prop.Value.GetString() ?? string.Empty;
                }
            }

            var contentType = headers.TryGetValue("contentType", out var ct) ? ct : "application/json";

            logger.LogDebug("Push message has {AttrCount} attributes; contentType={ContentType}", headers.Count, contentType);
            var outcome = await receiver.ProcessAsync(new DistributedInboundMessage(bytes, contentType, null, null, headers, "google-pubsub"), ctx.RequestAborted);
            // Acknowledge push with 200 OK on success; non-success returns 500 to trigger retry
            return outcome == InboundProcessOutcome.Success ? Results.Ok() : Results.StatusCode(500);
        });

        return endpoints;
    }
}
