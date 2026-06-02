namespace Library.Exceptions;

public sealed class BookServiceException : AppException
{
    public BookServiceException(string message) : base($"{message}")
    {
    }

    public BookServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}