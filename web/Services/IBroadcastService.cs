using System.Linq.Expressions;
using web.Models;

namespace web.Services;

public interface IBroadcastService
{
    Task BroadcastAsync<TPayload>(string eventName, TPayload? payload, CancellationToken cancellationToken);

    Task BroadcastAsync<TPayload>(string group, string eventName, TPayload? payload, CancellationToken cancellationToken);

    Task BroadcastAsync<TPayload>(string eventName, TPayload? payload, Expression<Func<UserSessionRecord, bool>> filter, CancellationToken cancellationToken);
}