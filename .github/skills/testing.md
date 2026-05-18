# Skill: Testing — xUnit + FluentAssertions + Testcontainers

Reference this file when writing any test.

---

## Test Projects

| Project | Purpose |
|---|---|
| `tests/TasksApi.UnitTests` | Handlers, domain logic — no I/O, no DB |
| `tests/TasksApi.IntegrationTests` | Full HTTP stack with real PostgreSQL via Testcontainers |

---

## Naming Convention

```
MethodOrFeature_Scenario_ExpectedResult
```

Examples:
```csharp
HandleAsync_ValidCommand_ReturnsCreatedTask()
HandleAsync_EmptyTitle_ReturnsValidationError()
POST_Tasks_ReturnsCreated_WhenValid()
POST_Tasks_Returns401_WhenNoToken()
GET_Tasks_Id_Returns404_WhenNotFound()
```

---

## Unit Test Pattern

Test handlers in isolation. Use an in-memory mock for `AppDbContext`.

```csharp
// tests/TasksApi.UnitTests/Features/Tasks/CreateTaskHandlerTests.cs
public class CreateTaskHandlerTests
{
    private readonly AppDbContext _db;
    private readonly CreateTaskHandler _handler;

    public CreateTaskHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _handler = new CreateTaskHandler(_db);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsCreatedTask()
    {
        // Arrange
        var command = new CreateTaskCommand(
            Request: new CreateTaskRequest("Buy milk", null),
            UserId: Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Buy milk");
        _db.Tasks.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleAsync_EmptyTitle_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateTaskCommand(
            Request: new CreateTaskRequest("", null),
            UserId: Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("Task.TitleRequired");
    }
}
```

---

## Integration Test Pattern

Use `TasksApiFactory` (WebApplicationFactory + Testcontainers). Spins up a real PostgreSQL container.

```csharp
// tests/TasksApi.IntegrationTests/Infrastructure/TasksApiFactory.cs
public class TasksApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings__DefaultConnection"] = _postgres.GetConnectionString()
            });
        });

        builder.ConfigureServices(services =>
        {
            // Run migrations on startup
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        });
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
```

```csharp
// tests/TasksApi.IntegrationTests/Features/Tasks/TaskEndpointsTests.cs
public class TaskEndpointsTests(TasksApiFactory factory)
    : IClassFixture<TasksApiFactory>
{
    private readonly HttpClient _client = factory.CreateAuthenticatedClient(); // helper

    [Fact]
    public async Task POST_Tasks_ReturnsCreated_WhenValid()
    {
        // Arrange
        var request = new { title = "Integration task", description = "test" };

        // Act
        var response = await _client.PostAsJsonAsync("/tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<TaskResponse>();
        body!.Title.Should().Be("Integration task");
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task POST_Tasks_Returns401_WhenNoToken()
    {
        // Arrange — unauthenticated client
        var unauthClient = factory.CreateClient();
        var request = new { title = "Should fail" };

        // Act
        var response = await unauthClient.PostAsJsonAsync("/tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_Tasks_Id_Returns404_WhenNotFound()
    {
        var response = await _client.GetAsync($"/tasks/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

---

## Auth Helper

Add this to `TasksApiFactory` for convenience:

```csharp
public HttpClient CreateAuthenticatedClient(string role = "User")
{
    var client = CreateClient();
    var token = JwtTestHelper.GenerateToken(userId: Guid.NewGuid(), role: role);
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
    return client;
}
```

---

## Rules

- **Never** share database state between tests — each test must be independent
- **Always** use `IClassFixture<TasksApiFactory>` (not `IAsyncLifetime` on the test class)
- **Unit tests** must complete in < 100ms each — no sleep, no real I/O
- **Integration tests** reuse the same Testcontainer per class (fixture) for speed
- **Minimum coverage per new endpoint:** happy path + 401 + 404/400
- **FluentAssertions** for all assertions — never `Assert.Equal` / `Assert.True`