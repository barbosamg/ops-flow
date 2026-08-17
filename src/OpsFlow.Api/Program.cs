using System.Text.Json.Serialization;
using FluentValidation;
using OpsFlow.Api.Endpoints;
using OpsFlow.Application.Orders.Ports;
using OpsFlow.Application.Orders.Queries.GetOrders;
using OpsFlow.Application.Providers.Ports;
using OpsFlow.Domain.Orders;
using OpsFlow.Infrastructure.Orders;
using OpsFlow.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<
    IValidator<GetOrdersQuery>,
    GetOrdersQueryValidator>();

builder.Services.AddSingleton<
    IOrderReadRepository,
    InMemoryOrderReadRepository>();

builder.Services.AddSingleton<
    IProviderReadRepository,
    InMemoryProviderReadRepository>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter<OrderStatus>());
});

var app = builder.Build();

app.MapGet("/", () => "OpsFlow API");

app.MapOrdersEndpoints();
app.MapProvidersEndpoints();

app.Run();
