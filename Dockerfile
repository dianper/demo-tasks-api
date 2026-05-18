# syntax=docker/dockerfile:1

# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ARG BUILD_VERSION=0.0.0

# Restore (cached layer — only re-runs if .csproj changes)
COPY src/TasksApi/TasksApi.csproj src/TasksApi/
RUN dotnet restore src/TasksApi/TasksApi.csproj

# Copy everything and build
COPY src/ src/
RUN dotnet publish src/TasksApi/TasksApi.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:Version=${BUILD_VERSION}

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN groupadd --system appgroup && useradd --system -g appgroup appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TasksApi.dll"]