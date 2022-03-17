using web.Models;

namespace web.Services.Interfaces;

public interface IEntityEventBroadcastService<TEntity>
    where TEntity : class
{
    Task PublishAsync(string? connectionId,
        string eventName,
        EntityWebSocketEvent<TEntity> socketEvent,
        CancellationToken cancellationToken);
}