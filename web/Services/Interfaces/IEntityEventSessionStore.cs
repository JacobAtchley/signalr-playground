using web.Models.Entities;

namespace web.Services.Interfaces;

public interface IEntityEventSessionStore<TEntity>
    where TEntity : EntityEventSubscription
{
    Task RegisterAsync(TEntity subscription, CancellationToken cancellationToken);

    Task<List<TEntity>?> GetConnectionsAsync(string? trigger, CancellationToken cancellationToken);
}