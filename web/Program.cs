using Coravel;
using MatBlazor;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;
using Microsoft.EntityFrameworkCore;
using web;
using web.Data;
using web.Services;

var builder = WebApplication.CreateBuilder(args);

var useServerlessAzureSignalR = false;

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddMatBlazor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpClient>(provider =>
{
    var factory = provider.GetService<IHttpClientFactory>();
    var client = factory.CreateClient();
    return client;
});
builder.Services.AddMatToaster(config =>
{
    config.Position = MatToastPosition.BottomRight;
    config.PreventDuplicates = true;
    config.NewestOnTop = true;
    config.ShowCloseButton = true;
    config.MaximumOpacity = 95;
    config.VisibleStateDuration = 3000;
});

builder.Services.AddSingleton<IUserIdProvider, PlaygroundUserIdProvider>();
builder.Services.AddDbContextFactory<UserSessionStoreDbContext>(
    opts => opts.UseSqlServer(builder.Configuration.GetConnectionString("UserSessionStoreEf")));
//builder.Services.AddTransient<IUserSessionStore, UserSessionStoreEf>();
builder.Services.AddTransient<IUserSessionStore, UserSessionStoreDictionary>();

if (useServerlessAzureSignalR)
{
    builder.Services.AddSingleton<ServerlessSignalRService>()
        .AddHostedService(sp => sp.GetService<ServerlessSignalRService>())
        .AddSingleton<IHubContextStore>(sp => sp.GetService<ServerlessSignalRService>());

    builder.Services.AddTransient<IBroadcastService, ServerLessHubContextBroadcastService>();
    builder.Services.AddHostedService<ServerlessSignalRService>();
    builder.Services.AddHostedService<ServerlessQueueListener>();
}
else
{
    builder.Services.AddSignalR()
        .AddHubOptions<ChatHub>(chatHubOptions => { chatHubOptions.AddFilter(new ChatHubFilter()); })
        .AddAzureSignalR();

    builder.Services.AddTransient<IBroadcastService, HubContextBroadcastService>();
}

builder.Services.AddScheduler();
builder.Services.AddTransient<UserSessionWatchDog>();
builder.Services.AddTransient<MessageBroadcaster>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    if (!useServerlessAzureSignalR)
    {
        endpoints.MapHub<ChatHub>("/chat");
    }
});

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapGet("/api/username", Users.GenerateUserName);

app.MapGet("/api/connected-users", (IUserSessionStore store, CancellationToken cancellationToken) => store.GetUserSessionsAsync(cancellationToken));

if (useServerlessAzureSignalR)
{
    app.MapPost("/chat/negotiate", async (string userName, ServerlessSignalRService service, CancellationToken cancellationToken) =>
    {
        var negotiateResponse = await service.ChatHubContext.NegotiateAsync(new NegotiationOptions { UserId = userName }, cancellationToken);

        return new Dictionary<string, string?>
        {
            { "url", negotiateResponse.Url },
            { "accessToken", negotiateResponse.AccessToken }
        };
    });
}

app.Services.UseScheduler(scheduler =>
{
    scheduler.Schedule<UserSessionWatchDog>().EveryThirtySeconds();
    scheduler.Schedule<MessageBroadcaster>().EverySecond();
});

await app.RunAsync();