using web.Models;
using web.Models.Entities;
using web.Services.Interfaces;

namespace web.Services;

public class PersonWebSockDomainEventHandler : EntityWebSockDomainEventHandler<Person, PersonEventSubscription>
{
    public PersonWebSockDomainEventHandler(IEntityEventBroadcastService<Person> broadcastService,
        IEntityEventBroadcastFilterService<Person, PersonEventSubscription> filterService) : base(broadcastService, filterService)
    {
    }

    protected override string EventName => "personEvent";
}