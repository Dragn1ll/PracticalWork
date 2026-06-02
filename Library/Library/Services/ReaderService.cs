using System.Text;
using Library.Abstraction.Services;
using Library.Abstraction.Storage;
using Library.Abstraction.Storage.Repositories;
using Library.Dto.Output;
using Library.Exceptions;
using Library.Models;
using Library.SharedKernel.Events.Reader;

namespace Library.Services;

/// <inheritdoc cref="IReaderService"/>
public sealed class ReaderService : IReaderService
{
    private readonly IReaderRepository _readerRepository;
    private readonly ICacheStorage _cache;
    private readonly ILibraryEventProducer _producer;

    public ReaderService(IReaderRepository readerRepository, ICacheStorage cache, 
        ILibraryEventProducer producer)
    {
        _readerRepository = readerRepository;
        _cache = cache;
        _producer = producer;
    }

    /// <inheritdoc cref="IReaderService.CreateReader"/>
    public async Task<Guid> CreateReader(Reader reader)
    {
        reader.IsActive = true;
        try
        {
            var readerId = await _readerRepository.CreateReader(reader);

            await _producer.PublishEventAsync(new ReaderCreatedEvent(readerId, reader.FullName, reader.PhoneNumber, 
                reader.ExpiryDate.ToDateTime(default), DateTime.UtcNow));
            
            return readerId;
        }
        catch (Exception ex) when (ex is not ClientErrorException)
        {
            throw new ReaderServiceException("Ошибка создания карточки читателя!", ex);
        }
    }

    /// <inheritdoc cref="IReaderService.ExtendValidity"/>
    public async Task ExtendValidity(Guid readerId, DateOnly newExpiryDate)
    {
        
        if (newExpiryDate < DateOnly.FromDateTime(DateTime.Now))
        {
            throw new ClientErrorException("Дата продления не может быть раньше сегодняшней!");
        }
        
        try
        {
            var reader = await _readerRepository.GetReaderById(readerId);
            
            reader.UpdateExpiryDate(newExpiryDate);
            
            await _readerRepository.UpdateReader(readerId, reader);
        }
        catch (Exception ex) when (ex is not ClientErrorException)
        {
            throw new ReaderServiceException("Ошибка продления карточки читателя!", ex);
        }
    }

    /// <inheritdoc cref="IReaderService.CloseReader"/>
    public async Task CloseReader(Guid readerId)
    {
        try
        {
            var reader = await _readerRepository.GetReaderById(readerId);
            
            var books = await GetBorrowedBooks(readerId);

            if (books.Any())
            {
                var sb = new StringBuilder("У пользователя есть взятые книги:\n");
                foreach (var book in books)
                {
                    sb.Append(book.BookId);
                    sb.Append(" - Дата выдачи: ");
                    sb.Append(book.BorrowDate.ToShortDateString());
                    sb.Append(" - Срок сдачи: ");
                    sb.Append(book.DueDate.ToShortDateString());
                    sb.Append('\n');
                }
                
                throw new ClientErrorException(sb.ToString());
            }
            
            reader.DeActiveReader();
            
            await _readerRepository.UpdateReader(readerId, reader);

            await _producer.PublishEventAsync(new ReaderClosedEvent(readerId, reader.FullName, DateTime.UtcNow, ""));
        }
        catch (Exception ex) when (ex is not ClientErrorException)
        {
            throw new ReaderServiceException("Ошибка закрытия карточки читателя!", ex);
        }
    }

    /// <inheritdoc cref="IReaderService.GetBorrowedBooks"/>
    public async Task<IList<BorrowedBookDto>> GetBorrowedBooks(Guid readerId)
    {
        try
        {
            var cacheKey = $"reader:books:{readerId}";
            var cache = await _cache.TryGetAsync<IList<BorrowedBookDto>>(cacheKey);
            
            if (cache == null)
            {
                var borrowedBooks = await _readerRepository.GetBorrowedBooks(readerId);
                await _cache.SetAsync(cacheKey, borrowedBooks, TimeSpan.FromMinutes(15));
                return borrowedBooks;
            }
            
            return cache;
        }
        catch (Exception ex) when (ex is not ClientErrorException)
        {
            throw new ReaderServiceException("Ошибка получения взятых книг!", ex);
        }
    }
}