using Library.Data.PostgreSql.Entities;
using Library.Data.PostgreSql.Repositories;
using Library.Exceptions;
using Library.Models;
using Library.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Library.Data.PostgreSql.UnitTests;

public class BookRepositoryTests
{
    private AppDbContext NewDb() => DbFactory.Create($"book_{Guid.NewGuid()}");
 
    [Fact]
    public async Task CreateBook_ScientificCategory_SavesAndReturnsId()
    {
        await using var db = NewDb();
        var repo = new BookRepository(db);
        var book = new Book
        {
            Title = "Физика", Authors = ["Иванов"],
            Year = 2020, Category = BookCategory.ScientificBook,
            Status = BookStatus.Available, Description = "Desc"
        };
 
        var id = await repo.CreateBook(book);
 
        Assert.NotEqual(Guid.Empty, id);
        Assert.True(await db.Books.AnyAsync(b => b.Id == id));
    }
 
    [Fact]
    public async Task CreateBook_FictionCategory_PersistsAsFictionEntity()
    {
        await using var db = NewDb();
        var repo = new BookRepository(db);
        var book = new Book
        {
            Title = "Роман", Authors = ["Автор"], Year = 2021,
            Category = BookCategory.FictionBook, Status = BookStatus.Available, Description = ""
        };
 
        var id = await repo.CreateBook(book);
 
        Assert.True(await db.FictionBooks.AnyAsync(b => b.Id == id));
    }
 
    [Fact]
    public async Task CreateBook_UnsupportedCategory_Throws()
    {
        await using var db = NewDb();
        var repo = new BookRepository(db);
        var book = new Book { Category = (BookCategory)999, Status = BookStatus.Available };
 
        await Assert.ThrowsAsync<ArgumentException>(() => repo.CreateBook(book));
    }
 
    [Fact]
    public async Task GetBookById_WhenExists_ReturnsBook()
    {
        await using var db = NewDb();
        var entity = new FictionBookEntity
        {
            Title = "Тест", Authors = ["Авт"], Year = 2000,
            Status = BookStatus.Available, Description = "", CoverImagePath = ""
        };
        db.Add(entity);
        await db.SaveChangesAsync();
 
        var repo = new BookRepository(db);
        var book = await repo.GetBookById(entity.Id);
 
        Assert.Equal("Тест", book.Title);
        Assert.Equal(BookCategory.FictionBook, book.Category);
    }
 
    [Fact]
    public async Task GetBookById_WhenNotFound_ThrowsBookNotFoundException()
    {
        await using var db = NewDb();
        var repo = new BookRepository(db);
 
        await Assert.ThrowsAsync<BookNotFoundException>(() => repo.GetBookById(Guid.NewGuid()));
    }
 
    [Fact]
    public async Task GetBooks_FiltersAndPaginates()
    {
        await using var db = NewDb();
        for (var i = 0; i < 5; i++)
            db.Add(new FictionBookEntity
            {
                Title = $"Книга {i}", Authors = ["Автор A"], Year = 2020,
                Status = BookStatus.Available, Description = "", CoverImagePath = ""
            });
        await db.SaveChangesAsync();
 
        var repo = new BookRepository(db);
        var result = 
            await repo.GetBooks(BookStatus.Available, BookCategory.FictionBook, "Автор A", 1, 3);
 
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(3, result.PageSize);
    }
 
    [Fact]
    public async Task GetBooks_WhenEmptyAuthorFilter_ReturnsAll()
    {
        await using var db = NewDb();
        db.Add(new FictionBookEntity
        {
            Title = "Кн", Authors = ["X"], Year = 2020,
            Status = BookStatus.Available, Description = "", CoverImagePath = ""
        });
        db.Add(new FictionBookEntity
        {
            Title = "Кн2", Authors = ["Y"], Year = 2020,
            Status = BookStatus.Available, Description = "", CoverImagePath = ""
        });
        await db.SaveChangesAsync();
 
        var repo = new BookRepository(db);
        var result = await repo.GetBooks(BookStatus.Available, BookCategory.FictionBook, "", 1, 10);
 
        Assert.Equal(2, result.Items.Count);
    }
 
    [Fact]
    public async Task GetBooks_SecondPage_ReturnsCorrectItems()
    {
        await using var db = NewDb();
        for (var i = 0; i < 4; i++)
            db.Add(new FictionBookEntity
            {
                Title = $"Кн{i}", Authors = ["A"], Year = 2020,
                Status = BookStatus.Available, Description = "", CoverImagePath = ""
            });
        await db.SaveChangesAsync();
 
        var repo = new BookRepository(db);
        var page2 = 
            await repo.GetBooks(BookStatus.Available, BookCategory.FictionBook, "", 2, 2);
 
        Assert.Equal(2, page2.Items.Count);
        Assert.Equal(2, page2.Page);
    }
 
    [Fact]
    public async Task UpdateBook_WhenExists_UpdatesFields()
    {
        await using var db = NewDb();
        var entity = new FictionBookEntity
        {
            Title = "Старый", Authors = ["A"], Year = 2000,
            Status = BookStatus.Available, Description = "", CoverImagePath = ""
        };
        db.Add(entity);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
 
        var repo = new BookRepository(db);
        await repo.UpdateBook(entity.Id, new Book
        {
            Title = "Новый", Authors = ["B"], Year = 2025,
            Status = BookStatus.Available, Description = "Описание", CoverImagePath = ""
        });
 
        var updated = await db.Books.FindAsync(entity.Id);
        Assert.Equal("Новый", updated!.Title);
        Assert.Equal(2025, updated.Year);
    }
 
    [Fact]
    public async Task UpdateBook_WhenNotFound_ThrowsBookNotFoundException()
    {
        await using var db = NewDb();
        var repo = new BookRepository(db);
 
        await Assert.ThrowsAsync<BookNotFoundException>(() =>
            repo.UpdateBook(Guid.NewGuid(), new Book()));
    }
 
    [Fact]
    public async Task GetAvailableOldBooks_ReturnsOnlyOldAvailableBooks()
    {
        await using var db = NewDb();
        var oldBook = new FictionBookEntity
        {
            Title = "Старая", Authors = ["A"], Year = 2000,
            Status = BookStatus.Available, Description = "", CoverImagePath = ""
        };
        var newBook = new FictionBookEntity
        {
            Title = "Новая", Authors = ["B"], Year = 2024,
            Status = BookStatus.Borrow, Description = "", CoverImagePath = ""
        };
        db.AddRange(oldBook, newBook);
        await db.SaveChangesAsync();
 
        var repo = new BookRepository(db);
        var result = 
            await repo.GetAvailableOldBooks(DateOnly.FromDateTime(DateTime.UtcNow), 1, 100);
 
        Assert.Contains(result, r => r.Id == oldBook.Id);
        Assert.DoesNotContain(result, r => r.Id == newBook.Id);
    }
}