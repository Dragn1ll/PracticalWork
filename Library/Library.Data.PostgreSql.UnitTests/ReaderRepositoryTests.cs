using Library.Data.PostgreSql.Entities;
using Library.Data.PostgreSql.Repositories;
using Library.Exceptions;
using Library.Models;
using Library.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Library.Data.PostgreSql.UnitTests;

public class ReaderRepositoryTests
{
    private AppDbContext NewDb() => DbFactory.Create($"reader_{Guid.NewGuid()}");
 
    [Fact]
    public async Task CreateReader_WhenPhoneUnique_SavesAndReturnsId()
    {
        await using var db = NewDb();
        var repo = new ReaderRepository(db);
        var reader = new Reader
        {
            FullName = "Иван", PhoneNumber = "+7900000001",
            Email = "ivan@test.com", IsActive = true,
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
        };
 
        var id = await repo.CreateReader(reader);
 
        Assert.NotEqual(Guid.Empty, id);
        Assert.True(await db.Readers.AnyAsync(r => r.Id == id));
    }
 
    [Fact]
    public async Task CreateReader_WhenPhoneDuplicate_ThrowsReaderAlreadyExistsException()
    {
        await using var db = NewDb();
        db.Add(new ReaderEntity
        {
            FullName = "Существующий", PhoneNumber = "+7900000002",
            Email = "x@y.com", IsActive = true,
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
        });
        await db.SaveChangesAsync();
 
        var repo = new ReaderRepository(db);
        await Assert.ThrowsAsync<ReaderAlreadyExistsException>(() =>
            repo.CreateReader(new Reader
            {
                FullName = "Новый", PhoneNumber = "+7900000002", Email = "a@b.com"
            }));
    }
 
    [Fact]
    public async Task GetReaderById_WhenExists_ReturnsReader()
    {
        await using var db = NewDb();
        var entity = new ReaderEntity
        {
            FullName = "Пётр", PhoneNumber = "+7999",
            Email = "p@t.com", IsActive = true,
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
        };
        db.Add(entity);
        await db.SaveChangesAsync();
 
        var repo = new ReaderRepository(db);
        var reader = await repo.GetReaderById(entity.Id);
 
        Assert.Equal("Пётр", reader.FullName);
    }
 
    [Fact]
    public async Task GetReaderById_WhenNotFound_ThrowsReaderNotFoundException()
    {
        await using var db = NewDb();
        var repo = new ReaderRepository(db);
 
        await Assert.ThrowsAsync<ReaderNotFoundException>(() => repo.GetReaderById(Guid.NewGuid()));
    }
 
    [Fact]
    public async Task GetBorrowedBooks_WhenReaderNotFound_ThrowsReaderNotFoundException()
    {
        await using var db = NewDb();
        var repo = new ReaderRepository(db);
 
        await Assert.ThrowsAsync<ReaderNotFoundException>(() => repo.GetBorrowedBooks(Guid.NewGuid()));
    }
 
    [Fact]
    public async Task GetBorrowedBooks_ReturnsOnlyIssuedBorrows()
    {
        await using var db = NewDb();
        var readerEntity = new ReaderEntity
        {
            FullName = "Анна", PhoneNumber = "+7111",
            Email = "anna@t.com", IsActive = true,
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
        };
        db.Add(readerEntity);
        await db.SaveChangesAsync();
 
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var issuedBorrow = new BookBorrowEntity
        {
            BookId = Guid.NewGuid(), ReaderId = readerEntity.Id,
            BorrowDate = now, DueDate = now.AddDays(30), Status = BookIssueStatus.Issued
        };
        db.Add(issuedBorrow);
        db.Add(new BookBorrowEntity
        {
            BookId = Guid.NewGuid(), ReaderId = readerEntity.Id,
            BorrowDate = now.AddDays(-60), DueDate = now.AddDays(-30), Status = BookIssueStatus.Returned
        });
        await db.SaveChangesAsync();
 
        var repo = new ReaderRepository(db);
        var result = await repo.GetBorrowedBooks(readerEntity.Id);
 
        Assert.Single(result);
        Assert.Equal(result.Single().BookId, issuedBorrow.BookId);
    }
 
    [Fact]
    public async Task UpdateReader_WhenExists_UpdatesFields()
    {
        await using var db = NewDb();
        var entity = new ReaderEntity
        {
            FullName = "Старый", PhoneNumber = "+7333",
            Email = "old@t.com", IsActive = true,
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
        };
        db.Add(entity);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
 
        var repo = new ReaderRepository(db);
        await repo.UpdateReader(entity.Id, new Reader
        {
            FullName = "Новый", PhoneNumber = "+7444",
            Email = "new@t.com", IsActive = false,
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });
 
        var updated = await db.Readers.FindAsync(entity.Id);
        Assert.Equal("Новый", updated!.FullName);
        Assert.False(updated.IsActive);
    }
 
    [Fact]
    public async Task UpdateReader_WhenNotFound_ThrowsReaderNotFoundException()
    {
        await using var db = NewDb();
        var repo = new ReaderRepository(db);
 
        await Assert.ThrowsAsync<ReaderNotFoundException>(() =>
            repo.UpdateReader(Guid.NewGuid(), new Reader()));
    }
}