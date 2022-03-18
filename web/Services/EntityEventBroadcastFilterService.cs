using web.Models;
using web.Models.Entities;
using web.Services.Interfaces;

namespace web.Services;

public abstract class EntityEventBroadcastFilterService<TEntity, TSubscription> : IEntityEventBroadcastFilterService<TEntity, TSubscription>
    where TSubscription : EntityEventSubscription
{
    private readonly IEntityEventSessionStore<TSubscription> _store;

    public EntityEventBroadcastFilterService(IEntityEventSessionStore<TSubscription> store)
    {
        _store = store;
    }

    public async Task<List<string?>?> GetConnectionIdsAsync(EntityWebSocketEvent<TEntity> entityWebSocketEvent, CancellationToken cancellationToken)
    {
        var connections = await _store.GetConnectionsAsync(entityWebSocketEvent.Trigger, cancellationToken);
        var filtered = connections?.Where(x => Filter(entityWebSocketEvent, x)).Select(x => x.ConnectionId).ToList();
        return filtered;
    }

    protected abstract bool Filter(EntityWebSocketEvent<TEntity> entityWebSocketEvent, TSubscription entityEventSubscription);
}

public class PersonEventBroadcastFilterService : EntityEventBroadcastFilterService<Person, PersonEventSubscription>
{
    public PersonEventBroadcastFilterService(IEntityEventSessionStore<PersonEventSubscription> store) : base(store)
    {
    }

    protected override bool Filter(EntityWebSocketEvent<Person> entityWebSocketEvent, PersonEventSubscription entityEventSubscription)
    {
        if (string.IsNullOrWhiteSpace(entityEventSubscription.Filter))
        {
            return true;
        }

        if (!entityEventSubscription.Filter.EndsWith("firstName")) return false;

        switch (entityEventSubscription.Trigger)
        {
            case EntityTrigger.Added:
                return true;
            case EntityTrigger.Updated:
                return entityWebSocketEvent.Before?.FirstName != entityWebSocketEvent.After?.FirstName;
            case EntityTrigger.Unknown:
                break;
            case EntityTrigger.Deleted:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }
}