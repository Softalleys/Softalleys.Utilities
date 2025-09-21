ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the entire repository (project uses intra-repo project references)
COPY . .

# Restore and publish Ping
RUN dotnet restore "Softalleys.Utilities.Events.Distributed.GooglePubSub.Example/Ping/Ping.csproj"
RUN dotnet publish "Softalleys.Utilities.Events.Distributed.GooglePubSub.Example/Ping/Ping.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Ping.dll"]
