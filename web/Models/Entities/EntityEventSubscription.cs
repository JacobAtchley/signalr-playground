using Newtonsoft.Json;

namespace web.Models.Entities;

public abstract class EntityEventSubscription
{
    [JsonProperty(Order = 1)]
    public string? ConnectionId { get; set; }

    [JsonProperty(Order = 2)]
    public Guid Id { get; init; }

    [JsonProperty(Order = 3)]
    public EntityTrigger Trigger { get; set; }

    [JsonProperty(Order = 4)]
    public string? Filter { get; set; }

    [JsonProperty(Order = 5)]
    public DateTimeOffset SubscriptionDate { get; set; }
}

public class PersonEventSubscription : EntityEventSubscription
{

}