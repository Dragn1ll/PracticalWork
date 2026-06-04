using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace Reports.Cache.Redis.UnitTests;

public class RedisCacheTests
{
    private readonly Mock<IDistributedCache> _distributedCache = new();
    private readonly RedisCache _sut;
 
    public RedisCacheTests()
    {
        _sut = new RedisCache(_distributedCache.Object);
    }
 
    [Fact]
    public async Task TryGetAsync_WhenKeyExists_ReturnsDeserializedValue()
    {
        var payload = JsonSerializer.Serialize(42);
        _distributedCache.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(payload));
 
        var result = await _sut.TryGetAsync<int>("key");
 
        Assert.Equal(42, result);
    }
 
    [Fact]
    public async Task TryGetAsync_WhenKeyMissing_ReturnsDefault()
    {
        _distributedCache.Setup(c => c.GetAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
 
        var result = await _sut.TryGetAsync<string>("missing");
 
        Assert.Null(result);
    }
 
    [Fact]
    public async Task SetAsync_CallsDistributedCacheSetString()
    {
        _distributedCache.Setup(c => c.SetAsync(
            "k", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
 
        await _sut.SetAsync("k", "value", TimeSpan.FromMinutes(5));
 
        _distributedCache.Verify(c => c.SetAsync(
            "k", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
 
    [Fact]
    public async Task SetAsync_StoresKeyForPrefixRemoval()
    {
        _distributedCache.Setup(c => c.SetAsync(
            It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _distributedCache.Setup(c 
                => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
 
        await _sut.SetAsync("books:list:1", "v1", TimeSpan.FromMinutes(5));
        await _sut.SetAsync("books:list:2", "v2", TimeSpan.FromMinutes(5));
        await _sut.RemoveByPrefixAsync("books:list:");
 
        _distributedCache.Verify(c 
            => c.RemoveAsync("books:list:1", It.IsAny<CancellationToken>()), Times.Once);
        _distributedCache.Verify(c 
            => c.RemoveAsync("books:list:2", It.IsAny<CancellationToken>()), Times.Once);
    }
 
    [Fact]
    public async Task RemoveAsync_RemovesFromCacheAndInternalList()
    {
        _distributedCache.Setup(c => c.SetAsync(
            It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _distributedCache.Setup(c 
                => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
 
        await _sut.SetAsync("key1", "val", TimeSpan.FromMinutes(1));
        await _sut.RemoveAsync("key1");
 
        await _sut.RemoveByPrefixAsync("key");
 
        _distributedCache.Verify(c 
            => c.RemoveAsync("key1", It.IsAny<CancellationToken>()), Times.Once);
    }
 
    [Fact]
    public async Task RemoveByPrefixAsync_OnlyRemovesMatchingKeys()
    {
        _distributedCache.Setup(c => c.SetAsync(
            It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _distributedCache.Setup(c 
                => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
 
        await _sut.SetAsync("books:1", "v", TimeSpan.FromMinutes(1));
        await _sut.SetAsync("readers:1", "v", TimeSpan.FromMinutes(1));
 
        await _sut.RemoveByPrefixAsync("books:");
 
        _distributedCache.Verify(c 
            => c.RemoveAsync("books:1", It.IsAny<CancellationToken>()), Times.Once);
        _distributedCache.Verify(c 
            => c.RemoveAsync("readers:1", It.IsAny<CancellationToken>()), Times.Never);
    }
 
    [Fact]
    public async Task RemoveByPrefixAsync_WhenNoMatchingKeys_DoesNotCallRemove()
    {
        await _sut.RemoveByPrefixAsync("nonexistent:");
 
        _distributedCache.Verify(
            c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
