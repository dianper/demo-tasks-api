using FluentAssertions;
using TasksApi.Endpoints;

namespace TasksApi.UnitTests.Endpoints;

public class SystemEndpointsTests
{
    [Fact]
    public void BuildHealthResponse_WithConfiguredVersion_ReturnsHealthyMetadataWithVersion()
    {
        HealthResponse response = SystemEndpoints.BuildHealthResponse("1.2.3");

        response.Service.Should().Be("Tasks API");
        response.Status.Should().Be("Healthy");
        response.TimestampUtc.Kind.Should().Be(DateTimeKind.Utc);
        response.Version.Should().Be("1.2.3");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildHealthResponse_WhenVersionMissing_UsesDefaultVersion(string? version)
    {
        HealthResponse response = SystemEndpoints.BuildHealthResponse(version);

        response.Version.Should().Be("0.1.0");
    }
}
