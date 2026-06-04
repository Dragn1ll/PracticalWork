using Library.Data.PostgreSql.Entities;
using Library.Data.PostgreSql.Repositories;
using Library.Exceptions;
using Library.Models;
using Library.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Library.Data.PostgreSql.UnitTests;

public class LibraryRepositoryTests
{
    private AppDbContext NewDb() => DbFactory.Create($"library_{Guid.NewGuid()}");
 
    [Fact]
    public async Task CreateBorrow_SavesAndReturnsId()
    {
        await using var db = NewDb();
        var repo = new LibraryRepository(db);
        var borrow = new Borrow
        {
            BookId = Guid.NewGuid(), ReaderId = Guid.NewGuid(),
            BorrowDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            Status = BookIssueStatus.Issued
        };
 
        var id = await repo.CreateBorrow(borrow);
 
        Assert.NotEqual(Guid.Empty, id);
        Assert.True(await db.BookBorrows.AnyAsync(b => b.Id == id));
    }
 
    [Fact]
    public async Task GetBorrowByBookId_WhenActiveIssuedBorrow_ReturnsBorrow()
    {
        await using var db = NewDb();
        var bookId = Guid.NewGuid();
        var entity = new BookBorrowEntity
        {
            BookId = bookId, ReaderId = Guid.NewGuid(),
            BorrowDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            Status = BookIssueStatus.Issued
        };
        db.Add(entity);
        await db.SaveChangesAsync();
 
        var repo = new LibraryRepository(db);
        var borrow = await repo.GetBorrowByBookId(bookId);
 
        Assert.Equal(bookId, borrow.BookId);
        Assert.Equal(BookIssueStatus.Issued, borrow.Status);
    }
 
    [Fact]
    public async Task GetBorrowByBookId_WhenNoActiveBorrow_ThrowsBorrowNotFoundException()
    {
        await using var db = NewDb();
        var repo = new LibraryRepository(db);
 
        await Assert.ThrowsAsync<BorrowNotFoundException>(() => repo.GetBorrowByBookId(Guid.NewGuid()));
    }
 
    [Fact]
    public async Task GetBookById_WhenNotFound_ThrowsBookNotFoundException()
    {
        await using var db = NewDb();
        var repo = new LibraryRepository(db);
 
        await Assert.ThrowsAsync<BookNotFoundException>(() => repo.GetBookById(Guid.NewGuid()));
    }
 
    [Fact]
    public async Task GetLibraryBooks_PaginatesCorrectly()
    {
        await using var db = NewDb();
        for (var i = 0; i < 5; i++)
            db.Add(new FictionBookEntity
            {
                Title = $"Книга {i}", Authors = ["А"], Year = 2020,
                Status = BookStatus.Available, Description = "", CoverImagePath = ""
            });
        await db.SaveChangesAsync();
 
        var repo = new LibraryRepository(db);
        var page1 = 
            await repo.GetLibraryBooks(BookCategory.Default, "", false, 1, 3);
        var page2 = 
            await repo.GetLibraryBooks(BookCategory.Default, "", false, 2, 3);
 
        Assert.Equal(3, page1.Items.Count);
        Assert.Equal(2, page2.Items.Count);
    }
 
    [Fact]
    public async Task GetBookIdByTitle_WhenExists_ReturnsId()
    {
        await using var db = NewDb();
        var entity = new FictionBookEntity
        {
            Title = "Найди меня", Authors = ["А"], Year = 2020,
            Status = BookStatus.Available, Description = "", CoverImagePath = ""
        };
        db.Add(entity);
        await db.SaveChangesAsync();
 
        var repo = new LibraryRepository(db);
        var id = await repo.GetBookIdByTitle("Найди меня");
 
        Assert.Equal(entity.Id, id);
    }
 
    [Fact]
    public async Task GetBookIdByTitle_WhenNotFound_ThrowsBookNotFoundException()
    {
        await using var db = NewDb();
        var repo = new LibraryRepository(db);
 
        await Assert.ThrowsAsync<BookNotFoundException>(() => repo.GetBookIdByTitle("Несуществующая"));
    }
 
    [Fact]
    public async Task UpdateBorrow_WhenExists_UpdatesStatusAndReturnDate()
    {
        await using var db = NewDb();
        var bookId = Guid.NewGuid();
        var readerId = Guid.NewGuid();
        var entity = new BookBorrowEntity
        {
            BookId = bookId, ReaderId = readerId,
            BorrowDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            Status = BookIssueStatus.Issued
        };
        db.Add(entity);
        await db.SaveChangesAsync();
 
        var returnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var borrow = new Borrow
        {
            BookId = bookId, ReaderId = readerId,
            ReturnDate = returnDate, Status = BookIssueStatus.Returned
        };
 
        var repo = new LibraryRepository(db);
        await repo.UpdateBorrow(borrow);
 
        var updated = await db.BookBorrows.FindAsync(entity.Id);
        Assert.Equal(BookIssueStatus.Returned, updated!.Status);
        Assert.Equal(returnDate, updated.ReturnDate);
    }
 
    [Fact]
    public async Task UpdateBorrow_WhenNotFound_ThrowsBorrowNotFoundException()
    {
        await using var db = NewDb();
        var repo = new LibraryRepository(db);
 
        await Assert.ThrowsAsync<BorrowNotFoundException>(() =>
            repo.UpdateBorrow(new Borrow { BookId = Guid.NewGuid(), Status = BookIssueStatus.Issued }));
    }
}