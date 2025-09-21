using System;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Softalleys.Utilities.Events;
using Softalleys.Utilities.Events.Distributed.Configuration;
using Softalleys.Utilities.Events.Distributed.GooglePubSub;
using Softalleys.Utilities.Events.Distributed.GooglePubSub.Options;
using Softalleys.Utilities.Events.Distributed.Receiving;
using Softalleys.Utilities.Events.Distributed.GooglePubSub.Receiving;
using Xunit;

namespace Softalleys.Utilities.Events.Tests.Distributed;

public class GooglePubSubTransportTests
{
    private static string CreatePushBody(byte[] data, IDictionary<string,string>? attributes = null)
    {
        var body = new
        {
            message = new
            {
                data = Convert.ToBase64String(data),
                attributes = attributes ?? new Dictionary<string,string>()
            },
            subscription = "projects/test/subscriptions/dummy"
        };
        return JsonSerializer.Serialize(body);
    }

    [Fact]
    public async Task MapReceiver_DefaultRoute_NoJwt_Succeeds()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRouting();
        builder.Services.AddLogging();
        builder.Services.AddSingleton<IEventBus, EventBus>();
        builder.Services
            .AddSoftalleysEvents()
            .AddDistributedEvents(d =>
            {
                d.UseGooglePubSub(g => g.Configure(o =>
                {
                    o.RequireJwtValidation = false; // bypass JWT in test
                }));
            });
        builder.Services.AddSingleton<IDistributedEventReceiver, DummyReceiver>();

        var app = builder.Build();
        app.MapGooglePubSubReceiver();
        await app.StartAsync();

        var client = app.GetTestClient();
        var payload = Encoding.UTF8.GetBytes("{\"hello\":\"world\"}");
        var body = CreatePushBody(payload, new Dictionary<string, string> { { "contentType", "application/json" } });
        var resp = await client.PostAsync("/.well-known/events/subscribe", new StringContent(body, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var receiver = app.Services.GetRequiredService<IDistributedEventReceiver>() as DummyReceiver;
        Assert.NotNull(receiver);
        Assert.True(receiver!.WasCalled);
    }

    [Fact]
    public async Task MapReceiver_CustomRoute_CustomValidator_Denies()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRouting();
        builder.Services.AddLogging();
        builder.Services.AddSingleton<IEventBus, EventBus>();
        builder.Services
            .AddSoftalleysEvents()
            .AddDistributedEvents(d =>
            {
                d.UseGooglePubSub(g => g.Configure(o =>
                {
                    o.RequireJwtValidation = true;
                    o.CustomJwtValidator = _ => Task.FromResult(false);
                }));
            });
        builder.Services.AddSingleton<IDistributedEventReceiver, DummyReceiver>();

        var app = builder.Build();
        app.MapGooglePubSubReceiver("/custom/subscribe");
        await app.StartAsync();

        var client = app.GetTestClient();
        var payload = Encoding.UTF8.GetBytes("{}");
        var body = CreatePushBody(payload);
        var req = new HttpRequestMessage(HttpMethod.Post, "/custom/subscribe");
        req.Content = new StringContent(body, Encoding.UTF8, "application/json");
        req.Headers.Add("Authorization", "Bearer dummy.invalid.token");
        var resp = await client.SendAsync(req);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        var receiver = app.Services.GetRequiredService<IDistributedEventReceiver>() as DummyReceiver;
        Assert.False(receiver!.WasCalled);
    }

    private sealed class DummyReceiver : IDistributedEventReceiver
    {
        public bool WasCalled { get; private set; }
        public Task<InboundProcessOutcome> ProcessAsync(DistributedInboundMessage message, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(InboundProcessOutcome.Success);
        }
    }
}
