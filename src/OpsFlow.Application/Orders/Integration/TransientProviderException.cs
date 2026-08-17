namespace OpsFlow.Application.Orders.Integration;

public sealed class TransientProviderException : Exception
{
    public TransientProviderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
