using System.Linq.Expressions;
using Microsoft.AspNetCore.SignalR;
using web.Models.Entities;
using web.Services.Interfaces;

namespace web.Services.Hubs;

public class HubContextBroadcastService : IBroadcastService
{
    private readonly IUserSessionStore _userSessionStore;
    private readonly IHubContext<ChatHub> _hubContext;

    public HubContextBroadcastService(IUserSessionStore userSessionStore, IHubContext<ChatHub> hubContext)
    {
        _userSessionStore = userSessionStore;
        _hubContext = hubContext;
    }

    public async Task BroadcastAsync<TPayload>(string eventName, TPayload? payload, CancellationToken cancellationToken)
    {
        var users = await _userSessionStore.GetUserSessionsAsync(cancellationToken);

        await SendMessageToClientsAsync(users, eventName, payload, cancellationToken);
    }

    private async Task SendMessageToClientsAsync<TPayload>( IEnumerable<UserSession> users, string eventName, TPayload? payload, CancellationToken cancellationToken)
    {
        if (payload is not null)
        {
            await _hubContext.Clients
                .Clients(users.Select(x => x.ConnectionId))
                .SendCoreAsync(eventName, new object[] { payload }, cancellationToken);
        }
    }

    public async Task BroadcastAsync<TPayload>(string group, string eventName, TPayload? payload, CancellationToken cancellationToken)
    {
        var users = await _userSessionStore.GetUserSessionsByGroupAsync(group, cancellationToken);

        await SendMessageToClientsAsync(users, eventName, payload, cancellationToken );
    }

    public async Task BroadcastAsync<TPayload>(string eventName, TPayload? payload, Expression<Func<UserSession, bool>> filter, CancellationToken cancellationToken)
    {
        var users = await _userSessionStore.GetUserSessionsByFilterAsync(filter, cancellationToken);

        await SendMessageToClientsAsync(users, eventName, payload, cancellationToken );
    }
}