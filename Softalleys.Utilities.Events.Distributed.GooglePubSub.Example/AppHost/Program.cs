using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Google Pub/Sub emulator container
var pubsub = builder.AddContainer("pubsub-emulator", "messagebird/gcloud-pubsub-emulator:latest")
    .WithEndpoint(8085)
    .WithEnvironment("PUBSUB_PROJECT_ID", "local-project");

// Pong web app (subscriber)
var pong = builder.AddProject("pong", "..\\Pong\\Pong.csproj")
    .WithEnvironment("ASPNETCORE_URLS", "http://localhost:5187")
    .WithEnvironment("PUBSUB_EMULATOR_HOST", "localhost:8085")
    .WithEnvironment("CLOUDSDK_API_ENDPOINT_OVERRIDES_PUBSUB", "http://localhost:8085/")
    .WithEnvironment("Softalleys__Events__Distributed__GooglePubSub__ProjectId", "local-project")
    .WithEnvironment("Softalleys__Events__Distributed__GooglePubSub__TopicId", "events")
    .WithEnvironment("Pong__SubscriptionId", "pong-sub");

// Ping console app (publisher)
var ping = builder.AddProject("ping", "..\\Ping\\Ping.csproj")
    .WithEnvironment("PUBSUB_EMULATOR_HOST", "localhost:8085")
    .WithEnvironment("CLOUDSDK_API_ENDPOINT_OVERRIDES_PUBSUB", "http://localhost:8085/")
    .WithEnvironment("Softalleys__Events__Distributed__GooglePubSub__ProjectId", "local-project")
    .WithEnvironment("Softalleys__Events__Distributed__GooglePubSub__TopicId", "events");

builder.Build().Run();
