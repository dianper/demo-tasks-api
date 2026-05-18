using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TasksApi.Endpoints;
using TasksApi.IntegrationTests.Infrastructure;

namespace TasksApi.IntegrationTests.Endpoints;

public class SystemEndpointsTests(TasksApiFactory factory) : IClassFixture<TasksApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Get_Health_DefaultScenario_ReturnsHealthyResponse()
    {
        HttpResponseMessage response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        HealthResponse? body = await response.Content.ReadFromJsonAsync<HealthResponse>();

        body.Should().NotBeNull();
        body!.Service.Should().Be("Tasks API");
        body.Status.Should().Be("Healthy");
        body.Version.Should().Be("0.1.0");
    }
}
