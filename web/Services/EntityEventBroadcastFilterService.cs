using web.Models;
using web.Models.Entities;
using web.Services.Interfaces;

namespace web.Services;

public class EntityEventBroadcastFilterService<TEntity, TSubscription> : IEntityEventBroadcastFilterService<TEntity, TSubscription>
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
        return connections?.Select(x => x.ConnectionId).ToList();
    }
}