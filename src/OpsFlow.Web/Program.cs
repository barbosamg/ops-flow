using OpsFlow.Web.Components;
using Telerik.Blazor.Services;
using OpsFlow.Web.Clients.Customers;
using OpsFlow.Web.Clients.Dashboard;
using OpsFlow.Web.Clients.Orders;
using OpsFlow.Web.Clients.Providers;
using OpsFlow.Web.Realtime;

var builder = WebApplication.CreateBuilder(args);

// Serviços usados pelo circuito Blazor.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddTelerikBlazor();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException(
        "API base URL is not configured.");

builder.Services.AddHttpClient<IOrdersApiClient, OrdersApiClient>(
    client =>
    {
        client.BaseAddress = new Uri(
            apiBaseUrl,
            UriKind.Absolute);
    });

builder.Services.AddHttpClient<IProvidersApiClient, ProvidersApiClient>(
    client =>
    {
        client.BaseAddress = new Uri(
            apiBaseUrl,
            UriKind.Absolute);
    });

builder.Services.AddHttpClient<ICustomersApiClient, CustomersApiClient>(
    client =>
    {
        client.BaseAddress = new Uri(
            apiBaseUrl,
            UriKind.Absolute);
    });

builder.Services.AddHttpClient<IDashboardApiClient, DashboardApiClient>(
    client =>
    {
        client.BaseAddress = new Uri(
            apiBaseUrl,
            UriKind.Absolute);
    });

// O serviço scoped mantém uma conexão SignalR por circuito Blazor.
builder.Services.AddScoped<OrderUpdatesClient>();

var app = builder.Build();

// Pipeline HTTP do frontend.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // HSTS força HTTPS fora do ambiente de desenvolvimento.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
