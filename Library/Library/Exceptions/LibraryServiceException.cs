namespace Library.Exceptions;

public class LibraryServiceException : AppException
{
    public LibraryServiceException(string message) : base($"{message}")
    {
    }

    public LibraryServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}