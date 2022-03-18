namespace web.Models;

public class EntityWebSocketEvent<TEntity>
{
    public Guid Id { get; set; }
    public TEntity? Before { get; set; }
    public TEntity? After { get; set; }
    public EntityTrigger Trigger { get; set; }
    public DateTimeOffset Date { get; set; }
}