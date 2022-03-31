using System.Text.Json;
using LitRedis.Core.Interfaces;
using StackExchange.Redis;
using web.Models;
using web.Models.Entities;
using web.Services.Interfaces;

namespace web.Services.Db;

public class EntityEventSessionStoreRedis<TEntity> : IEntityEventSessionStore<TEntity>
    where TEntity : EntityEventSubscription
{
    private readonly ILitRedisConnectionService _connectionService;

    public EntityEventSessionStoreRedis(ILitRedisConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    private string GetKey(EntityTrigger trigger) => $"[SignalRSessionEventSubscriptions][{typeof(TEntity).Name}][{trigger}]";

    public Task RegisterAsync(TEntity subscription, CancellationToken cancellationToken)
    {
        var key = GetKey(subscription.Trigger);

        return _connectionService.UseDbAsync(async (db, ct) =>
        {
            await db.SetAddAsync(key, JsonSerializer.Serialize(subscription));
            return true;
        }, cancellationToken);
    }

    public Task<List<TEntity>?> GetConnectionsAsync(EntityTrigger trigger, CancellationToken cancellationToken)
    {
        var key = GetKey(trigger);

        return _connectionService.UseDbAsync(async (db, ct) =>
        {
            var values = db.SetScanAsync(key);
            var list = new List<TEntity>();

            await foreach (var redisValue in values.WithCancellation(ct))
            {
                if (redisValue.IsNullOrEmpty)
                {
                    continue;
                }

                list.Add(JsonSerializer.Deserialize<TEntity>(redisValue));
            }

            return list;
        }, cancellationToken);
    }

    public async Task RemoveConnectionsOlderThanAsync(DateTimeOffset cutoff, CancellationToken cancellationToken)
    {
        var list = new List<EntityTrigger>
        {
            EntityTrigger.Added,
            EntityTrigger.Deleted,
            EntityTrigger.Updated
        };


        foreach (var trigger in list)
        {
            var connections = await GetConnectionsAsync(trigger, cancellationToken);

            var redisValues = (from connection in connections
                               where connection.SubscriptionDate <= cutoff
                               select new RedisValue(JsonSerializer.Serialize(connection))).ToArray();

            await _connectionService.UseDbAsync(async (db, ct) =>
            {
                await db.SetRemoveAsync(GetKey(trigger), redisValues);

                return true;
            }, cancellationToken);
        }
    }

    public async Task RemoveConnectionAsync(string connectionId, CancellationToken cancellationToken)
    {
        var list = new List<string>
        {
            GetKey(EntityTrigger.Added),
            GetKey(EntityTrigger.Deleted),
            GetKey(EntityTrigger.Updated)
        };

        foreach (var key in list)
        {
            await _connectionService.UseDbAsync(async (db, ct) =>
            {
                var values = db.SetScanAsync(key, $"*{connectionId}*");

                var valuesToRemove = new List<RedisValue>();

                await foreach (var value in values.WithCancellation(ct))
                {
                    valuesToRemove.Add(value);
                }

                await db.SetRemoveAsync(key, valuesToRemove.ToArray());

                return true;
            }, cancellationToken);
        }
    }
}