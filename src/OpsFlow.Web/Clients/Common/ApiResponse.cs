using System.Net.Http.Json;
using System.Text.Json;

namespace OpsFlow.Web.Clients.Common;

internal static class ApiResponse
{
    public static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ApiProblemDetails? problem = null;

        try
        {
            problem = await response.Content
                .ReadFromJsonAsync<ApiProblemDetails>(cancellationToken);
        }
        catch (Exception exception)
            when (exception is JsonException or NotSupportedException)
        {
            // A resposta ainda será convertida em um erro seguro para a UI.
        }

        throw new ApiClientException(
            (int)response.StatusCode,
            problem?.Detail
                ?? problem?.Title
                ?? $"The API returned HTTP {(int)response.StatusCode}.",
            problem?.CorrelationId,
            problem?.Errors
                ?? new Dictionary<string, string[]>(StringComparer.Ordinal));
    }

    private sealed record ApiProblemDetails(
        string? Title,
        string? Detail,
        int? Status,
        string? CorrelationId,
        Dictionary<string, string[]>? Errors);
}
