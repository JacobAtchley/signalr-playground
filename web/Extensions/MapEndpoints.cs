using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.SignalR.Management;
using web.Data;
using web.Models.Entities;
using web.Models.ViewModels;
using web.Services.Interfaces;
using web.Services.Jobs;
using web.Services.Serverless;

namespace web.Extensions;

public static class MapEndpoints
{
    public static IEndpointRouteBuilder MapMyEndpoints(this IEndpointRouteBuilder app, bool useServerlessAzureSignalR)
    {
        app.MapGet("/api/username", Users.GenerateUserName);
        app.MapGet("/api/entity-pump",async (PersonEntityPump pump) => { await pump.Invoke(); return "Pumped!"; });

        app.MapPost("/api/connections/{connectionId}/person-events", async (
            [FromRoute] string connectionId,
            [FromBody] EventSubscriptionViewModel sub,
            IEntityEventSessionStore<PersonEventSubscription> store,
            CancellationToken cancellationToken) =>
        {
            var personSub = new PersonEventSubscription
            {
                Filter = sub.Filter,
                ConnectionId = connectionId,
                Trigger = sub.Trigger.GetValueOrDefault(),
                Id = Guid.NewGuid(),
                SubscriptionDate = DateTimeOffset.UtcNow
            };

            await store.RegisterAsync(personSub, cancellationToken);
            return new { Text = "Subscribed" };
        });

        if (useServerlessAzureSignalR)
        {
            app.MapPost("/chat/negotiate", async (string userName, ServerlessSignalRService service, CancellationToken cancellationToken) =>
            {
                var negotiateResponse = await service.ChatHubContext!.NegotiateAsync(new NegotiationOptions { UserId = userName }, cancellationToken);

                return new Dictionary<string, string?>
                {
                    { "url", negotiateResponse.Url },
                    { "accessToken", negotiateResponse.AccessToken }
                };
            });
        }

        return app;
    }
}