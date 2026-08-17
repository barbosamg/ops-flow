namespace OpsFlow.Web.Clients.Common;

public sealed class ApiClientException : Exception
{
    public ApiClientException(
        int statusCode,
        string message,
        string? correlationId,
        IReadOnlyDictionary<string, string[]> errors)
        : base(message)
    {
        StatusCode = statusCode;
        CorrelationId = correlationId;
        Errors = errors;
    }

    public int StatusCode { get; }
    public string? CorrelationId { get; }
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
