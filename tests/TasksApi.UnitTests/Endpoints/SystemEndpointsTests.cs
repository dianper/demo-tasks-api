using FluentAssertions;
using TasksApi.Endpoints;

namespace TasksApi.UnitTests.Endpoints;

public class SystemEndpointsTests
{
    [Fact]
    public void BuildHealthResponse_DefaultScenario_ReturnsHealthyMetadata()
    {
        HealthResponse response = SystemEndpoints.BuildHealthResponse();

        response.Service.Should().Be("Tasks API");
        response.Status.Should().Be("Healthy");
        response.TimestampUtc.Kind.Should().Be(DateTimeKind.Utc);
    }
}