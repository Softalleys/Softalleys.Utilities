ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the entire repository (project uses intra-repo project references)
COPY . .

# Restore and publish Pong
RUN dotnet restore "Softalleys.Utilities.Events.Distributed.GooglePubSub.Example/Pong/Pong.csproj"
RUN dotnet publish "Softalleys.Utilities.Events.Distributed.GooglePubSub.Example/Pong/Pong.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:5187
EXPOSE 5187

ENTRYPOINT ["dotnet", "Pong.dll"]
