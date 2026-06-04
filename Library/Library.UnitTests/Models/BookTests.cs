using Library.Models;
using Library.SharedKernel.Enums;

namespace Library.UnitTests.Models;

public class BookTests
{
    [Fact]
    public void CanBeArchived_WhenStatusAvailable_ReturnsTrue()
    {
        var book = new Book { Status = BookStatus.Available };
        
        Assert.True(book.CanBeArchived());
    }
 
    [Fact]
    public void CanBeArchived_WhenStatusBorrow_ReturnsFalse()
    {
        var book = new Book { Status = BookStatus.Borrow };
        
        Assert.False(book.CanBeArchived());
    }
 
    [Fact]
    public void CanBeArchived_WhenAlreadyArchived_ReturnsTrue()
    {
        var book = new Book { Status = BookStatus.Archived, IsArchived = true };
        
        Assert.True(book.CanBeArchived());
    }

    [Fact]
    public void CanBeBorrowed_WhenAvailableAndNotArchived_ReturnsTrue()
    {
        var book = new Book { Status = BookStatus.Available, IsArchived = false };
        
        Assert.True(book.CanBeBorrowed());
    }
 
    [Fact]
    public void CanBeBorrowed_WhenArchived_ReturnsFalse()
    {
        var book = new Book { Status = BookStatus.Available, IsArchived = true };
        
        Assert.False(book.CanBeBorrowed());
    }
 
    [Fact]
    public void CanBeBorrowed_WhenStatusBorrow_ReturnsFalse()
    {
        var book = new Book { Status = BookStatus.Borrow, IsArchived = false };
        
        Assert.False(book.CanBeBorrowed());
    }

    [Fact]
    public void Archive_WhenAvailable_SetsIsArchivedAndStatus()
    {
        var book = new Book { Status = BookStatus.Available };
 
        book.Archive();
 
        Assert.True(book.IsArchived);
        Assert.Equal(BookStatus.Archived, book.Status);
    }
 
    [Fact]
    public void Archive_WhenStatusBorrow_ThrowsInvalidOperation()
    {
        var book = new Book { Status = BookStatus.Borrow };
        
        Assert.Throws<InvalidOperationException>(() => book.Archive());
    }
 
    [Fact]
    public void Archive_WhenAlreadyArchived_ThrowsInvalidOperation()
    {
        var book = new Book { Status = BookStatus.Archived, IsArchived = true };
        
        book.Archive();
        
        Assert.Equal(BookStatus.Archived, book.Status);
    }
    
    [Fact]
    public void UpdateDetails_SetsDescriptionAndCoverImagePath()
    {
        var book = new Book();
 
        book.UpdateDetails("Описание", "path/to/cover.jpg");
 
        Assert.Equal("Описание", book.Description);
        Assert.Equal("path/to/cover.jpg", book.CoverImagePath);
    }
 
    [Fact]
    public void UpdateDetails_AllowsEmptyDescription()
    {
        var book = new Book { Description = "Старое" };
        
        book.UpdateDetails(string.Empty, "cover.jpg");
        
        Assert.Equal(string.Empty, book.Description);
    }
}