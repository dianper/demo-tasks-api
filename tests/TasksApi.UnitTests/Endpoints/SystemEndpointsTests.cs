using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TasksApi.Endpoints;

namespace TasksApi.UnitTests.Endpoints;

public class SystemEndpointsTests
{
    [Fact]
    public void BuildHealthResponse_WhenVersionConfigured_ReturnsHealthyMetadataWithConfiguredVersion()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["App:Version"] = "2.3.4"
            })
            .Build();

        HealthResponse response = SystemEndpoints.BuildHealthResponse(configuration);

        response.Service.Should().Be("Tasks API");
        response.Status.Should().Be("Healthy");
        response.TimestampUtc.Kind.Should().Be(DateTimeKind.Utc);
        response.Version.Should().Be("2.3.4");
    }

    [Fact]
    public void BuildHealthResponse_WhenVersionNotConfigured_FallsBackToDefaultVersion()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        HealthResponse response = SystemEndpoints.BuildHealthResponse(configuration);

        response.Version.Should().Be("0.1.0");
    }
}
