using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using web.Models;
using web.Services.Interfaces;

namespace web.Services;

public abstract class EntityWebSockDomainEventHandler<TEntity, TSubscription> :
    BaseDisposable,
    IEntityAddedDomainEventSubscriber<TEntity>,
    IEntityUpdatedDomainEventSubscriber<TEntity>,
    IEntityDeletedDomainEventSubscriber<TEntity>
    where TEntity : class
{
    private readonly IEntityEventBroadcastService<TEntity> _broadcastService;
    private readonly IEntityEventBroadcastFilterService<TEntity, TSubscription> _filterService;

    protected abstract string EventName { get; }

    protected EntityWebSockDomainEventHandler(IEntityEventBroadcastService<TEntity> broadcastService, IEntityEventBroadcastFilterService<TEntity, TSubscription> filterService)
    {
        _broadcastService = broadcastService;
        _filterService = filterService;
    }

    public Task EntityAddedAsync(EntityAddedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = new())
    {
        var @event = new EntityWebSocketEvent<TEntity>
        {
            After = domainEvent.Entity,
            Before = null,
            Date = domainEvent.Time,
            Id = domainEvent.MessageId,
            Trigger = "Added"
        };

        return SendMessagesAsync(@event, cancellationToken);
    }

    public Task EntityUpdatedAsync(EntityUpdatedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = new())
    {
        var @event = new EntityWebSocketEvent<TEntity>
        {
            After = domainEvent.Modified,
            Before = domainEvent.Previous,
            Date = domainEvent.Time,
            Id = domainEvent.MessageId,
            Trigger = "Updated"
        };

        return SendMessagesAsync(@event, cancellationToken);
    }

    public Task EntityDeletedAsync(EntityDeletedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = new())
    {
        var @event = new EntityWebSocketEvent<TEntity>
        {
            After = domainEvent.Entity,
            Before = null,
            Date = domainEvent.Time,
            Id = domainEvent.MessageId,
            Trigger = "Deleted"
        };

        return SendMessagesAsync(@event, cancellationToken);
    }

    private async Task SendMessagesAsync(EntityWebSocketEvent<TEntity> entityWebSocketEvent, CancellationToken cancellationToken)
    {
        var connectionIds = await _filterService.GetConnectionIdsAsync(entityWebSocketEvent, cancellationToken);

        if (connectionIds is null)
        {
            return;
        }

        const int chunkSize = 100;

        foreach (var chunk in connectionIds.Chunk(chunkSize))
        {
            var tasks = new List<Task>(chunkSize);
            tasks.AddRange(chunk.Select(connectionId => _broadcastService.PublishAsync(connectionId, EventName, entityWebSocketEvent, cancellationToken)));
            await Task.WhenAll(tasks);
        }
    }
}