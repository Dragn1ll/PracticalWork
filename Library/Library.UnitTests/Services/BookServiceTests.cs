using Library.Abstraction.Services;
using Library.Abstraction.Storage;
using Library.Abstraction.Storage.Repositories;
using Library.Dto.Input;
using Library.Dto.Output;
using Library.Exceptions;
using Library.Models;
using Library.Services;
using Library.SharedKernel.Enums;
using Library.SharedKernel.Events.Book;
using Moq;

namespace Library.UnitTests.Services;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepo = new();
    private readonly Mock<ICacheStorage> _cache = new();
    private readonly Mock<IFileStorage> _fileStorage = new();
    private readonly Mock<ILibraryEventProducer> _producer = new();
    private readonly BookService _sut;
 
    public BookServiceTests()
    {
        _sut = new BookService(_bookRepo.Object, _cache.Object, _fileStorage.Object, _producer.Object);
    }
    
    [Fact]
    public async Task CreateBook_WhenSuccess_ReturnsGuidAndPublishesEvent()
    {
        var bookId = Guid.NewGuid();
        var book = new Book { Title = "Война и мир", Category = BookCategory.FictionBook };
 
        _bookRepo.Setup(r => r.CreateBook(It.IsAny<Book>())).ReturnsAsync(bookId);
        _producer.Setup(p 
            => p.PublishEventAsync(It.IsAny<BookCreatedEvent>(), CancellationToken.None)).Returns(Task.CompletedTask);
 
        var result = await _sut.CreateBook(book);
 
        Assert.Equal(bookId, result);
        _producer.Verify(p 
            => p.PublishEventAsync(It.IsAny<BookCreatedEvent>(), CancellationToken.None), Times.Once);
    }
 
    [Fact]
    public async Task CreateBook_SetsStatusToAvailable()
    {
        Book? captured = null;
        _bookRepo.Setup(r => r.CreateBook(It.IsAny<Book>()))
            .Callback<Book>(b => captured = b)
            .ReturnsAsync(Guid.NewGuid());
        _producer.Setup(p 
            => p.PublishEventAsync(It.IsAny<BookCreatedEvent>(), CancellationToken.None)).Returns(Task.CompletedTask);
 
        await _sut.CreateBook(new Book { Title = "Тест" });
 
        Assert.Equal(BookStatus.Available, captured!.Status);
    }
 
    [Fact]
    public async Task CreateBook_WhenRepositoryThrows_ThrowsBookServiceException()
    {
        _bookRepo.Setup(r => r.CreateBook(It.IsAny<Book>())).ThrowsAsync(new Exception("DB error"));
 
        await Assert.ThrowsAsync<BookServiceException>(() => _sut.CreateBook(new Book()));
    }
    
    [Fact]
    public async Task UpdateBook_WhenBookIsArchived_ThrowsClientErrorException()
    {
        _bookRepo.Setup(r => r.GetBookById(It.IsAny<Guid>()))
            .ReturnsAsync(new Book { IsArchived = true, Status = BookStatus.Archived });
 
        await Assert.ThrowsAsync<ClientErrorException>(() =>
            _sut.UpdateBook(Guid.NewGuid(), 
                new UpdateBookDto(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<int>())));
    }
 
    [Fact]
    public async Task UpdateBook_WhenValid_UpdatesAndInvalidatesCache()
    {
        var book = new Book
        {
            Title = "Старое", Authors = ["Автор"], IsArchived = false,
            Status = BookStatus.Available, Category = BookCategory.FictionBook
        };
        _bookRepo.Setup(r => r.GetBookById(It.IsAny<Guid>())).ReturnsAsync(book);
        _bookRepo.Setup(r => r.UpdateBook(It.IsAny<Guid>(), It.IsAny<Book>())).Returns(Task.CompletedTask);
        _cache.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
 
        await _sut.UpdateBook(Guid.NewGuid(), new UpdateBookDto("Новое", ["Автор"], 2025));
 
        _bookRepo.Verify(r => r.UpdateBook(It.IsAny<Guid>(), It.IsAny<Book>()), Times.Once);
        _cache.Verify(c => c.RemoveByPrefixAsync(It.IsAny<string>()), Times.AtLeastOnce);
    }
 
    [Fact]
    public async Task UpdateBook_WhenNotFound_ThrowsBookServiceException()
    {
        _bookRepo.Setup(r => r.GetBookById(It.IsAny<Guid>()))
            .ThrowsAsync(new BookNotFoundException("Не найдена"));
 
        await Assert.ThrowsAsync<BookNotFoundException>(() =>
            _sut.UpdateBook(Guid.NewGuid(), 
                new UpdateBookDto(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<int>())));
    }
    
    [Fact]
    public async Task ArchiveBook_WhenValid_ReturnsDto_AndPublishesEvent()
    {
        var bookId = Guid.NewGuid();
        var book = new Book { Title = "Архивная", Status = BookStatus.Available, IsArchived = false };
 
        _bookRepo.Setup(r => r.GetBookById(bookId)).ReturnsAsync(book);
        _bookRepo.Setup(r => r.UpdateBook(bookId, It.IsAny<Book>())).Returns(Task.CompletedTask);
        _cache.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _producer.Setup(p 
            => p.PublishEventAsync(It.IsAny<BookArchivedEvent>(), CancellationToken.None)).Returns(Task.CompletedTask);
 
        var result = await _sut.ArchiveBook(bookId);
 
        Assert.Equal(bookId, result.Id);
        Assert.Equal("Архивная", result.Title);
        _producer.Verify(p 
            => p.PublishEventAsync(It.IsAny<BookArchivedEvent>(), CancellationToken.None), Times.Once);
    }
 
    [Fact]
    public async Task ArchiveBook_WhenBookAlreadyArchived_ThrowsClientErrorException()
    {
        _bookRepo.Setup(r => r.GetBookById(It.IsAny<Guid>()))
            .ReturnsAsync(new Book { IsArchived = true, Status = BookStatus.Archived });
 
        await Assert.ThrowsAsync<ClientErrorException>(() => _sut.ArchiveBook(Guid.NewGuid()));
    }
 
    [Fact]
    public async Task ArchiveBook_WhenBookIsBorrowed_ThrowsClientErrorException()
    {
        _bookRepo.Setup(r => r.GetBookById(It.IsAny<Guid>()))
            .ReturnsAsync(new Book { Status = BookStatus.Borrow, IsArchived = false });
 
        await Assert.ThrowsAsync<ClientErrorException>(() => _sut.ArchiveBook(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetBooks_WhenCacheHit_ReturnsCachedResult_WithoutCallingRepo()
    {
        var cached = new PagedListDto<BookListDto>([], 1, 10, 0);
        _cache.Setup(c 
            => c.TryGetAsync<PagedListDto<BookListDto>>(It.IsAny<string>())).ReturnsAsync(cached);
 
        var result = await _sut.GetBooks(new GetBookListDto(BookStatus.Available, 
            It.IsAny<BookCategory>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));
 
        _bookRepo.Verify(r => r.GetBooks(It.IsAny<BookStatus>(), It.IsAny<BookCategory>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        Assert.Same(cached, result);
    }
 
    [Fact]
    public async Task GetBooks_WhenCacheMiss_QueriesRepo_AndSetsCache()
    {
        var pagedList = new PagedListDto<BookListDto>([], 1, 10, 0);
        _cache.Setup(c 
            => c.TryGetAsync<PagedListDto<BookListDto>>(It.IsAny<string>())).ReturnsAsync((PagedListDto<BookListDto>?)null);
        _bookRepo.Setup(r 
            => r.GetBooks(It.IsAny<BookStatus>(), It.IsAny<BookCategory>(), It.IsAny<string>(), 
                It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(pagedList);
        _cache.Setup(c 
                => c.SetAsync(It.IsAny<string>(), It.IsAny<PagedListDto<BookListDto>>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);
 
        await _sut.GetBooks(new GetBookListDto(BookStatus.Available, It.IsAny<BookCategory>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));
 
        _bookRepo.Verify(r => r.GetBooks(It.IsAny<BookStatus>(), It.IsAny<BookCategory>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        _cache.Verify(c => c.SetAsync(It.IsAny<string>(), pagedList, TimeSpan.FromMinutes(10)), Times.Once);
    }
}