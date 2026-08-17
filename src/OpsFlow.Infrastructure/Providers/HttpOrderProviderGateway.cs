using System.Net;
using System.Net.Http.Json;
using OpsFlow.Application.Orders.Integration;
using Polly.Timeout;

namespace OpsFlow.Infrastructure.Providers;

public sealed class HttpOrderProviderGateway(HttpClient httpClient) :
    IOrderProviderGateway
{
    public async Task<ProviderProcessingResult> ProcessAsync(
        ProviderProcessingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "api/simulated-provider/process",
                request,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return ProviderProcessingResult.Succeeded();
            }

            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                var failure = await response.Content
                    .ReadFromJsonAsync<ProviderFailureResponse>(
                        cancellationToken: cancellationToken);

                return ProviderProcessingResult.Rejected(
                    failure?.ErrorCode ?? "PROVIDER_REJECTED",
                    failure?.SanitizedError ??
                    "The provider rejected the order.");
            }

            response.EnsureSuccessStatusCode();
            throw new InvalidOperationException("Unreachable provider response.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ProviderProcessingResult.TimedOut();
        }
        catch (TimeoutRejectedException)
        {
            return ProviderProcessingResult.TimedOut();
        }
        catch (HttpRequestException exception)
        {
            throw new TransientProviderException(
                "The provider is temporarily unavailable.",
                exception);
        }
    }

    private sealed record ProviderFailureResponse(
        string ErrorCode,
        string SanitizedError);
}
