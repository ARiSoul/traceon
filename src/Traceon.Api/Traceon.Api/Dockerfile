# ── Build stage ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

# Copy Directory.Build.props and project files first (layer cache for restore)
COPY src/Traceon.Api/Traceon.Api/src/Directory.Build.props \
     src/Traceon.Api/Traceon.Api/src/

COPY src/Traceon.Api/Traceon.Api/src/Traceon.Api/Traceon.Api.csproj \
     src/Traceon.Api/Traceon.Api/src/Traceon.Api/
COPY src/Traceon.Api/Traceon.Api/src/Traceon.Application/Traceon.Application.csproj \
     src/Traceon.Api/Traceon.Api/src/Traceon.Application/
COPY src/Traceon.Api/Traceon.Api/src/Traceon.Contracts/Traceon.Contracts.csproj \
     src/Traceon.Api/Traceon.Api/src/Traceon.Contracts/
COPY src/Traceon.Api/Traceon.Api/src/Traceon.Domain/Traceon.Domain.csproj \
     src/Traceon.Api/Traceon.Api/src/Traceon.Domain/
COPY src/Traceon.Api/Traceon.Api/src/Traceon.Infrastructure/Traceon.Infrastructure.csproj \
     src/Traceon.Api/Traceon.Api/src/Traceon.Infrastructure/

RUN dotnet restore src/Traceon.Api/Traceon.Api/src/Traceon.Api/Traceon.Api.csproj

# Copy all source and publish
COPY src/Traceon.Api/Traceon.Api/src/ src/Traceon.Api/Traceon.Api/src/

RUN dotnet publish src/Traceon.Api/Traceon.Api/src/Traceon.Api/Traceon.Api.csproj \
    -c Release -o /app/publish --no-restore

# ── Runtime stage ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Traceon.Api.dll"]
