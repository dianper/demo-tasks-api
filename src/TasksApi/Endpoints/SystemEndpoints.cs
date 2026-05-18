namespace TasksApi.Endpoints;

public static class SystemEndpoints
{
    public static WebApplication MapSystemEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () => TypedResults.Ok(BuildHealthResponse()))
            .AllowAnonymous()
            .WithName("GetHealth");

        return app;
    }

    public static HealthResponse BuildHealthResponse()
    {
        return new HealthResponse(
            Service: "Tasks API",
            Status: "Healthy",
            TimestampUtc: DateTime.UtcNow);
    }
}

public sealed record HealthResponse(string Service, string Status, DateTime TimestampUtc);