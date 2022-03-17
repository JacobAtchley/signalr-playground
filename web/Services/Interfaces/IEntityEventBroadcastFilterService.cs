using web.Models;

namespace web.Services.Interfaces;

public interface IEntityEventBroadcastFilterService<TEntity, TSubscription>
{
    Task<List<string?>?> GetConnectionIdsAsync(EntityWebSocketEvent<TEntity> entityWebSocketEvent, CancellationToken cancellationToken);
}