using System.Linq.Expressions;
using web.Data;

namespace web.Services;

public interface IUserSessionStore
{
    Task<IEnumerable<UserSessionRecord>> GetUserSessionsAsync(CancellationToken cancellationToken);

    Task<IEnumerable<UserSessionRecord>> GetUserSessionsByGroupAsync(string group, CancellationToken cancellationToken);

    Task<IEnumerable<UserSessionRecord>> GetUserSessionsByFilterAsync(Expression<Func<UserSessionRecord, bool>> filter, CancellationToken cancellationToken);

    Task AddUserSessionAsync(UserSessionRecord record, CancellationToken cancellationToken);

    Task RemoveUserSessionAsync(string connectionId, CancellationToken cancellationToken);
}