namespace web.Models;

public class EntityWebSocketEvent<TEntity>
{
    public Guid Id { get; set; }
    public TEntity? Before { get; set; }
    public TEntity? After { get; set; }
    public string? Trigger { get; set; }
    public DateTimeOffset Date { get; set; }
}