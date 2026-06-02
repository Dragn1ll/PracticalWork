namespace Library.Dto.Output;

public class BorrowedIssuedBookInfoDto
{
    public string BookTitle { get; set; } = string.Empty;

    public IReadOnlyList<string> BookAuthors { get; set; } = [];
    
    public Guid ReaderId { get; set; }
    
    public string ReaderFullName { get; set; } = string.Empty;
    
    public string ReaderEmail { get; set; } = string.Empty;
    
    public Guid BorrowId { get; set; }
    
    public DateOnly BorrowDueDate { get; set; }
}