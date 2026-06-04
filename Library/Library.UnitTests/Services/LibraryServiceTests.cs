using FluentAssertions;
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

public class LibraryServiceTests
{
    private readonly Mock<ILibraryRepository> _libraryRepo = new();
    private readonly Mock<ICacheStorage> _cache = new();
    private readonly Mock<IFileStorage> _fileStorage = new();
    private readonly Mock<ILibraryEventProducer> _producer = new();
    private readonly LibraryService _sut;
 
    public LibraryServiceTests()
    {
        _sut = new LibraryService(_libraryRepo.Object, _cache.Object, _fileStorage.Object, _producer.Object);
    }

    [Fact]
    public async Task BorrowBook_WhenBookAvailable_CreatesBorrowAndPublishesEvent()
    {
        var bookId = Guid.NewGuid();
        var readerId = Guid.NewGuid();
        var borrowId = Guid.NewGuid();
        var book = new Book { Title = "Книга", Status = BookStatus.Available, IsArchived = false };
 
        _libraryRepo.Setup(r => r.GetBookById(bookId)).ReturnsAsync(book);
        _libraryRepo.Setup(r => r.CreateBorrow(It.IsAny<Borrow>())).ReturnsAsync(borrowId);
        _cache.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _producer.Setup(p 
            => p.PublishEventAsync(It.IsAny<BookBorrowedEvent>(), CancellationToken.None)).Returns(Task.CompletedTask);
 
        var result = await _sut.BorrowBook(bookId, readerId);
 
        Assert.Equal(borrowId, result);
        _producer.Verify(p 
            => p.PublishEventAsync(It.IsAny<BookBorrowedEvent>(), CancellationToken.None), Times.Once);
        _cache.Verify(c => c.RemoveAsync($"reader:books:{readerId}"), Times.Once);
    }
 
    [Fact]
    public async Task BorrowBook_WhenBookNotAvailable_ThrowsClientErrorException()
    {
        _libraryRepo.Setup(r => r.GetBookById(It.IsAny<Guid>()))
            .ReturnsAsync(new Book { Status = BookStatus.Borrow, IsArchived = false });
 
        await Assert.ThrowsAsync<ClientErrorException>(() =>
            _sut.BorrowBook(Guid.NewGuid(), Guid.NewGuid()));
    }
 
    [Fact]
    public async Task BorrowBook_WhenBookArchived_ThrowsClientErrorException()
    {
        _libraryRepo.Setup(r => r.GetBookById(It.IsAny<Guid>()))
            .ReturnsAsync(new Book { Status = BookStatus.Available, IsArchived = true });
 
        await Assert.ThrowsAsync<ClientErrorException>(() =>
            _sut.BorrowBook(Guid.NewGuid(), Guid.NewGuid()));
    }
 
    [Fact]
    public async Task BorrowBook_WhenBookNotFound_ThrowsBookNotFoundException()
    {
        _libraryRepo.Setup(r => r.GetBookById(It.IsAny<Guid>()))
            .ThrowsAsync(new BookNotFoundException("Не найдена"));
 
        await Assert.ThrowsAsync<BookNotFoundException>(() =>
            _sut.BorrowBook(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task ReturnBook_WhenSuccess_UpdatesBorrowAndPublishesEvent()
    {
        var bookId = Guid.NewGuid();
        var borrow = new Borrow
        {
            BookId = bookId, ReaderId = Guid.NewGuid(),
            BorrowDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            Status = BookIssueStatus.Issued
        };
 
        _libraryRepo.Setup(r => r.GetBorrowByBookId(bookId)).ReturnsAsync(borrow);
        _libraryRepo.Setup(r => r.UpdateBorrow(It.IsAny<Borrow>())).Returns(Task.CompletedTask);
        _cache.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _producer.Setup(p 
            => p.PublishEventAsync(It.IsAny<BookReturnedEvent>(), CancellationToken.None)).Returns(Task.CompletedTask);
 
        await _sut.ReturnBook(bookId);
 
        _libraryRepo.Verify(r => r.UpdateBorrow(It.IsAny<Borrow>()), Times.Once);
        _producer.Verify(p 
            => p.PublishEventAsync(It.IsAny<BookReturnedEvent>(), CancellationToken.None), Times.Once);
        _cache.Verify(c => c.RemoveAsync($"reader:books:{borrow.ReaderId}"), Times.Once);
    }
 
    [Fact]
    public async Task ReturnBook_WhenBorrowNotFound_ThrowsBorrowNotFoundException()
    {
        _libraryRepo.Setup(r => r.GetBorrowByBookId(It.IsAny<Guid>()))
            .ThrowsAsync(new BorrowNotFoundException("Не найдена"));
 
        await Assert.ThrowsAsync<BorrowNotFoundException>(() => _sut.ReturnBook(Guid.NewGuid()));
    }
 
    [Fact]
    public async Task GetLibraryBooks_WhenCacheHit_ReturnsCachedData()
    {
        var cached = new PagedListDto<LibraryBookDto>([], 1, 10, 0);
        _cache.Setup(c => c.TryGetAsync<PagedListDto<LibraryBookDto>>(It.IsAny<string>()))
            .ReturnsAsync(cached);
 
        var result = await _sut.GetLibraryBooks(
            new GetLibraryBooksDto(It.IsAny<BookCategory>(), It.IsAny<string>(), 
                It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()));
 
        _libraryRepo.Verify(r => r.GetLibraryBooks(It.IsAny<BookCategory>(), It.IsAny<string>(),
            It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        Assert.Same(cached, result);
    }
 
    [Fact]
    public async Task GetLibraryBooks_WhenCacheMiss_QueriesRepoAndCaches()
    {
        var pagedList = new PagedListDto<LibraryBookDto>([], 1, 10, 0);
        _cache.Setup(c => c.TryGetAsync<PagedListDto<LibraryBookDto>>(It.IsAny<string>()))
            .ReturnsAsync((PagedListDto<LibraryBookDto>?)null);
        _libraryRepo.Setup(r => r.GetLibraryBooks(It.IsAny<BookCategory>(), It.IsAny<string>(),
            It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(pagedList);
        _cache.Setup(c 
                => c.SetAsync(It.IsAny<string>(), It.IsAny<PagedListDto<LibraryBookDto>>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);
 
        await _sut.GetLibraryBooks(new GetLibraryBooksDto(It.IsAny<BookCategory>(), It.IsAny<string>(),
            It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()));
 
        _libraryRepo.Verify(r => r.GetLibraryBooks(It.IsAny<BookCategory>(), It.IsAny<string>(),
            It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        _cache.Verify(c 
            => c.SetAsync(It.IsAny<string>(), pagedList, TimeSpan.FromMinutes(5)), Times.Once);
    }
    
    [Fact]
    public async Task GetBookDetailsById_WhenBookArchived_ThrowsClientErrorException()
    {
        _cache.Setup(c 
            => c.TryGetAsync<BookDetailsDto>(It.IsAny<string>())).ReturnsAsync((BookDetailsDto?)null);
        _libraryRepo.Setup(r => r.GetBookById(It.IsAny<Guid>()))
            .ReturnsAsync(new Book { IsArchived = true });
 
        await Assert.ThrowsAsync<ClientErrorException>(() => _sut.GetBookDetailsById(Guid.NewGuid()));
    }
 
    [Fact]
    public async Task GetBookDetailsById_WhenCacheHit_DoesNotQueryRepo()
    {
        var bookId = Guid.NewGuid();
        var cached = new BookDetailsDto(bookId, "Книга", [], "", 2020, BookCategory.Default, 
            BookStatus.Available, "", false);
        _cache.Setup(c => c.TryGetAsync<BookDetailsDto>($"book:details:{bookId}")).ReturnsAsync(cached);
 
        var result = await _sut.GetBookDetailsById(bookId);
 
        _libraryRepo.Verify(r => r.GetBookById(It.IsAny<Guid>()), Times.Never);
        Assert.Same(cached, result);
    }
}
