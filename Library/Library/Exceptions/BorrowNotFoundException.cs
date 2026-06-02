namespace Library.Exceptions;

public class BorrowNotFoundException : ClientErrorException
{
    public BorrowNotFoundException(string message) : base(message)
    {
    }
}