using System.Linq.Expressions;
using web.Models.Entities;

namespace web.Services.Interfaces;

public interface IBroadcastService
{
    Task BroadcastAsync<TPayload>(string eventName, TPayload? payload, CancellationToken cancellationToken);

    Task BroadcastAsync<TPayload>(string group, string eventName, TPayload? payload, CancellationToken cancellationToken);

    Task BroadcastAsync<TPayload>(string eventName, TPayload? payload, Expression<Func<UserSession, bool>> filter, CancellationToken cancellationToken);
}