using Library.Abstraction.Services;
using Library.Abstraction.Storage;
using Library.Abstraction.Storage.Repositories;
using Library.Dto.Input;
using Library.Dto.Output;
using Library.Exceptions;
using Library.Models;
using Library.SharedKernel.Enums;
using Library.SharedKernel.Events.Book;
using Microsoft.AspNetCore.Http;
using PracticalWork.Library.Dto.Input;

namespace Library.Services;

/// <inheritdoc cref="IBookService"/>
public sealed class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly ICacheStorage _cache;
    private readonly IFileStorage _fileStorage;
    private readonly ILibraryEventProducer _producer;

    public BookService(IBookRepository bookRepository, ICacheStorage cache, IFileStorage fileStorage,
        ILibraryEventProducer producer)
    {
        _bookRepository = bookRepository;
        _cache = cache;
        _fileStorage = fileStorage;
        _producer = producer;
    }

    /// <inheritdoc cref="IBookService.CreateBook"/>
    public async Task<Guid> CreateBook(Book book)
    {
        book.Status = BookStatus.Available;
        try
        {
            var bookId = await _bookRepository.CreateBook(book);

            await _producer.PublishEventAsync(new BookCreatedEvent(bookId, book.Title, book.Category.ToString(), 
                book.Authors, book.Year, DateTime.UtcNow));

            return bookId;
        }
        catch (Exception ex)
        {
            throw new BookServiceException("Ошибка создания книги.", ex);
        }
    }

    /// <inheritdoc cref="IBookService.UpdateBook"/>
    public async Task UpdateBook(Guid bookId, UpdateBookDto updateBookDto)
    {
        try
        {
            var book = await _bookRepository.GetBookById(bookId);

            if (book.IsArchived)
            {
                throw new ClientErrorException("Книга находится в архиве.");
            }

            await InvalidationBookListCache(book);
            await InvalidationLibraryBookCache(book);

            book.Title = updateBookDto.Title;
            book.Authors = updateBookDto.Authors;
            book.Year = updateBookDto.Year;

            await _bookRepository.UpdateBook(bookId, book);
        }
        catch (Exception ex) when (ex is not ClientErrorException)
        {
            throw new BookServiceException("Ошибка обновления данных книги!", ex);
        }
    }

    /// <inheritdoc cref="IBookService.ArchiveBook"/>
    public async Task<ArchiveBookDto> ArchiveBook(Guid bookId)
    {
        try
        {
            var book = await _bookRepository.GetBookById(bookId);

            if (!book.CanBeArchived())
            {
                throw new ClientErrorException("Книга не может быть переведена в архив.");
            }

            if (book.IsArchived)
            {
                throw new ClientErrorException("Книга уже переведена в архив.");
            }

            await InvalidationBookListCache(book);
            await InvalidationLibraryBookCache(book);

            book.Archive();

            await _bookRepository.UpdateBook(bookId, book);

            await _producer.PublishEventAsync(new BookArchivedEvent(bookId, book.Title, "", DateTime.UtcNow));

            return new ArchiveBookDto(bookId, book.Title);
        }
        catch (Exception ex) when (ex is not ClientErrorException)
        {
            throw new BookServiceException("Ошибка перевода книги в архив!", ex);
        }
    }

    /// <inheritdoc cref="IBookService.GetBooks"/>
    public async Task<PagedListDto<BookListDto>> GetBooks(GetBookListDto getBookList)
    {
        try
        {
            var cacheKey = $"books:list:{HashCode.Combine(getBookList.Status, getBookList.Category, 
                getBookList.Author)}:{getBookList.Page}:{getBookList.PageSize}";
            var cache = await _cache.TryGetAsync<PagedListDto<BookListDto>>(cacheKey);
            
            if (cache == null)
            {
                var pagedBookList = await _bookRepository.GetBooks(getBookList.Status, getBookList.Category, 
                    getBookList.Author, getBookList.Page, getBookList.PageSize);

                foreach (var book in pagedBookList.Items)
                {
                    if (!string.IsNullOrWhiteSpace(book.CoverImagePath))
                    {
                        book.CoverImagePath = await _fileStorage.GetFileUrlAsync(book.CoverImagePath);
                    }
                }
                
                await _cache.SetAsync(cacheKey, pagedBookList, TimeSpan.FromMinutes(10));
                return pagedBookList;
            }
            
            return cache;
        }
        catch (Exception ex)
        {
            throw new BookServiceException("Ошибка получения списка книг!", ex);
        }
    }

    /// <inheritdoc cref="IBookService.CreateBookDetails"/>
    public async Task CreateBookDetails(Guid bookId, IFormFile coverImage, string description)
    {
        var extension = Path.GetExtension(coverImage.FileName);
        if (!IsValidImageExtension(extension) || coverImage.Length > 5 * 1024 * 1024)
        {
            throw new ClientErrorException("Неверный формат изображения!");
        }

        try
        {
            var book = await _bookRepository.GetBookById(bookId);
            if (book.IsArchived)
            {
                throw new ClientErrorException("Книга заархивирована.");
            }

            book.UpdateDetails(description,
                $"{DateTime.UtcNow.Year}/{DateTime.UtcNow.Month}/{bookId}{extension}");

            await _fileStorage.UploadFileAsync(book.CoverImagePath, coverImage.OpenReadStream(), 
                coverImage.ContentType);
            
            await _bookRepository.UpdateBook(bookId, book);

            await _cache.RemoveAsync($"book:details:{bookId}");
            await InvalidationBookListCache(book);
        }
        catch (Exception ex) when (ex is not ClientErrorException)
        {
            throw new BookServiceException("Ошибка добавления деталей книги!", ex);
        }
    }

    private async Task InvalidationBookListCache(Book book)
    {
        foreach (var author in book.Authors)
        {
            var prefix = $"books:list:{HashCode.Combine(book.Status, book.Category, author)}:";

            await _cache.RemoveByPrefixAsync(prefix);
        }
    }

    private async Task InvalidationLibraryBookCache(Book book)
    {
        foreach (var author in book.Authors)
        {
            var prefix = $"books:list:{HashCode.Combine(book.Category, author, book.Status == BookStatus.Available)}:";

            await _cache.RemoveByPrefixAsync(prefix);
        }
    }

    private bool IsValidImageExtension(string extension)
    {
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        return allowedExtensions.Contains(extension);
    }
}