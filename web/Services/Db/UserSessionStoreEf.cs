using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using web.Models.Entities;
using web.Services.Interfaces;

namespace web.Services.Db;

public class UserSessionStoreEf : IUserSessionStore
{
    private readonly IDbContextFactory<UserSessionStoreDbContext> _dbContextFactory;

    public UserSessionStoreEf(IDbContextFactory<UserSessionStoreDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<UserSession>> GetUserSessionsAsync(CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var sessions = await context.UserSessions!.AsNoTracking().ToArrayAsync(cancellationToken);
        return sessions;
    }

    public async Task<IEnumerable<UserSession>> GetUserSessionsByGroupAsync(string group, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var sessions = await context.UserSessions!.AsNoTracking().Where(x => x.Group == group).ToArrayAsync(cancellationToken);
        return sessions;
    }

    public async Task<IEnumerable<UserSession>> GetUserSessionsByFilterAsync(Expression<Func<UserSession, bool>> filter, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var sessions = await context.UserSessions!.AsNoTracking().Where(filter).ToArrayAsync(cancellationToken);
        return sessions;
    }

    public async Task AddUserSessionAsync(UserSession record, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await context.AddAsync(record, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task RemoveUserSessionAsync(string? connectionId, CancellationToken cancellationToken)
        => RemoveAsync(x => x.ConnectionId == connectionId, cancellationToken);

    public async Task RemoveAsync(Expression<Func<UserSession, bool>> filter, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        object[] sessions = await context
            .UserSessions!
            .Where(filter)
            .ToArrayAsync(cancellationToken);

        context.RemoveRange(sessions);

        await context.SaveChangesAsync(cancellationToken);
    }
}