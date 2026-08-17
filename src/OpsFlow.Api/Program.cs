using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpsFlow.Api.Endpoints;
using OpsFlow.Api.Background;
using OpsFlow.Api.Errors;
using OpsFlow.Api.Health;
using OpsFlow.Api.Hubs;
using OpsFlow.Api.Middleware;
using OpsFlow.Application.Customers.Ports;
using OpsFlow.Application.Dashboard.Ports;
using OpsFlow.Application.Orders.Commands.CreateOrder;
using OpsFlow.Application.Orders.Commands.RetryOrder;
using OpsFlow.Application.Orders.Commands.UpdateOrder;
using OpsFlow.Application.Orders.Ports;
using OpsFlow.Application.Orders.Queries.GetOrders;
using OpsFlow.Application.Orders.Services;
using OpsFlow.Application.Providers.Ports;
using OpsFlow.Domain.Orders;
using OpsFlow.Infrastructure.Customers;
using OpsFlow.Infrastructure.Dashboard;
using OpsFlow.Infrastructure.Orders;
using OpsFlow.Infrastructure.Messaging;
using OpsFlow.Infrastructure.Persistence;
using OpsFlow.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("OpsFlow")
    ?? throw new InvalidOperationException(
        "Connection string 'OpsFlow' is required.");

builder.Services.AddDbContextFactory<OpsFlowDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddSignalR();
builder.Services.AddOpenApi();
builder.Services.Configure<OrderQueueOptions>(
    builder.Configuration.GetSection(OrderQueueOptions.SectionName));
builder.Services.AddSingleton<AzureOrderQueueClient>();
builder.Services.AddSingleton<IOrderRetryPublisher>(serviceProvider =>
    serviceProvider.GetRequiredService<AzureOrderQueueClient>());
builder.Services.AddHostedService<OrderRetryOutboxDispatcher>();
builder.Services.AddHostedService<OrderUpdateSignalRRelay>();

builder.Services.AddScoped<IValidator<GetOrdersQuery>, GetOrdersQueryValidator>();
builder.Services.AddScoped<
    IValidator<CreateOrderCommand>,
    CreateOrderCommandValidator>();
builder.Services.AddScoped<
    IValidator<UpdateOrderCommand>,
    UpdateOrderCommandValidator>();
builder.Services.AddScoped<
    IValidator<RetryOrderCommand>,
    RetryOrderCommandValidator>();

builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddScoped<IOrderReadRepository, EfOrderReadRepository>();
builder.Services.AddScoped<
    IOrderDetailsReadRepository,
    EfOrderDetailsReadRepository>();
builder.Services.AddScoped<IOrderOutbox, EfOrderOutbox>();
builder.Services.AddScoped<ICustomerReadRepository, EfCustomerReadRepository>();
builder.Services.AddScoped<IProviderReadRepository, EfProviderReadRepository>();
builder.Services.AddScoped<IDashboardReadRepository, EfDashboardReadRepository>();
builder.Services.AddScoped<OrderApplicationService>();
builder.Services.AddScoped<OpsFlowDbSeeder>();

builder.Services.AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy(),
        tags: ["live"])
    .AddCheck<DatabaseHealthCheck>("sqlserver", tags: ["ready"])
    .AddCheck<QueueHealthCheck>("queue", tags: ["ready"]);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter<OrderStatus>());
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter<IntegrationAttemptStatus>());
});

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => TypedResults.Ok(new
{
    Service = "OpsFlow API",
    Status = "Running"
}));

app.MapOrdersEndpoints();
app.MapCustomersEndpoints();
app.MapProvidersEndpoints();
app.MapDashboardEndpoints();
app.MapSimulatedProviderEndpoints();
app.MapHub<OrderUpdatesHub>("/hubs/orders");

app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("live")
    });
app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("ready")
    });

var applyMigrations = app.Environment.IsDevelopment() ||
    app.Configuration.GetValue<bool>("Persistence:ApplyMigrationsOnStartup");

if (applyMigrations)
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OpsFlowDbContext>();
    await dbContext.Database.MigrateAsync();

    if (app.Environment.IsDevelopment() ||
        app.Configuration.GetValue<bool>("Persistence:SeedOnStartup"))
    {
        await scope.ServiceProvider
            .GetRequiredService<OpsFlowDbSeeder>()
            .SeedAsync(CancellationToken.None);
    }
}

await app.RunAsync();

public partial class Program;
