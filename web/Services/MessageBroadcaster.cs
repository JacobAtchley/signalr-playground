using Coravel.Invocable;
using web.Data;

namespace web.Services
{
    public class MessageBroadcaster : IInvocable
    {
        private readonly IBroadcastService _broadcastService;

        public MessageBroadcaster(IBroadcastService broadcastService)
        {
            _broadcastService = broadcastService;
        }

        public async Task Invoke()
        {
            await _broadcastService.BroadcastAsync("broadcastMessage", new Message{ Date = DateTime.UtcNow, Text = $"Hello from broadcaster {Guid.NewGuid()}" }, default);
        }
    }
}