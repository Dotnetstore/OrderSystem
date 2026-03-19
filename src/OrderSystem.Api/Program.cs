using Microsoft.EntityFrameworkCore;
using OrderSystem.Application.Exceptions;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OrderSystem.Api.Requests;
using OrderSystem.Application.Services;
using OrderSystem.Domain.Exceptions;
using OrderSystem.Infrastructure.Extensions;
using OrderSystem.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/api.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSqlClientInstrumentation()
            .AddSource("OrderSystem")
            .AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddRuntimeInstrumentation();
        metrics.AddConsoleExporter();
    });


builder.Host.UseSerilog();

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddScoped<OrderService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception exception) when (TryMapException(exception, out var statusCode, out var title))
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        await Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: exception.Message)
            .ExecuteAsync(context);
    }
});

app.MapGet("/", () => Results.Ok(new
{
    Name = "OrderSystem API",
    Status = "Running"
}));

app.MapGet("/favicon.ico", () => Results.NoContent());

app.MapHealthChecks("/health");

app.MapGet("/orders", async (ApplicationDbContext db) =>
{
    var orders = await db.Orders
        .Select(o => new {
            o.Id,
            o.Customer,
            o.Amount,
            o.Status,
            o.CreatedAt
        })
        .ToListAsync();

    return Results.Ok(orders);
});

app.MapGet("/orders/{id:guid}/audit", async (Guid id, ApplicationDbContext db) =>
{
    var order = await db.Orders
        .Include(o => o.AuditLog)
        .FirstOrDefaultAsync(o => o.Id == id);

    if (order is null)
        return Results.NotFound();

    return Results.Ok(order.AuditLog
        .Select(a => new { a.Timestamp, a.Message }));
});

app.MapPost("/orders", async (CreateOrderRequest req, OrderService service, CancellationToken ct) =>
{
    var id = await service.CreateOrderAsync(req.Customer, req.Amount, ct);
    return Results.Created($"/orders/{id}", new { Id = id });
});

app.MapPost("/orders/{id:guid}/start-processing", async (Guid id, OrderService service, CancellationToken ct) =>
{
    await service.StartProcessingAsync(id, ct);
    return Results.Ok(new { Message = "Order processing started" });
});

app.MapPost("/orders/{id:guid}/complete", async (Guid id, OrderService service, CancellationToken ct) =>
{
    await service.CompleteAsync(id, ct);
    return Results.Ok(new { Message = "Order completed" });
});

app.MapPost("/orders/{id:guid}/cancel", async (Guid id, CancelOrderRequest req, OrderService service, CancellationToken ct) =>
{
    await service.CancelAsync(id, req.Reason, ct);
    return Results.Ok(new { Message = "Order cancelled" });
});

app.Run();

static bool TryMapException(Exception exception, out int statusCode, out string title)
{
    switch (exception)
    {
        case OrderNotFoundException:
            statusCode = StatusCodes.Status404NotFound;
            title = "Order not found";
            return true;
        case InvalidOrderStateException:
            statusCode = StatusCodes.Status409Conflict;
            title = "Invalid order state";
            return true;
        default:
            statusCode = StatusCodes.Status500InternalServerError;
            title = string.Empty;
            return false;
    }
}

public partial class Program;
