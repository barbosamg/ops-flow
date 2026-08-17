using FluentValidation;
using OpsFlow.Api.Endpoints;
using OpsFlow.Application.Orders.Queries.GetOrders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<
    IValidator<GetOrdersQuery>,
    GetOrdersQueryValidator>();

var app = builder.Build();

app.MapGet("/", () => "OpsFlow API");

app.MapOrdersEndpoints();

app.Run();