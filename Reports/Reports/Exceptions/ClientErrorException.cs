namespace Reports.Exceptions;

public class ClientErrorException : AppException
{
    public ClientErrorException(string message) : base(message)
    {
    }
}