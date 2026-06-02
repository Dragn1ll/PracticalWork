namespace Library.BackgroundServices.Email.Dto;

public class ActivityLogStatisticDto
{
    public int NewBooksCount { get; set; }
    
    public int NewReadersCount { get; set; }
    
    public int BorrowedBooksCount { get; set; }
    
    public int ReturnedBooksCount { get; set; }
    
    public int OverdueBooksCount { get; set; }
}