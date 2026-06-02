namespace Library.Exceptions;

public class ReaderAlreadyExistsException : ClientErrorException
{
    public ReaderAlreadyExistsException(string message) : base(message)
    {
    }
}