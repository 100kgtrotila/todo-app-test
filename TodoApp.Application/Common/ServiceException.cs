namespace TodoApp.Application.Common;

public enum ServiceErrorType
{
    NotFound,
    Forbidden,
    Conflict,
    Validation
}

public sealed class ServiceException : Exception
{
    public ServiceErrorType ErrorType { get; }

    public ServiceException()
    {
    }

    public ServiceException(string message) : base(message)
    {
    }

    public ServiceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ServiceException(ServiceErrorType errorType, string message)
        : base(message)
    {
        ErrorType = errorType;
    }

    public ServiceException(ServiceErrorType errorType, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorType = errorType;
    }
}