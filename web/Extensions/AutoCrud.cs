using Firebend.AutoCrud.ChangeTracking.Mongo;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.CustomFields.Mongo;
using Firebend.AutoCrud.DomainEvents.MassTransit.Extensions;
using Firebend.AutoCrud.Mongo;
using web.Models;
using web.Models.Entities;
using web.Services;

namespace web.Extensions;

public static class AutoCrud
{
    public static IServiceCollection AddAutoCrud(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        return serviceCollection.UsingMongoCrud(configuration.GetConnectionString("Mongo"), true, mongo =>
        {
            mongo.AddEntity<Guid, Person>(person =>
                person.WithDefaultDatabase("SignalRPlayground")
                    .WithCollection("People")
                    .WithFullTextSearch()
                    .AddCustomFields()
                    .AddCrud()
                    .AddDomainEvents(de =>
                        de.WithMongoChangeTracking()
                            .WithMassTransit()
                            .AddWebSocketHandlers<Guid, Person, PersonWebSockDomainEventHandler, PersonEventSubscription, PersonEventBroadcastFilterService>()));
        });
    }
}