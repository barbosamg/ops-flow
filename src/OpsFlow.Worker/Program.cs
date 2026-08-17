using Microsoft.EntityFrameworkCore;
using OpsFlow.Application.Orders.Integration;
using OpsFlow.Application.Orders.Ports;
using OpsFlow.Application.Orders.Services;
using OpsFlow.Infrastructure.Messaging;
using OpsFlow.Infrastructure.Orders;
using OpsFlow.Infrastructure.Persistence;
using OpsFlow.Infrastructure.Providers;
using OpsFlow.Worker;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("OpsFlow")
    ?? throw new InvalidOperationException(
        "Connection string 'OpsFlow' is required.");
var providerBaseUrl = builder.Configuration["ProviderApi:BaseUrl"]
    ?? throw new InvalidOperationException(
        "ProviderApi:BaseUrl is required.");

builder.Services.AddDbContext<OpsFlowDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.Configure<OrderQueueOptions>(
    builder.Configuration.GetSection(OrderQueueOptions.SectionName));
builder.Services.AddSingleton<AzureOrderQueueClient>();
builder.Services.AddSingleton<IOrderUpdatePublisher>(serviceProvider =>
    serviceProvider.GetRequiredService<AzureOrderQueueClient>());

builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddScoped<OrderRetryProcessor>();

builder.Services
    .AddHttpClient<IOrderProviderGateway, HttpOrderProviderGateway>(client =>
    {
        client.BaseAddress = new Uri(providerBaseUrl, UriKind.Absolute);
    })
    .AddStandardResilienceHandler(options =>
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(8);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
        options.Retry.MaxRetryAttempts = 2;
        options.Retry.Delay = TimeSpan.FromMilliseconds(200);
    });

builder.Services.AddHostedService<OrderRetryWorker>();

var host = builder.Build();
await host.RunAsync();
