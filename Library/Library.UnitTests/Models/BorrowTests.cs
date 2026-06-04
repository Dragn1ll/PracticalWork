using FluentAssertions;
using Library.Models;
using Library.SharedKernel.Enums;

namespace Library.UnitTests.Models;

public class BorrowTests
{
    private static Borrow CreateIssuedBorrow(int dueDaysFromNow = 10) => new()
    {
        BookId = Guid.NewGuid(),
        ReaderId = Guid.NewGuid(),
        BorrowDate = DateOnly.FromDateTime(DateTime.UtcNow),
        DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(dueDaysFromNow)),
        Status = BookIssueStatus.Issued
    };
    
    [Fact]
    public void ReturnBook_WhenIssued_SetsStatusReturnedAndReturnDate()
    {
        var borrow = CreateIssuedBorrow(dueDaysFromNow: 10);
 
        borrow.ReturnBook();
 
        Assert.Equal(BookIssueStatus.Returned, borrow.Status);
        Assert.NotEqual(default, borrow.ReturnDate);
    }
 
    [Fact]
    public void ReturnBook_WhenReturnedBeforeDueDate_StatusIsReturned()
    {
        var borrow = CreateIssuedBorrow(dueDaysFromNow: 10);
        
        borrow.ReturnBook();
        
        Assert.Equal(BookIssueStatus.Returned, borrow.Status);
    }
 
    [Fact]
    public void ReturnBook_WhenReturnedAfterDueDate_StatusIsOverdue()
    {
        var borrow = new Borrow
        {
            BookId = Guid.NewGuid(),
            ReaderId = Guid.NewGuid(),
            BorrowDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)),
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            Status = BookIssueStatus.Issued
        };
 
        borrow.ReturnBook();
 
        Assert.Equal(BookIssueStatus.Overdue, borrow.Status);
    }
 
    [Fact]
    public void ReturnBook_WhenAlreadyReturned_ThrowsInvalidOperation()
    {
        var borrow = CreateIssuedBorrow();
        
        borrow.ReturnBook();
 
        Assert.Throws<InvalidOperationException>(() => borrow.ReturnBook());
    }
 
    [Fact]
    public void ReturnBook_WhenStatusOverdue_ThrowsInvalidOperation()
    {
        var borrow = new Borrow { Status = BookIssueStatus.Overdue };
        
        Assert.Throws<InvalidOperationException>(() => borrow.ReturnBook());
    }
 
    [Fact]
    public void ReturnBook_SetsReturnDateToToday()
    {
        var borrow = CreateIssuedBorrow(dueDaysFromNow: 5);
        
        borrow.ReturnBook();
        
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), borrow.ReturnDate);
    }
}