using System.Linq.Expressions;
using web.Models;
using web.Services.Interfaces;

namespace web.Services.Serverless;

public class ServerLessHubContextBroadcastService : IBroadcastService
{
    private readonly IHubContextStore _hubContextStore;
    private readonly IUserSessionStore _userSessionStore;

    public ServerLessHubContextBroadcastService(IHubContextStore hubContextStore, IUserSessionStore userSessionStore)
    {
        _hubContextStore = hubContextStore;
        _userSessionStore = userSessionStore;
    }

    public async Task BroadcastAsync<TPayload>(string eventName, TPayload? payload, CancellationToken cancellationToken)
    {
        var users = await _userSessionStore.GetUserSessionsAsync(cancellationToken);

        await SendMessageToClientsAsync(users, eventName, payload, cancellationToken);
    }

    private async Task SendMessageToClientsAsync<TPayload>( IEnumerable<UserSessionRecord> users, string eventName, TPayload? payload, CancellationToken cancellationToken)
    {
        if (payload is not null)
        {
            await _hubContextStore.ChatHubContext?.Clients
                .Clients(users.Select(x => x.ConnectionId).ToList())
                .SendCoreAsync(eventName, new object[] { payload }, cancellationToken)!;
        }
    }

    public async Task BroadcastAsync<TPayload>(string group, string eventName, TPayload? payload, CancellationToken cancellationToken)
    {
        var users = await _userSessionStore.GetUserSessionsByGroupAsync(group, cancellationToken);

        await SendMessageToClientsAsync(users, eventName, payload, cancellationToken );
    }

    public async Task BroadcastAsync<TPayload>(string eventName, TPayload? payload, Expression<Func<UserSessionRecord, bool>> filter, CancellationToken cancellationToken)
    {
        var users = await _userSessionStore.GetUserSessionsByFilterAsync(filter, cancellationToken);

        await SendMessageToClientsAsync(users, eventName, payload, cancellationToken );
    }
}