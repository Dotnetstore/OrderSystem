using OrderSystem.Application.Exceptions;
using OrderSystem.Application.Interfaces;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Services;

public sealed class OrderService(IOrderRepository repository)
{
    public async ValueTask<Guid> CreateOrderAsync(string customer, decimal amount, CancellationToken ct = default)
    {
        var order = new Order(customer, amount);

        await repository.AddAsync(order, ct);

        return order.Id;
    }

    public async ValueTask StartProcessingAsync(Guid id, CancellationToken ct = default)
    {
        var order = await repository.GetAsync(id, ct)
                    ?? throw new OrderNotFoundException(id);

        order.StartProcessing();

        await repository.UpdateAsync(order, ct);
    }

    public async ValueTask CompleteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await repository.GetAsync(id, ct)
                    ?? throw new OrderNotFoundException(id);

        order.Complete();

        await repository.UpdateAsync(order, ct);
    }

    public async ValueTask CancelAsync(Guid id, string reason, CancellationToken ct = default)
    {
        var order = await repository.GetAsync(id, ct)
                    ?? throw new OrderNotFoundException(id);

        order.Cancel(reason);

        await repository.UpdateAsync(order, ct);
    }
}