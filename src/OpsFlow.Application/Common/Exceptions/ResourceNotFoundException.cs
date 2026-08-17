namespace OpsFlow.Application.Common.Exceptions;

public sealed class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string resourceName, object key)
        : base($"{resourceName} '{key}' was not found.")
    {
    }
}
