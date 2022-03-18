using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using web.Models;
using web.Models.Entities;
using web.Services.Interfaces;

namespace web.Services.Db;

public class EntityEventSessionStore<TEntity> : IEntityEventSessionStore<TEntity>
    where TEntity : EntityEventSubscription
{
    private readonly IDbContextFactory<UserSessionStoreDbContext> _dbContextFactory;

    public EntityEventSessionStore(IDbContextFactory<UserSessionStoreDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    private UserSessionStoreDbContext? _dbContext;
    private DbSet<TEntity>? _set;

    private async Task<DbSet<TEntity>> GetDbSetAsync(CancellationToken cancellationToken)
    {
        if (_set is not null) return _set;

        _dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        _set = _dbContext.Set<TEntity>();

        return _set;
    }

    public async Task RegisterAsync(TEntity subscription, CancellationToken cancellationToken)
    {
        var set = await GetDbSetAsync(cancellationToken);
        await set.AddAsync(subscription, cancellationToken);
        await _dbContext?.SaveChangesAsync(cancellationToken)!;
    }

    public async Task<List<TEntity>?> GetConnectionsAsync(EntityTrigger trigger, CancellationToken cancellationToken)
    {
        var set = await GetDbSetAsync(cancellationToken);
        var list = await set.Where(x => x.Trigger == trigger).ToListAsync(cancellationToken);
        return list;
    }

    public Task RemoveConnectionsOlderThanAsync(DateTimeOffset cutoff, CancellationToken cancellationToken)
    {
        return RemoveFilterAsync(x => cutoff >= x.SubscriptionDate, cancellationToken);
    }

    public Task RemoveConnectionAsync(string connectionId, CancellationToken cancellationToken)
    {
        return RemoveFilterAsync(x => x.ConnectionId == connectionId, cancellationToken);
    }

    private async Task RemoveFilterAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
    {
        var set = await GetDbSetAsync(cancellationToken);
        var list = await set.Where(filter).ToListAsync(cancellationToken);
        set.RemoveRange(list);
        await _dbContext?.SaveChangesAsync(cancellationToken)!;
    }
}