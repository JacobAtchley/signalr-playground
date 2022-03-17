using Microsoft.AspNetCore.SignalR;
using web.Models;
using web.Services.Interfaces;

namespace web.Services.Hubs;

public class EntityEventBroadcastService<TEntity> : IEntityEventBroadcastService<TEntity>
    where TEntity : class
{
    private readonly IHubContext<ChatHub> _hub;

    public EntityEventBroadcastService(IHubContext<ChatHub> hub)
    {
        _hub = hub;
    }

    public Task PublishAsync(string? connectionId, string eventName, EntityWebSocketEvent<TEntity> socketEvent, CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(connectionId)
            ? Task.CompletedTask
            : _hub.Clients.Client(connectionId).SendCoreAsync(eventName, new object?[] { socketEvent }, cancellationToken);
    }
}