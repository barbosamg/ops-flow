using FluentValidation;
using OpsFlow.Api.Endpoints;
using OpsFlow.Application.Orders.Queries.GetOrders;
using OpsFlow.Application.Orders.Ports;
using OpsFlow.Infrastructure.Orders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<
    IValidator<GetOrdersQuery>,
    GetOrdersQueryValidator>();

builder.Services.AddSingleton<
    IOrderReadRepository,
    InMemoryOrderReadRepository>();

var app = builder.Build();

app.MapGet("/", () => "OpsFlow API");

app.MapOrdersEndpoints();

app.Run();
