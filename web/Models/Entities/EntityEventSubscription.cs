namespace web.Models.Entities;

public abstract class EntityEventSubscription
{
    public Guid Id { get; init; }
    public string? ConnectionId { get; set; }
    public string? Trigger { get; set; }
    public string? Filter { get; set; }

    public UserSession? Session { get; set; }
}

public class PersonEventSubscription : EntityEventSubscription
{

}