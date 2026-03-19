using Microsoft.EntityFrameworkCore;
using OrderSystem.Application.Interfaces;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Infrastructure.Persistence;

public class OrderRepository(ApplicationDbContext db) : IOrderRepository
{
    public async ValueTask AddAsync(Order order, CancellationToken cancellationToken)
    {
        await db.Orders.AddAsync(order, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken)
        => db.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    
    public async ValueTask UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        db.Orders.Update(order);
        await db.SaveChangesAsync(cancellationToken);
    }
}
