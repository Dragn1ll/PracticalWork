using Library.Abstraction.Services;
using Library.Abstraction.Storage;
using Library.Abstraction.Storage.Repositories;
using Library.Dto.Input;
using Library.Dto.Output;
using Library.Exceptions;
using Library.Models;
using Library.SharedKernel.Enums;
using Library.SharedKernel.Events.Book;

namespace Library.Services;

/// <inheritdoc cref="ILibraryService"/>
public sealed class LibraryService : ILibraryService
{
    private readonly ILibraryRepository _libraryRepository;
    private readonly ICacheStorage _cache;
    private readonly IFileStorage _fileStorage;
    private readonly ILibraryEventProducer _producer;

    public LibraryService(ILibraryRepository libraryRepository, ICacheStorage cache, IFileStorage fileStorage,
        ILibraryEventProducer producer)
    {
        _libraryRepository = libraryRepository;
        _cache = cache;
        _fileStorage = fileStorage;
        _producer = producer;
    }
    
    /// <inheritdoc cref="ILibraryService.BorrowBook"/>
    public async Task<Guid> BorrowBook(Guid bookId, Guid readerId)
    {
        try
        {
            var book = await _libraryRepository.GetBookById(bookId);
            if (!book.CanBeBorrowed())
            {
                throw new ClientErrorException("Книга не может быть выдана.");
            }
            
            var borrow = new Borrow
            {
                BookId = bookId,
                ReaderId = readerId,
                BorrowDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                Status = BookIssueStatus.Issued
            };
            
            var borrowId = await _libraryRepository.CreateBorrow(borrow);
            
            await _cache.RemoveAsync($"reader:books:{borrow.ReaderId}");
            
            await _producer.PublishEventAsync(new BookBorrowedEvent(bookId, readerId, book.Title, "", 
                borrow.BorrowDate.ToDateTime(default), borrow.DueDate.ToDateTime(default)));
            
            return borrowId;
        }
        catch (Exception ex) when (ex is not ClientErrorException)
        {
            throw new LibraryServiceException("Ошибка создания записи выдачи книги!", ex);
        }
    }

    /// <inheritdoc cref="ILibraryService.GetLibraryBooks"/>
    public async Task<PagedListDto<LibraryBookDto>> GetLibraryBooks(GetLibraryBooksDto getLibraryBooksDto)
    {
        try
        {
            var cacheKey = $"library:books:{HashCode.Combine(getLibraryBooksDto.Category, getLibraryBooksDto.Author, 
                getLibraryBooksDto.AvailableOnly)}:{getLibraryBooksDto.Page}:{getLibraryBooksDto.PageSize}";
            var cache = await _cache.TryGetAsync<PagedListDto<LibraryBookDto>>(cacheKey);

            if (cache == null)
            {
                var libraryBooks = await _libraryRepository
                    .GetLibraryBooks(getLibraryBooksDto.Category, getLibraryBooksDto.Author, getLibraryBooksDto.AvailableOnly, 
                        getLibraryBooksDto.Page, getLibraryBooksDto.PageSize);
                
                await _cache.SetAsync(cacheKey, libraryBooks, TimeSpan.FromMinutes(5));
                
                return libraryBooks;
            }
            
            return cache;
        }
        catch (Exception ex)
        {
            throw new LibraryServiceException("Ошибка получения книг библиотеки!", ex);
        }
    }

    /// <inheritdoc cref="ILibraryService.ReturnBook"/>
    public async Task ReturnBook(Guid bookId)
    {
        try
        {
            var borrow = await _libraryRepository.GetBorrowByBookId(bookId);
            
            borrow.ReturnBook();
            
            await _libraryRepository.UpdateBorrow(borrow);

            await _cache.RemoveAsync($"reader:books:{borrow.ReaderId}");
            
            await _producer.PublishEventAsync(new BookReturnedEvent(bookId, borrow.ReaderId, "", "", 
                borrow.ReturnDate.ToDateTime(default)));
        }
        catch (Exception ex) when (ex is not ClientErrorException)
        {
            throw new LibraryServiceException("Ошибка возвращения книги библиотеки!", ex);
        }
    }

    /// <inheritdoc cref="ILibraryService.GetBookDetailsById"/>
    public async Task<BookDetailsDto> GetBookDetailsById(Guid bookId)
    {
        try
        {
            var cacheKey = $"book:details:{bookId}";
            var cache = await _cache.TryGetAsync<BookDetailsDto>(cacheKey);

            if (cache == null)
            {
                var book = await _libraryRepository.GetBookById(bookId);
                if (book.IsArchived)
                {
                    throw new ClientErrorException("Книга заархивирована.");
                }
                
                var coverImagePath = string.IsNullOrEmpty(book.CoverImagePath) 
                    ? string.Empty
                    : await _fileStorage.GetFileUrlAsync(book.CoverImagePath);
                var bookDetails = new BookDetailsDto(bookId, book.Title, book.Authors, book.Description, book.Year, 
                    book.Category, book.Status, coverImagePath, book.IsArchived);
                
                await _cache.SetAsync(cacheKey, bookDetails, TimeSpan.FromMinutes(30));

                return bookDetails;
            }

            return cache;
        }
        catch (Exception ex) when (ex is not ClientErrorException)
        {
            throw new LibraryServiceException("Ошибка получения деталей книги по идентификатору!", ex);
        }
    }

    /// <inheritdoc cref="ILibraryService.GetBookDetailsByTitle"/>
    public async Task<BookDetailsDto> GetBookDetailsByTitle(string title)
    {
        try
        {
            var bookId = await _libraryRepository.GetBookIdByTitle(title);
            
            return await GetBookDetailsById(bookId);
        }
        catch (Exception ex) when (ex is not ClientErrorException)
        {
            throw new LibraryServiceException("Ошибка получения деталей книги по названию!", ex);
        }
    }
}