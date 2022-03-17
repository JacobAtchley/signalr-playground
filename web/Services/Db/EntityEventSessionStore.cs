using Microsoft.EntityFrameworkCore;
using web.Models.Entities;
using web.Services.Interfaces;

namespace web.Services.Db;

public class EntityEventSessionStore<TEntity> : IEntityEventSessionStore<TEntity>
    where TEntity : EntityEventSubscription
{
    private readonly UserSessionStoreDbContext _context;

    public EntityEventSessionStore(UserSessionStoreDbContext context)
    {
        _context = context;
    }

    private DbSet<TEntity> GetDbSet()
    {
        return _context.Set<TEntity>();
    }

    public async Task RegisterAsync(TEntity subscription, CancellationToken cancellationToken)
    {
        var set = GetDbSet();
        await set.AddAsync(subscription, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<TEntity>?> GetConnectionsAsync(string? trigger, CancellationToken cancellationToken)
    {
        var set = GetDbSet();
        var list = await set.Where(x => x.Trigger == trigger).ToListAsync(cancellationToken);
        return list;
    }
}