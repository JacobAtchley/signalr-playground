using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Interfaces.Models;
using web.Models.Entities;
using web.Services;
using web.Services.Db;
using web.Services.Hubs;
using web.Services.Interfaces;

namespace web.Extensions;

public static class DomainEventExtensions
{
    public static DomainEventsConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity> AddWebSocketHandlers<TKey, TEntity, THandler, TSubscription, TFilter>(
        this DomainEventsConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity> domainEventsConfigurator,
        bool useRedis)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where THandler : EntityWebSockDomainEventHandler<TEntity, TSubscription>
        where TSubscription : EntityEventSubscription
    {
        if (useRedis)
        {
            domainEventsConfigurator.Builder
                .WithRegistration<IEntityEventSessionStore<TSubscription>, EntityEventSessionStoreRedis<TSubscription>>();
        }
        else
        {
            domainEventsConfigurator.Builder
                .WithRegistration<IEntityEventSessionStore<TSubscription>, EntityEventSessionStore<TSubscription>>();
        }

        domainEventsConfigurator.Builder
            .WithRegistration<IEntityEventBroadcastService<TEntity>, EntityEventBroadcastService<TEntity>>();

        domainEventsConfigurator.Builder
            .WithRegistration<IEntityEventBroadcastFilterService<TEntity, TSubscription>, TFilter>();

        return domainEventsConfigurator.WithDomainEventEntityAddedSubscriber<THandler>()
            .WithDomainEventEntityUpdatedSubscriber<THandler>()
            .WithDomainEventEntityDeletedSubscriber<THandler>();
    }
}