namespace Queuesto;

public class MessageWrapper<T>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Time { get; set; } = DateTime.UtcNow;
    public string Schema { get; set; }
    public string Source { get; set; }
    public string ContentType { get; set; }
    public string CommandType { get; set; }
    public T Message { get; set; }
}