using System.Linq.Expressions;
using web.Models.Entities;

namespace web.Services.Interfaces;

public interface IUserSessionStore
{
    Task<IEnumerable<UserSession>> GetUserSessionsAsync(CancellationToken cancellationToken);

    Task<IEnumerable<UserSession>> GetUserSessionsByGroupAsync(string group, CancellationToken cancellationToken);

    Task<IEnumerable<UserSession>> GetUserSessionsByFilterAsync(Expression<Func<UserSession, bool>> filter, CancellationToken cancellationToken);

    Task AddUserSessionAsync(UserSession record, CancellationToken cancellationToken);

    Task RemoveUserSessionAsync(string? connectionId, CancellationToken cancellationToken);

    Task RemoveAsync(Expression<Func<UserSession, bool>> filter, CancellationToken cancellationToken);
}