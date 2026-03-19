using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderSystem.Application.Interfaces;
using OrderSystem.Infrastructure.Persistence;
using RabbitMQ.Client;

namespace OrderSystem.Tests;

public sealed class ApiTestFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public ApiTestFactory()
    {
        _connection.Open();
    }

    public PublishedMessageCollector PublishedMessages { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IMessagePublisher>();
            services.RemoveAll<IConnection>();

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
            services.AddScoped<IMessagePublisher>(_ => PublishedMessages);
        });
    }

    public async Task ResetAsync()
    {
        PublishedMessages.Clear();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task<OrderSnapshot?> GetOrderAsync(Guid orderId)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.Orders
            .AsNoTracking()
            .Where(order => order.Id == orderId)
            .Select(order => new OrderSnapshot(
                order.Id,
                order.Customer,
                order.Amount,
                order.Status,
                order.AuditLog.Count))
            .SingleOrDefaultAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}

public sealed record OrderSnapshot(
    Guid Id,
    string Customer,
    decimal Amount,
    OrderSystem.Domain.Entities.OrderStatus Status,
    int AuditEntryCount);
