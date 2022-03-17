using Azure.Messaging.ServiceBus;
using web.Models;
using web.Models.Entities;
using web.Services.Interfaces;

namespace web.Services.Serverless;

public class ServerlessQueueListener : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    private ServiceBusClient? _client;
    private ServiceBusProcessor? _processor;

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
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken)!;
            await _processor.DisposeAsync();
        }

        if (_client is not null)
        {
            await _client.DisposeAsync();
        }

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

            if (message is null)
            {
                return;
            }
            var connectionId = message.Data?.ConnectionId;

            if (string.IsNullOrWhiteSpace(connectionId))
            {
                return;
            }

            var sessionStore = _serviceProvider.GetService<IUserSessionStore>();

            if (sessionStore is null)
            {
                return;
            }

            if (message.IsConnectedEvent)
            {
                var record = new UserSession
                {
                    UserName = message.Data?.UserId,
                    Group = string.Empty,
                    ConnectionId = connectionId,
                    LastConnectedDate = DateTimeOffset.UtcNow
                };

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