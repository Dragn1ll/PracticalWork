using Library.Abstraction.Services;
using Library.Abstraction.Storage.Repositories;
using Library.BackgroundServices.Email.Services;
using Library.Dto.Output;
using Library.SharedKernel.Events.Book;
using Microsoft.Extensions.Logging;
using Moq;

namespace Library.BackgroundServices.Email.UnitTests;

public class ArchiveServiceTests
{
    private readonly Mock<IBookRepository> _bookRepo = new();
    private readonly Mock<IBookService> _bookService = new();
    private readonly Mock<ILibraryEventProducer> _producer = new();
    private readonly Mock<ILogger<ArchiveService>> _logger = new();
 
    private ArchiveService Build(DateTimeOffset now) =>
        new(_bookRepo.Object, _bookService.Object, _producer.Object,
            new FakeTimeProvider(now), _logger.Object);
 
    [Fact]
    public async Task ArchiveOldBooks_WhenNoBooks_ReturnsZeroCounts()
    {
        _bookRepo.Setup(r => r.GetAvailableOldBooks(It.IsAny<DateOnly>(), 1, 10))
            .ReturnsAsync(new List<AvailableOldBookDto>());
 
        var result = await Build(DateTimeOffset.UtcNow).ArchiveOldBooks(3, 10);
 
        Assert.Equal(0, result.TotalProcessed);
        Assert.Equal(0, result.ArchivedCount);
        Assert.Equal(0, result.SkippedCount);
    }
 
    [Fact]
    public async Task ArchiveOldBooks_WhenAllSucceed_CorrectlyCountsArchived()
    {
        var books = new List<AvailableOldBookDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Книга 1" },
            new() { Id = Guid.NewGuid(), Title = "Книга 2" }
        };
 
        _bookRepo.Setup(r => r.GetAvailableOldBooks(It.IsAny<DateOnly>(), 1, 100))
            .ReturnsAsync(books);
        _bookService.Setup(s => s.ArchiveBook(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new ArchiveBookDto(id, "Книга"));
        _producer.Setup(p 
            => p.PublishEventAsync(It.IsAny<BookArchivedEvent>(), CancellationToken.None)).Returns(Task.CompletedTask);
 
        var result = await Build(DateTimeOffset.UtcNow).ArchiveOldBooks(3, 100);
 
        Assert.Equal(2, result.TotalProcessed);
        Assert.Equal(2, result.ArchivedCount);
        Assert.Equal(0, result.SkippedCount);
        _producer.Verify(p 
            => p.PublishEventAsync(It.IsAny<BookArchivedEvent>(), CancellationToken.None), Times.Exactly(2));
    }
 
    [Fact]
    public async Task ArchiveOldBooks_WhenBookFails_CountedAsSkipped_ContinuesProcessing()
    {
        var goodId = Guid.NewGuid();
        var badId = Guid.NewGuid();
 
        _bookRepo.Setup(r => r.GetAvailableOldBooks(It.IsAny<DateOnly>(), 1, 100))
            .ReturnsAsync(new List<AvailableOldBookDto>
            {
                new() { Id = goodId, Title = "Хорошая" },
                new() { Id = badId, Title = "Плохая" }
            });
        _bookService.Setup(s => s.ArchiveBook(goodId)).ReturnsAsync(new ArchiveBookDto(goodId, "Хорошая"));
        _bookService.Setup(s => s.ArchiveBook(badId)).ThrowsAsync(new Exception("Ошибка архивации"));
        _producer.Setup(p 
            => p.PublishEventAsync(It.IsAny<BookArchivedEvent>(), CancellationToken.None)).Returns(Task.CompletedTask);
 
        var result = await Build(DateTimeOffset.UtcNow).ArchiveOldBooks(3, 100);
 
        Assert.Equal(2, result.TotalProcessed);
        Assert.Equal(1, result.ArchivedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Contains("Ошибка архивации", result.SkipReasons);
    }
 
    [Fact]
    public async Task ArchiveOldBooks_CutoffDateCalculatedCorrectly()
    {
        var now = new DateTimeOffset(2025, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var expectedCutoff = new DateOnly(2022, 6, 15);
 
        _bookRepo.Setup(r => r.GetAvailableOldBooks(expectedCutoff, 1, 50))
            .ReturnsAsync(new List<AvailableOldBookDto>())
            .Verifiable();
 
        await Build(now).ArchiveOldBooks(3, 50);
 
        _bookRepo.Verify(r => r.GetAvailableOldBooks(expectedCutoff, 1, 50), Times.Once);
    }
 
    [Fact]
    public async Task ArchiveOldBooks_ReturnsNonNegativeExecutionTime()
    {
        _bookRepo.Setup(r => r.GetAvailableOldBooks(It.IsAny<DateOnly>(), 1, 100))
            .ReturnsAsync(new List<AvailableOldBookDto>());
 
        var result = await Build(DateTimeOffset.UtcNow).ArchiveOldBooks(3, 100);
 
        Assert.True(result.ExecutionTime >= TimeSpan.Zero);
    }
}