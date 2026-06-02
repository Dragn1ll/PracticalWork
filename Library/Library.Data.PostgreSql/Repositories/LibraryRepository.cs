using Library.Abstraction.Storage.Repositories;
using Library.Data.PostgreSql.Entities;
using Library.Dto.Output;
using Library.Exceptions;
using Library.Models;
using Library.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Library.Data.PostgreSql.Repositories;

/// <inheritdoc cref="ILibraryRepository"/>
public class LibraryRepository : ILibraryRepository
{
    private readonly AppDbContext _appDbContext;

    public LibraryRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    
    /// <inheritdoc cref="ILibraryRepository.CreateBorrow"/>
    public async Task<Guid> CreateBorrow(Borrow borrow)
    {
        var entity = new BookBorrowEntity
        {
            BookId = borrow.BookId,
            ReaderId = borrow.ReaderId,
            BorrowDate = borrow.BorrowDate,
            DueDate = borrow.DueDate,
            Status = borrow.Status
        };
        
        _appDbContext.Add(entity);
        await _appDbContext.SaveChangesAsync();
        
        return entity.Id;
    }

    /// <inheritdoc cref="ILibraryRepository.GetBorrowByBookId"/>
    public async Task<Borrow> GetBorrowByBookId(Guid bookId)
    {
        var entity = await _appDbContext.BookBorrows. AsNoTracking()
            .SingleOrDefaultAsync(b => b.BookId == bookId && b.Status == BookIssueStatus.Issued)
            ?? throw new BorrowNotFoundException(
                $"Отсутствует активная запись о выдачи книги с идентификатором: {bookId}");
        
        return new Borrow
        {
            BookId = bookId,
            ReaderId = entity.ReaderId,
            BorrowDate = entity.BorrowDate,
            DueDate = entity.DueDate,
            Status = entity.Status
        };
    }
    
    /// <inheritdoc cref="ILibraryRepository.GetBookById"/>
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
    
    /// <inheritdoc cref="ILibraryRepository.GetLibraryBooks"/>
    public async Task<PagedListDto<LibraryBookDto>> GetLibraryBooks(BookCategory category, string author, bool availableOnly, 
        int page, int pageSize)
    {
        var books = GetBookCategoryData(category);
        var entities = await books.AsNoTracking()
            .Include(b => b.IssuanceRecords)
            .Where(b => (b.Authors.Contains(author) || string.IsNullOrEmpty(author)) 
                        && b.Status != BookStatus.Archived
                        && (b.Status == BookStatus.Available || !availableOnly))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = entities.Select(e =>
            {
                var lastIssuance = e.IssuanceRecords.LastOrDefault(ir =>
                    ir.Status == BookIssueStatus.Issued);

                if (lastIssuance == null)
                {
                    return new LibraryBookDto(e.Title, e.Authors, e.Description, e.Year);
                }

                return new LibraryBookDto(e.Title, e.Authors, e.Description, e.Year, lastIssuance.ReaderId,
                    lastIssuance.BorrowDate, lastIssuance.DueDate);
            })
            .ToList();
        
        return new PagedListDto<LibraryBookDto>(
            items,
            page,
            pageSize,
            entities.Count
        );
    }
    
    /// <inheritdoc cref="ILibraryRepository.GetBookIdByTitle"/>
    public async Task<Guid> GetBookIdByTitle(string title)
    {
        var entity = await _appDbContext.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Title == title);

        return entity?.Id ?? throw new BookNotFoundException($"Отсутствует книга с названием: {title}");
    }

    /// <inheritdoc cref="ILibraryRepository.UpdateBorrow"/>
    public async Task UpdateBorrow(Borrow borrow)
    {
        var entity = await _appDbContext.BookBorrows
                         .SingleOrDefaultAsync(b => b.BookId == borrow.BookId 
                                                    && b.ReaderId == borrow.ReaderId
                                                    && b.Status == BookIssueStatus.Issued)
                     ?? throw new BorrowNotFoundException("Отсутствует активная запись о выдачи книги");
    
        entity.ReturnDate = borrow.ReturnDate;
        entity.Status = borrow.Status;
    
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<IList<BorrowedIssuedBookInfoDto>> GetBorrowedIssuedBooksInfo(DateOnly targetDueDate)
    {
        return await _appDbContext.BookBorrows
            .AsNoTracking()
            .Include(b => b.Book)
            .Include(b => b.Reader)
            .Where(b => b.Status == BookIssueStatus.Issued &&
                        b.ReturnDate == targetDueDate)
            .Select(b => new BorrowedIssuedBookInfoDto
            {
                BookTitle = b.Book.Title,
                BookAuthors = b.Book.Authors,
                ReaderId = b.Reader.Id,
                ReaderFullName = b.Reader.FullName,
                ReaderEmail = b.Reader.Email,
                BorrowId = b.Id,
                BorrowDueDate = b.DueDate
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