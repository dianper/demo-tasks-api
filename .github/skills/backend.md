# Skill: Backend — .NET 10 Minimal API Patterns

Reference this file when implementing any backend feature.

---

## Endpoint Pattern

Every resource has one static class in `src/TasksApi/Endpoints/`:

```csharp
// src/TasksApi/Endpoints/TaskEndpoints.cs
public static class TaskEndpoints
{
    public static WebApplication MapTaskEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/tasks")
            .RequireAuthorization()
            .WithTags("Tasks");

        group.MapGet("/", ListTasksAsync);
        group.MapGet("/{id:guid}", GetTaskByIdAsync);
        group.MapPost("/", CreateTaskAsync);
        group.MapPatch("/{id:guid}/complete", CompleteTaskAsync);
        group.MapDelete("/{id:guid}", DeleteTaskAsync);

        return app;
    }

    private static async Task<IResult> GetTaskByIdAsync(
        Guid id,
        GetTaskByIdHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetTaskByIdQuery(id), ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error);
    }

    private static async Task<IResult> CreateTaskAsync(
        CreateTaskRequest request,
        CreateTaskHandler handler,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await handler.HandleAsync(new CreateTaskCommand(request, userId), ct);

        return result.IsSuccess
            ? Results.Created($"/tasks/{result.Value.Id}", result.Value)
            : Results.BadRequest(result.Error);
    }
}
```

Register in `Program.cs`:
```csharp
app.MapTaskEndpoints();
```

---

## Feature Handler Pattern

```csharp
// src/TasksApi/Features/Tasks/Commands/CreateTaskHandler.cs
public sealed class CreateTaskHandler(AppDbContext db)
{
    public async Task<Result<TaskResponse>> HandleAsync(
        CreateTaskCommand command,
        CancellationToken ct = default)
    {
        // 1. Validate business rules
        if (string.IsNullOrWhiteSpace(command.Request.Title))
            return Result.Failure<TaskResponse>(TaskErrors.TitleRequired);

        // 2. Create domain entity
        var task = TaskItem.Create(
            title: command.Request.Title,
            description: command.Request.Description,
            createdByUserId: command.UserId);

        // 3. Persist
        db.Tasks.Add(task);
        await db.SaveChangesAsync(ct);

        // 4. Return mapped response
        return Result.Success(TaskResponse.FromEntity(task));
    }
}
```

---

## Result<T> Pattern

```csharp
// src/TasksApi/Common/Result.cs
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }

    private Result(T value) { IsSuccess = true; Value = value; }
    private Result(Error error) { IsSuccess = false; Error = error; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);
}

public static class Result
{
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
}

// src/TasksApi/Common/Error.cs
public sealed record Error(string Code, string Message)
{
    public static Error NotFound(string code, string message) => new(code, message);
    public static Error Validation(string code, string message) => new(code, message);
    public static Error Unauthorized() => new("Auth.Unauthorized", "Unauthorized");
}
```

---

## Domain Entity Pattern

```csharp
// src/TasksApi/Domain/TaskItem.cs
public sealed class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    // EF Core constructor
    private TaskItem() { }

    public static TaskItem Create(string title, string? description, Guid createdByUserId)
    {
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Status = TaskStatus.Todo,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public Result Complete()
    {
        if (Status == TaskStatus.Completed)
            return Result.Failure<bool>(TaskErrors.AlreadyCompleted);

        Status = TaskStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        return Result.Success(true);
    }
}

public enum TaskStatus { Todo, InProgress, Completed }
```

---

## EF Core Configuration Pattern

```csharp
// Inside AppDbContext.OnModelCreating:
builder.Entity<TaskItem>(e =>
{
    e.HasKey(t => t.Id);
    e.Property(t => t.Title).IsRequired().HasMaxLength(200);
    e.Property(t => t.Status).HasConversion<string>();
    e.Property(t => t.CreatedAt).IsRequired();
    e.HasIndex(t => t.CreatedByUserId);
});
```

---

## Error Catalogue Pattern

Group errors per feature:

```csharp
// src/TasksApi/Features/Tasks/TaskErrors.cs
public static class TaskErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Task.NotFound", "Task not found");

    public static readonly Error AlreadyCompleted =
        Error.Validation("Task.AlreadyCompleted", "Task is already completed");

    public static readonly Error TitleRequired =
        Error.Validation("Task.TitleRequired", "Title is required");
}
```

---

## Dependency Injection

Register handlers in a feature extension method:

```csharp
// src/TasksApi/Features/Tasks/TasksServiceExtensions.cs
public static class TasksServiceExtensions
{
    public static IServiceCollection AddTasksFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateTaskHandler>();
        services.AddScoped<GetTaskByIdHandler>();
        services.AddScoped<ListTasksHandler>();
        services.AddScoped<CompleteTaskHandler>();
        services.AddScoped<DeleteTaskHandler>();
        return services;
    }
}
```

Register in `Program.cs`:
```csharp
builder.Services.AddTasksFeature();
```