using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using web.Data;

namespace web.Services;

public class UserSessionStoreDbContext : DbContext
{
    public DbSet<UserSessionRecord> UserSessions { get; set; }

    public UserSessionStoreDbContext(DbContextOptions<UserSessionStoreDbContext> options) : base(options)
    {

    }
}

public class UserSessionStoreEf : IUserSessionStore
{
    private readonly IDbContextFactory<UserSessionStoreDbContext> _dbContextFactory;


    public UserSessionStoreEf(IDbContextFactory<UserSessionStoreDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<UserSessionRecord>> GetUserSessionsAsync(CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var sessions = await context.UserSessions.AsNoTracking().ToArrayAsync(cancellationToken);
        return sessions;
    }

    public async Task<IEnumerable<UserSessionRecord>> GetUserSessionsByGroupAsync(string group, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var sessions = await context.UserSessions.AsNoTracking().Where(x => x.Group == group).ToArrayAsync(cancellationToken);
        return sessions;
    }

    public async Task<IEnumerable<UserSessionRecord>> GetUserSessionsByFilterAsync(Expression<Func<UserSessionRecord, bool>> filter, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var sessions = await context.UserSessions.AsNoTracking().Where(filter).ToArrayAsync(cancellationToken);
        return sessions;
    }

    public async Task AddUserSessionAsync(UserSessionRecord record, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await context.AddAsync(record, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task RemoveUserSessionAsync(string connectionId, CancellationToken cancellationToken)
        => RemoveAsync(x => x.ConnectionId == connectionId, cancellationToken);

    public async Task RemoveAsync(Expression<Func<UserSessionRecord, bool>> filter, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        object[] sessions = await context
            .UserSessions
            .Where(filter)
            .ToArrayAsync(cancellationToken);

        context.RemoveRange(sessions);

        await context.SaveChangesAsync(cancellationToken);
    }
}