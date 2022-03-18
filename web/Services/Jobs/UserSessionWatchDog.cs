using Coravel.Invocable;
using web.Models.Entities;
using web.Services.Interfaces;

namespace web.Services.Jobs;

public class UserSessionWatchDog : IInvocable
{
    private readonly IUserSessionStore _userSessionStore;
    private readonly IEntityEventSessionStore<PersonEventSubscription> _peopleSubscriptions;
    private readonly ILogger<UserSessionWatchDog> _logger;

    public UserSessionWatchDog(IUserSessionStore userSessionStore,
        ILogger<UserSessionWatchDog> logger,
        IEntityEventSessionStore<PersonEventSubscription> peopleSubscriptions)
    {
        _userSessionStore = userSessionStore;
        _logger = logger;
        _peopleSubscriptions = peopleSubscriptions;
    }

    public async Task Invoke()
    {
        var cutoff = DateTimeOffset.UtcNow.AddHours(-6);

        await _userSessionStore.RemoveAsync(x => cutoff >= x.LastConnectedDate, default);
        await _peopleSubscriptions.RemoveConnectionsOlderThanAsync(cutoff, default);

        _logger.LogInformation("Cleared user sessions");
    }
}