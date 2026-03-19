namespace OrderSystem.Domain.Entities;

public sealed class AuditEntry
{
    public string Message { get; private set; } =  string.Empty;
    public DateTime Timestamp { get; private set; }

    private AuditEntry() { }

    public AuditEntry(string message)
    {
        Message = message;
        Timestamp = DateTime.UtcNow;
    }
}
