FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG GITHUB_USER
ARG GITHUB_TOKEN
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Создаём Nuget конфиг чтобы подтягивать Shared либы с Github Packages
RUN echo '<?xml version="1.0" encoding="utf-8"?> \
<configuration> \
  <packageSources> \
    <clear /> \
    <add key="GitHub" value="https://nuget.pkg.github.com/'$GITHUB_USER'/index.json" /> \
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" /> \
  </packageSources> \
  <packageSourceMapping> \
    <packageSource key="GitHub"> \
      <package pattern="Shared.*" /> \
    </packageSource> \
    <packageSource key="nuget.org"> \
      <package pattern="*" /> \
    </packageSource> \
  </packageSourceMapping> \
  <packageSourceCredentials> \
    <GitHub> \
      <add key="Username" value="'$GITHUB_USER'" /> \
      <add key="ClearTextPassword" value="'$GITHUB_TOKEN'" /> \
    </GitHub> \
  </packageSourceCredentials> \
</configuration>' > nuget.config

COPY . .
RUN dotnet restore "src/DirectoryService.WebAPI/DirectoryService.WebAPI.csproj"
WORKDIR "/src/src/DirectoryService.WebAPI"
RUN dotnet build "./DirectoryService.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DirectoryService.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DirectoryService.WebAPI.dll"]