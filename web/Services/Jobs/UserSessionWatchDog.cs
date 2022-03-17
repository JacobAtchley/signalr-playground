using Coravel.Invocable;
using web.Services.Interfaces;

namespace web.Services.Jobs;

public class UserSessionWatchDog : IInvocable
{
    private readonly IUserSessionStore _userSessionStore;
    private readonly ILogger<UserSessionWatchDog> _logger;

    public UserSessionWatchDog(IUserSessionStore userSessionStore, ILogger<UserSessionWatchDog> logger)
    {
        _userSessionStore = userSessionStore;
        _logger = logger;
    }

    public async Task Invoke()
    {
        await _userSessionStore
            .RemoveAsync(x => x.LastConnectedDate <= DateTimeOffset.UtcNow.AddHours(-6), default);

        _logger.LogInformation("Cleared user sessions");
    }
}