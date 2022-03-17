using Microsoft.AspNetCore.SignalR;
using web.Models.Entities;
using web.Services.Interfaces;

namespace web;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly IUserSessionStore _userSessionStore;

    public ChatHub(ILogger<ChatHub> logger, IUserSessionStore userSessionStore)
    {
        _logger = logger;
        _userSessionStore = userSessionStore;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var group = Context.GetHttpContext()?.Request.Query["group"].ToString();

            if (!string.IsNullOrWhiteSpace(group))
            {
                // await Groups.AddToGroupAsync(Context.ConnectionId, group);
                // _logger.LogInformation("User added to group. {User} {Group}", Context.UserIdentifier, group);
            }
            else
            {
                _logger.LogInformation("User is connected with no group specifiedS");
            }

            var session = new UserSession
            {
                ConnectionId = Context.ConnectionId,
                Group = group,
                UserName = Context.UserIdentifier,
                LastConnectedDate = DateTimeOffset.UtcNow
            };

            await _userSessionStore.AddUserSessionAsync(session, default);

            _logger.LogInformation(
                "User connected to chat hub. {UserId} {ConnectionId}",
                Context.UserIdentifier,
                Context.ConnectionId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in connected hub callback");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            _logger.LogInformation(
                "User disconnected from chat hub. {UserId} {ConnectionId}",
                Context.UserIdentifier,
                Context.ConnectionId);

            await _userSessionStore.RemoveUserSessionAsync(Context.ConnectionId, default);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in disconnected hub callback");
        }

        await base.OnDisconnectedAsync(exception);
    }
}