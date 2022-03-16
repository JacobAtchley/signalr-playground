using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using web.Models;

namespace web.Services;

public class PersonDomainEventHandler :
    BaseDisposable,
    IEntityAddedDomainEventSubscriber<Person>,
    IEntityUpdatedDomainEventSubscriber<Person>,
    IEntityDeletedDomainEventSubscriber<Person>
{
    private readonly IBroadcastService _broadcastService;

    public PersonDomainEventHandler(IBroadcastService broadcastService)
    {
        _broadcastService = broadcastService;
    }

    public Task EntityAddedAsync(EntityAddedDomainEvent<Person> domainEvent, CancellationToken cancellationToken = new())
    {
        var @event = new PersonalWebSocketEvent
        {
            After = domainEvent.Entity,
            Before = null,
            Date = domainEvent.Time,
            Id = domainEvent.MessageId,
            Trigger = "Added"
        };

        return _broadcastService.BroadcastAsync("personEvent", @event, cancellationToken);
    }

    public Task EntityUpdatedAsync(EntityUpdatedDomainEvent<Person> domainEvent, CancellationToken cancellationToken = new())
    {
        var @event = new PersonalWebSocketEvent
        {
            After = domainEvent.Modified,
            Before = domainEvent.Previous,
            Date = domainEvent.Time,
            Id = domainEvent.MessageId,
            Trigger = "Updated"
        };

        return _broadcastService.BroadcastAsync("personEvent", @event, cancellationToken);
    }

    public Task EntityDeletedAsync(EntityDeletedDomainEvent<Person> domainEvent, CancellationToken cancellationToken = new())
    {
        var @event = new PersonalWebSocketEvent
        {
            After = domainEvent.Entity,
            Before = null,
            Date = domainEvent.Time,
            Id = domainEvent.MessageId,
            Trigger = "Deleted"
        };

        return _broadcastService.BroadcastAsync("personEvent", @event, cancellationToken);
    }
}