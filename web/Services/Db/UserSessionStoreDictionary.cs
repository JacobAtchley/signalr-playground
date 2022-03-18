using System.Collections.Concurrent;
using System.Linq.Expressions;
using web.Models.Entities;
using web.Services.Interfaces;

namespace web.Services.Db;

internal static class UserSessionStoreDictionaryCache
{
    public static readonly ConcurrentDictionary<string, UserSession> Sessions = new();
}

public class UserSessionStoreDictionary : IUserSessionStore
{
    public Task<IEnumerable<UserSession>> GetUserSessionsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(UserSessionStoreDictionaryCache.Sessions.Values.AsEnumerable());
    }

    public Task<IEnumerable<UserSession>> GetUserSessionsByGroupAsync(string @group, CancellationToken cancellationToken)
    {
        return Task.FromResult(UserSessionStoreDictionaryCache.Sessions.Values.Where(x => x.Group == group));
    }

    public Task<IEnumerable<UserSession>> GetUserSessionsByFilterAsync(Expression<Func<UserSession, bool>> filter, CancellationToken cancellationToken)
    {
        return Task.FromResult(UserSessionStoreDictionaryCache.Sessions.Values.Where(filter.Compile().Invoke));
    }

    public Task AddUserSessionAsync(UserSession record, CancellationToken cancellationToken)
    {
        UserSessionStoreDictionaryCache.Sessions.TryAdd(record.ConnectionId!, record);
        return Task.CompletedTask;
    }

    public Task RemoveUserSessionAsync(string? connectionId, CancellationToken cancellationToken)
    {
        UserSessionStoreDictionaryCache.Sessions.TryRemove(connectionId!, out _);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Expression<Func<UserSession, bool>> filter, CancellationToken cancellationToken)
    {
        var func = filter.Compile();

        var keys = UserSessionStoreDictionaryCache.Sessions
            .Where(x => func(x.Value))
            .Select(x => x.Key)
            .ToArray();

        foreach (var k in keys)
        {
            UserSessionStoreDictionaryCache.Sessions.TryRemove(k, out _);
        }

        return Task.CompletedTask;
    }
}