namespace TasksApi.Endpoints;

public static class SystemEndpoints
{
    public static WebApplication MapSystemEndpoints(this WebApplication app)
    {
        string configuredVersion = app.Configuration["App:Version"] ?? "0.1.0";

        app.MapGet("/health", () => TypedResults.Ok(BuildHealthResponse(configuredVersion)))
            .AllowAnonymous()
            .WithName("GetHealth");

        return app;
    }

    public static HealthResponse BuildHealthResponse(string? version = null)
    {
        string resolvedVersion = string.IsNullOrWhiteSpace(version) ? "0.1.0" : version;

        return new HealthResponse(
            Service: "Tasks API",
            Status: "Healthy",
            TimestampUtc: DateTime.UtcNow,
            Version: resolvedVersion);
    }
}

public sealed record HealthResponse(string Service, string Status, DateTime TimestampUtc, string Version);