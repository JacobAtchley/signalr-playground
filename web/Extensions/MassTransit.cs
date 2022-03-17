using System.Text.RegularExpressions;
using Firebend.AutoCrud.DomainEvents.MassTransit.Extensions;
using MassTransit;

namespace web.Extensions;

public static class MassTransit
{
    private static readonly Regex ConStringParser = new(
        "^rabbitmq://([^:]+):(.+)@([^@]+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static IServiceCollection AddRabbitMqMassTransit(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var connString = configuration.GetConnectionString("RabbitMQ");

        if (string.IsNullOrWhiteSpace(connString))
        {
            throw new Exception("Please configure a service bus connection string for Rabbit MQ");
        }

        return serviceCollection.AddMassTransit(bus =>
            {
                bus.RegisterFirebendAutoCrudDomainEventHandlers(serviceCollection);

                bus.UsingRabbitMq((context, configurator) =>
                {
                    var match = ConStringParser.Match(connString);

                    var domain = match.Groups[3].Value;
                    var uri = $"rabbitmq://{domain}";

                    configurator.Host(new Uri(uri), h =>
                    {
                        h.PublisherConfirmation = true;
                        h.Username(match.Groups[1].Value);
                        h.Password(match.Groups[2].Value);
                    });

                    configurator.Lazy = true;
                    configurator.AutoDelete = true;
                    configurator.PurgeOnStartup = true;

                    context.RegisterFirebendAutoCrudDomainEventHandlerEndPoints(configurator, serviceCollection, AutoCrudMassTransitQueueMode.OneQueue);
                });
            })
            .AddMassTransitHostedService();
    }
}