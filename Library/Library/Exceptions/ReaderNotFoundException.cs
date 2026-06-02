namespace Library.Exceptions;

public class ReaderNotFoundException : ClientErrorException
{
    public ReaderNotFoundException(string message) : base(message)
    {
    }
}