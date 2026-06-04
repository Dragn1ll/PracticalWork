using FluentAssertions;
using Library.Abstraction.Services;
using Library.Abstraction.Storage;
using Library.Abstraction.Storage.Repositories;
using Library.Dto.Output;
using Library.Exceptions;
using Library.Models;
using Library.Services;
using Library.SharedKernel.Events.Reader;
using Moq;

namespace Library.UnitTests.Services;

public class ReaderServiceTests
{
    private readonly Mock<IReaderRepository> _readerRepo = new();
    private readonly Mock<ICacheStorage> _cache = new();
    private readonly Mock<ILibraryEventProducer> _producer = new();
    private readonly ReaderService _sut;
 
    public ReaderServiceTests()
    {
        _sut = new ReaderService(_readerRepo.Object, _cache.Object, _producer.Object);
    }
    
    [Fact]
    public async Task CreateReader_WhenSuccess_ReturnsGuidAndPublishesEvent()
    {
        var readerId = Guid.NewGuid();
        _readerRepo.Setup(r => r.CreateReader(It.IsAny<Reader>())).ReturnsAsync(readerId);
        _producer.Setup(p 
                => p.PublishEventAsync(It.IsAny<ReaderCreatedEvent>(), CancellationToken.None))
            .Returns(Task.CompletedTask);
 
        var result = await _sut.CreateReader(new Reader { FullName = "Иван", PhoneNumber = "+7900" });
 
        Assert.Equal(readerId, result);
        _producer.Verify(p => p.PublishEventAsync(
            It.IsAny<ReaderCreatedEvent>(), CancellationToken.None), Times.Once);
    }
 
    [Fact]
    public async Task CreateReader_SetsIsActiveToTrue()
    {
        Reader? captured = null;
        _readerRepo.Setup(r => r.CreateReader(It.IsAny<Reader>()))
            .Callback<Reader>(r => captured = r).ReturnsAsync(Guid.NewGuid());
        _producer.Setup(p 
                => p.PublishEventAsync(It.IsAny<ReaderCreatedEvent>(), CancellationToken.None))
            .Returns(Task.CompletedTask);
 
        await _sut.CreateReader(new Reader { FullName = "Тест" });
 
        Assert.True(captured!.IsActive);
    }
 
    [Fact]
    public async Task CreateReader_WhenDuplicate_ThrowsReaderAlreadyExistsException()
    {
        _readerRepo.Setup(r => r.CreateReader(It.IsAny<Reader>()))
            .ThrowsAsync(new ReaderAlreadyExistsException("Уже существует"));
 
        await Assert.ThrowsAsync<ReaderAlreadyExistsException>(() => _sut.CreateReader(new Reader()));
    }
    
    [Fact]
    public async Task ExtendValidity_WhenDateInPast_ThrowsClientErrorException()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
 
        await Assert.ThrowsAsync<ClientErrorException>(() =>
            _sut.ExtendValidity(Guid.NewGuid(), pastDate));
    }
 
    [Fact]
    public async Task ExtendValidity_WhenValid_UpdatesReader()
    {
        var reader = new Reader
        {
            IsActive = true, ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30))
        };
        var newDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));
        _readerRepo.Setup(r => r.GetReaderById(It.IsAny<Guid>())).ReturnsAsync(reader);
        _readerRepo.Setup(r 
            => r.UpdateReader(It.IsAny<Guid>(), It.IsAny<Reader>())).Returns(Task.CompletedTask);
 
        await _sut.ExtendValidity(Guid.NewGuid(), newDate);
 
        _readerRepo.Verify(r => r.UpdateReader(It.IsAny<Guid>(), It.IsAny<Reader>()), Times.Once);
    }
    
    [Fact]
    public async Task CloseReader_WhenBooksNotReturned_ThrowsClientErrorException()
    {
        var readerId = Guid.NewGuid();
        var reader = new Reader { IsActive = true, ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) };
        var borrowedBooks = new List<BorrowedBookDto>
        {
            new(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)))
        };
 
        _readerRepo.Setup(r => r.GetReaderById(readerId)).ReturnsAsync(reader);
        _readerRepo.Setup(r => r.GetBorrowedBooks(readerId)).ReturnsAsync(borrowedBooks);
        _cache.Setup(c 
            => c.TryGetAsync<IList<BorrowedBookDto>>(It.IsAny<string>())).ReturnsAsync((IList<BorrowedBookDto>?)null);
        _cache.Setup(c 
                => c.SetAsync(It.IsAny<string>(), It.IsAny<IList<BorrowedBookDto>>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);
 
        await Assert.ThrowsAsync<ClientErrorException>(() => _sut.CloseReader(readerId));
    }
 
    [Fact]
    public async Task CloseReader_WhenNoBorrows_DeactivatesAndPublishesEvent()
    {
        var readerId = Guid.NewGuid();
        var reader = new Reader { IsActive = true, ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)) };
 
        _readerRepo.Setup(r => r.GetReaderById(readerId)).ReturnsAsync(reader);
        _readerRepo.Setup(r => r.GetBorrowedBooks(readerId)).ReturnsAsync(new List<BorrowedBookDto>());
        _cache.Setup(c 
            => c.TryGetAsync<IList<BorrowedBookDto>>(It.IsAny<string>())).ReturnsAsync((IList<BorrowedBookDto>?)null);
        _cache.Setup(c 
                => c.SetAsync(It.IsAny<string>(), It.IsAny<IList<BorrowedBookDto>>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);
        _readerRepo.Setup(r 
            => r.UpdateReader(readerId, It.IsAny<Reader>())).Returns(Task.CompletedTask);
        _producer.Setup(p => p.PublishEventAsync(It.IsAny<ReaderClosedEvent>(), CancellationToken.None))
            .Returns(Task.CompletedTask);
 
        await _sut.CloseReader(readerId);
 
        Assert.False(reader.IsActive);
        _producer.Verify(p => p.PublishEventAsync(
            It.IsAny<ReaderClosedEvent>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetBorrowedBooks_WhenCacheMiss_QueriesRepoAndCaches()
    {
        var readerId = Guid.NewGuid();
        var books = new List<BorrowedBookDto>
        {
            new(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)))
        };
 
        _cache.Setup(c => c.TryGetAsync<IList<BorrowedBookDto>>($"reader:books:{readerId}"))
            .ReturnsAsync((IList<BorrowedBookDto>?)null);
        _readerRepo.Setup(r => r.GetBorrowedBooks(readerId)).ReturnsAsync(books);
        _cache.Setup(c 
                => c.SetAsync(It.IsAny<string>(), It.IsAny<IList<BorrowedBookDto>>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);
 
        var result = await _sut.GetBorrowedBooks(readerId);
 
        Assert.Single(result);
        _cache.Verify(c 
            => c.SetAsync($"reader:books:{readerId}", books, TimeSpan.FromMinutes(15)), Times.Once);
    }
 
    [Fact]
    public async Task GetBorrowedBooks_WhenCacheHit_ReturnsCachedData()
    {
        var readerId = Guid.NewGuid();
        var cached = new List<BorrowedBookDto>
        {
            new(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)))
        };
        _cache.Setup(c => c.TryGetAsync<IList<BorrowedBookDto>>($"reader:books:{readerId}"))
            .ReturnsAsync(cached);
 
        var result = await _sut.GetBorrowedBooks(readerId);
 
        _readerRepo.Verify(r => r.GetBorrowedBooks(It.IsAny<Guid>()), Times.Never);
        Assert.Same(cached, result);
    }
}