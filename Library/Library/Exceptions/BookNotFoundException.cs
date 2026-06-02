namespace Library.Exceptions;

public class BookNotFoundException : ClientErrorException
{
    public BookNotFoundException(string message) : base(message)
    {
    }
}