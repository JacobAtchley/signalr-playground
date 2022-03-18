using web.Models;
using web.Models.Entities;

namespace web.Services.Interfaces;

public interface IEntityEventSessionStore<TEntity>
    where TEntity : EntityEventSubscription
{
    Task RegisterAsync(TEntity subscription, CancellationToken cancellationToken);

    Task<List<TEntity>?> GetConnectionsAsync(EntityTrigger trigger, CancellationToken cancellationToken);

    Task RemoveConnectionsOlderThanAsync(DateTimeOffset cutoff, CancellationToken cancellationToken);

    Task RemoveConnectionAsync(string connectionId, CancellationToken cancellationToken);
}