using Library.Abstraction.Storage.Repositories;
using Library.Data.PostgreSql.Entities;
using Library.Dto.Output;
using Library.Exceptions;
using Library.Models;
using Library.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Library.Data.PostgreSql.Repositories;

/// <inheritdoc cref="IReaderRepository"/>
public class ReaderRepository : IReaderRepository
{
    private readonly AppDbContext _appDbContext;

    public ReaderRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    
    /// <inheritdoc cref="IReaderRepository.CreateReader"/>
    public async Task<Guid> CreateReader(Reader reader)
    {
        if (_appDbContext.Readers.Any(r => r.PhoneNumber == reader.PhoneNumber))
        {
            throw new ReaderAlreadyExistsException(
                $"Читатель с таким номером телефона уже существует: {reader.PhoneNumber}");
        }

        var entity = new ReaderEntity
        {
            FullName = reader.FullName,
            PhoneNumber = reader.PhoneNumber,
            ExpiryDate = reader.ExpiryDate,
            IsActive = reader.IsActive,
            Email = reader.Email
        };
        
        _appDbContext.Add(entity);
        await _appDbContext.SaveChangesAsync();
        
        return entity.Id;
    }

    /// <inheritdoc cref="IReaderRepository.GetReaderById"/>
    public async Task<Reader> GetReaderById(Guid readerId)
    {
        var entity = await _appDbContext.Readers.FindAsync(readerId)
            ?? throw new ReaderNotFoundException($"Отсутствует читатель с идентификатором: {readerId}");
        
        return new Reader
        {
            FullName = entity.FullName,
            PhoneNumber = entity.PhoneNumber,
            ExpiryDate = entity.ExpiryDate,
            IsActive = entity.IsActive,
            Email = entity.Email
        };
    }

    /// <inheritdoc cref="IReaderRepository.GetBorrowedBooks"/>
    public async Task<IList<BorrowedBookDto>> GetBorrowedBooks(Guid readerId)
    {
        var entity = await _appDbContext.Readers
            .AsNoTracking()
            .Where(r => r.Id == readerId)
            .Include(r => r.BorrowedRecords)
            .FirstOrDefaultAsync();

        return entity == null
            ? throw new ReaderNotFoundException($"Отсутствует читатель с идентификатором: {readerId}")
            : entity.BorrowedRecords
                .Where(b => b.Status == BookIssueStatus.Issued)
                .Select(b => new BorrowedBookDto(b.BookId, b.BorrowDate, b.DueDate))
                .ToList();
    }

    /// <inheritdoc cref="IReaderRepository.UpdateReader"/>
    public async Task UpdateReader(Guid readerId, Reader reader)
    {
        var entity = await _appDbContext.Readers.FindAsync(readerId)
                     ?? throw new ReaderNotFoundException($"Отсутствует читатель с идентификатором: {readerId}");
        
        entity.FullName = reader.FullName;
        entity.PhoneNumber = reader.PhoneNumber;
        entity.ExpiryDate = reader.ExpiryDate;
        entity.IsActive = reader.IsActive;
        entity.Email = reader.Email;
        
        _appDbContext.Update(entity);
        await _appDbContext.SaveChangesAsync();
    }
}