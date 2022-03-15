using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Azure;

namespace web.Services;
    public interface IHubContextStore
    {
        public ServiceHubContext? ChatHubContext { get; }
    }

    public class ServerlessSignalRService : IHostedService, IHubContextStore
    {
        private const string CHAT_HUB = "chat";
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public ServiceHubContext? ChatHubContext { get; private set; }

        public ServerlessSignalRService(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            using var manager = new ServiceManagerBuilder()
                .WithConfiguration(_configuration)
                .WithLoggerFactory(_loggerFactory)
                .BuildServiceManager();

            ChatHubContext = await manager.CreateHubContextAsync(CHAT_HUB, cancellationToken);
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Dispose(ChatHubContext);
        }

        private static Task Dispose(IServiceHubContext? hubContext)
        {
            return hubContext == null ? Task.CompletedTask : hubContext.DisposeAsync();
        }
    }