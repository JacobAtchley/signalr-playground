using System.Text.Json.Serialization;
using Coravel;
using LitRedis.Core;
using MatBlazor;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using web;
using web.Data;
using web.Extensions;
using web.Services;
using web.Services.Db;
using web.Services.Hubs;
using web.Services.Interfaces;
using web.Services.Jobs;
using web.Services.Serverless;

var builder = WebApplication.CreateBuilder(args);

const bool useServerlessAzureSignalR = false;

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
//builder.Services.AddTransient<IUserSessionStore, UserSessionStoreDictionary>();
builder.Services.AddTransient<IUserSessionStore, UserSessionStoreRedis>();
builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

if (useServerlessAzureSignalR)
{
    builder.Services.AddSingleton<ServerlessSignalRService>()
        .AddHostedService(sp => sp.GetService<ServerlessSignalRService>())
        .AddSingleton<IHubContextStore>(sp => sp.GetService<ServerlessSignalRService>()!);

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
builder.Services.AddTransient<PersonEntityPump>();
builder.Services.AddAutoCrud(builder.Configuration);
builder.Services.AddRabbitMqMassTransit(builder.Configuration);
//builder.Services.AddTransient<MessageBroadcaster>();

builder.Services.AddLitRedis(redis => redis.WithCaching().WithLocking().WithConnectionString("localhost:6379,defaultDatabase=0"));

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

app.MapMyEndpoints(useServerlessAzureSignalR);


app.Services.UseScheduler(scheduler =>
{
    scheduler.Schedule<UserSessionWatchDog>().EveryThirtySeconds();
    //scheduler.Schedule<MessageBroadcaster>().EverySecond();
    //scheduler.Schedule<PersonEntityPump>().EveryFiveSeconds();
});

await app.RunAsync();