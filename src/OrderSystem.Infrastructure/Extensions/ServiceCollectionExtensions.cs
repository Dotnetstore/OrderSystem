using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderSystem.Application.Interfaces;
using OrderSystem.Infrastructure.Messaging;
using OrderSystem.Infrastructure.Persistence;
using RabbitMQ.Client;

namespace OrderSystem.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection") 
                              ?? "Data Source=orders.db"));

        services
            .AddScoped<IOrderRepository, OrderRepository>()
            .AddScoped<IEventDispatcher, EventDispatcher>()
            .AddScoped<IMessagePublisher, RabbitMqPublisher>()
            .AddSingleton<IConnection>(_ =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = configuration["RabbitMq:Host"] ?? "localhost"
                };
                return factory.CreateConnection();
            });

        return services;
    }
}
