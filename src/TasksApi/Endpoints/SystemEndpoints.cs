namespace TasksApi.Endpoints;

public static class SystemEndpoints
{
    private const string DefaultVersion = "0.1.0";

    public static WebApplication MapSystemEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () => TypedResults.Ok(BuildHealthResponse(app.Configuration)))
            .AllowAnonymous()
            .WithName("GetHealth");

        return app;
    }

    public static HealthResponse BuildHealthResponse(IConfiguration configuration)
    {
        string? configuredVersion = configuration["App:Version"];
        string version = string.IsNullOrWhiteSpace(configuredVersion) ? DefaultVersion : configuredVersion;

        return new HealthResponse(
            Service: "Tasks API",
            Status: "Healthy",
            TimestampUtc: DateTime.UtcNow,
            Version: version);
    }
}

public sealed record HealthResponse(string Service, string Status, DateTime TimestampUtc, string Version);
