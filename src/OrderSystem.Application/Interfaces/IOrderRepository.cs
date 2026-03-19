using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Interfaces;

public interface IOrderRepository
{
    ValueTask AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask UpdateAsync(Order order, CancellationToken cancellationToken = default);
}
