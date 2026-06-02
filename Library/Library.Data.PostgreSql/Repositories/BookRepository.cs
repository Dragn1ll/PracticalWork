using Library.Abstraction.Storage.Repositories;
using Library.Data.PostgreSql.Entities;
using Library.Dto.Output;
using Library.Exceptions;
using Library.Models;
using Library.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Library.Data.PostgreSql.Repositories;

/// <inheritdoc cref="IBookRepository"/>>
public sealed class BookRepository : IBookRepository
{
    private readonly AppDbContext _appDbContext;

    public BookRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    /// <inheritdoc cref="IBookRepository.CreateBook"/>
    public async Task<Guid> CreateBook(Book book)
    {
        AbstractBookEntity entity = book.Category switch
        {
            BookCategory.ScientificBook => new ScientificBookEntity(),
            BookCategory.EducationalBook => new EducationalBookEntity(),
            BookCategory.FictionBook => new FictionBookEntity(),
            _ => throw new ArgumentException($"Неподдерживаемая категория книги: {book.Category}", nameof(book.Category))
        };

        entity.Title = book.Title;
        entity.Description = book.Description;
        entity.Year = book.Year;
        entity.Authors = book.Authors;
        entity.Status = book.Status;

        _appDbContext.Add(entity);
        await _appDbContext.SaveChangesAsync();

        return entity.Id;
    }

    /// <inheritdoc cref="IBookRepository.GetBookById"/>
    public async Task<Book> GetBookById(Guid bookId)
    {
        var entity = await _appDbContext.Books.FindAsync(bookId);

        return entity == null
            ? throw new BookNotFoundException($"Отсутствует книга с идентификатором: {bookId}")
            : new Book
            {
                Title = entity.Title,
                Authors = entity.Authors,
                Description = entity.Description,
                Year = entity.Year,
                Status = entity.Status,
                CoverImagePath = entity.CoverImagePath,
                Category = entity switch
                {
                    ScientificBookEntity => BookCategory.ScientificBook,
                    EducationalBookEntity => BookCategory.EducationalBook,
                    FictionBookEntity => BookCategory.FictionBook,
                    _ => throw new ArgumentException($"Неподдерживаемый тип книги: {entity.GetType()}")
                },
                IsArchived = entity.Status == BookStatus.Archived
            };
    }

    /// <inheritdoc cref="IBookRepository.GetBooks"/>
    public async Task<PagedListDto<BookListDto>> GetBooks(BookStatus status, BookCategory category, string author, 
        int page, int pageSize)
    {
        var books = GetBookCategoryData(category);
        var entities = await books.AsNoTracking()
            .Where(b => (b.Authors.Contains(author) || string.IsNullOrEmpty(author)) 
                        && b.Status == status)
            .ToListAsync();

        var items = entities
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new BookListDto(e.Id, e.Title, e.Authors, e.Description, e.Year,
                e.CoverImagePath))
            .ToList();
        
        return new PagedListDto<BookListDto>(
            items,
            page,
            pageSize,
            entities.Count
            );
    }

    /// <inheritdoc cref="IBookRepository.UpdateBook"/>
    public async Task UpdateBook(Guid bookId, Book book)
    {
        var entity = await _appDbContext.Books.FindAsync(bookId) 
                     ?? throw new BookNotFoundException($"Отсутствует книга с идентификатором: {bookId}");

        entity.Title = book.Title;
        entity.Authors = book.Authors;
        entity.Description = book.Description;
        entity.Year = book.Year;
        entity.Status = book.Status;
        entity.CoverImagePath = book.CoverImagePath;
        
        _appDbContext.Update(entity);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<IList<AvailableOldBookDto>> GetAvailableOldBooks(DateOnly cutoffDate, int page, int pageSize)
    {
        return await _appDbContext.Books
            .Include(b => b.IssuanceRecords)
            .Where(b => b.Status == BookStatus.Available &&
                        (b.IssuanceRecords.Count == 0 ||
                         b.IssuanceRecords.All(i => i.BorrowDate < cutoffDate)))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new AvailableOldBookDto
            {
                Id = b.Id,
                Title = b.Title
            })
            .ToListAsync();
    }
    
    private IQueryable<AbstractBookEntity> GetBookCategoryData(BookCategory category)
    {
        return category switch
        {
            BookCategory.ScientificBook => _appDbContext.ScientificBooks,
            BookCategory.EducationalBook => _appDbContext.EducationalBooks,
            BookCategory.FictionBook => _appDbContext.FictionBooks,
            BookCategory.Default => _appDbContext.Books,
            _ => throw new ArgumentException($"Неподдерживаемый тип книги: { category }")
        };
    }
}