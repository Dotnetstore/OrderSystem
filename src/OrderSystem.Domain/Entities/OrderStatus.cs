namespace OrderSystem.Domain.Entities;

public enum OrderStatus
{
    Created = 0,
    Processing = 1,
    Completed = 2,
    Cancelled = 3
}