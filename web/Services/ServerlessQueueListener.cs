using Azure.Messaging.ServiceBus;
using web.Data;

namespace web.Services;

public class ServerlessQueueListener : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    private ServiceBusClient _client;
    private ServiceBusProcessor _processor;

    public ServerlessQueueListener(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client = new ServiceBusClient(_configuration.GetConnectionString("ServiceBus"));
        _processor = _client.CreateProcessor("test-queue");
        _processor.ProcessMessageAsync += OnNewMessage;
        _processor.ProcessErrorAsync += OnError;
        await _processor.StartProcessingAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await _processor.DisposeAsync();
        await _client.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }

    private Task OnError(ProcessErrorEventArgs arg)
    {
        Console.WriteLine($"Error on service bus {arg.Exception.Message}");
        return Task.CompletedTask;
    }

    private async Task OnNewMessage(ProcessMessageEventArgs arg)
    {
        try
        {
            var message = arg.Message.Body.ToObjectFromJson<QueueMessage?>();
            var connectionId = message?.Data.ConnectionId;

            if (string.IsNullOrWhiteSpace(connectionId))
            {
                return;
            }

            var sessionStore = _serviceProvider.GetService<IUserSessionStore>();

            if (message.IsConnectedEvent)
            {
                var record = new UserSessionRecord(Guid.NewGuid(),
                    message.Data.UserId,
                    string.Empty,
                    connectionId,
                    DateTimeOffset.UtcNow);

                await sessionStore.AddUserSessionAsync(record, arg.CancellationToken);
                await arg.CompleteMessageAsync(arg.Message, arg.CancellationToken);
            }
            else if (message.IsDisConnectedEvent)
            {
                await sessionStore.RemoveUserSessionAsync(connectionId, arg.CancellationToken);
                await arg.CompleteMessageAsync(arg.Message, arg.CancellationToken);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("!!!!!!! Queue Listener !!!!!!" + e);
        }
    }
}