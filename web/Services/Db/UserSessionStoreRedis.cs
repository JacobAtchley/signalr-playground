using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using LitRedis.Core.Interfaces;
using MassTransit;
using StackExchange.Redis;
using web.Models.Entities;
using web.Services.Interfaces;

namespace web.Services.Db;

public class UserSessionStoreRedis : IUserSessionStore
{
    private const string KEY = "SignalRSessions";

    private readonly ILitRedisConnectionService _connectionService;

    public UserSessionStoreRedis(ILitRedisConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public Task<IEnumerable<UserSession>> GetUserSessionsAsync(CancellationToken cancellationToken)
    {
        var sessions =  _connectionService.UseDbAsync<IEnumerable<UserSession>>(async (db, ct) =>
        {
            var sessions = ScanSessions(db, cancellationToken).ToListAsync();
            return await sessions;
        }, cancellationToken);

        return sessions;
    }

    private static async IAsyncEnumerable<UserSession> ScanSessions(IDatabaseAsync db, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var values = db.SetScanAsync(KEY);

        await foreach (var value in values.WithCancellation(cancellationToken))
        {
            var valueString = value.ToString();

            if (string.IsNullOrWhiteSpace(valueString))
            {
                continue;
            }

            var session = JsonSerializer.Deserialize<UserSession>(valueString);

            if (session is not null)
            {
                yield return session;
            }
        }
    }

    public Task<IEnumerable<UserSession>> GetUserSessionsByGroupAsync(string group, CancellationToken cancellationToken)
    {
        return GetUserSessionsByFilterAsync(x => x.Group == group, cancellationToken);
    }

    public async Task<IEnumerable<UserSession>> GetUserSessionsByFilterAsync(Expression<Func<UserSession, bool>> filter, CancellationToken cancellationToken)
    {
        var sessions = await GetUserSessionsAsync(cancellationToken);
        return sessions.Where(filter.Compile()).ToArray();
    }

    public Task AddUserSessionAsync(UserSession record, CancellationToken cancellationToken)
    {
        return _connectionService.UseDbAsync((db, ct) => db.SetAddAsync(KEY, JsonSerializer.Serialize(record)), cancellationToken);
    }

    public Task RemoveUserSessionAsync(string? connectionId, CancellationToken cancellationToken)
    {
        return _connectionService.UseDbAsync(async (db, ct) =>
        {
            var values = db.SetScanAsync(KEY, $"*{connectionId}*");

            var valuesToRemove = new List<RedisValue>();

            await foreach (var value in values.WithCancellation(ct))
            {
                valuesToRemove.Add(value);
            }

            await db.SetRemoveAsync(KEY, valuesToRemove.ToArray());

            return true;
        }, cancellationToken);
    }

    public async Task RemoveAsync(Expression<Func<UserSession, bool>> filter, CancellationToken cancellationToken)
    {
        var sessions = await GetUserSessionsByFilterAsync(filter, cancellationToken);
        var values = sessions.Select(x => new RedisValue(JsonSerializer.Serialize(x))).ToArray();

        await _connectionService.UseDbAsync(async (db, ct) =>
        {
            await db.SetRemoveAsync(KEY, values);
            return true;
        }, cancellationToken);
    }
}