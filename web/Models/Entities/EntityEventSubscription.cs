namespace web.Models.Entities;

public abstract class EntityEventSubscription
{
    public Guid Id { get; init; }
    public string? ConnectionId { get; set; }
    public EntityTrigger Trigger { get; set; }
    public string? Filter { get; set; }

    public DateTimeOffset SubscriptionDate { get; set; }
}

public class PersonEventSubscription : EntityEventSubscription
{

}