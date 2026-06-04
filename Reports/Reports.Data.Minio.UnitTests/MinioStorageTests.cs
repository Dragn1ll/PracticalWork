using Microsoft.Extensions.Options;

namespace Reports.Data.Minio.UnitTests;

public class MinioStorageTests
{
    private static MinioStorage Build() =>
        new(Options.Create(new MinioOptions
        {
            Endpoint = "localhost:9000",
            AccessKey = "minioadmin",
            SecretKey = "minioadmin",
            BucketName = "test-bucket"
        }));
 
    [Fact]
    public void Constructor_DoesNotThrow()
    {
        var ex = Record.Exception(Build);
        
        Assert.Null(ex);
    }
 
    [Fact]
    public async Task UploadFileAsync_WhenMinioUnavailable_ThrowsAnyException()
    {
        var sut = Build();
        
        await Assert.ThrowsAnyAsync<Exception>(() =>
            sut.UploadFileAsync("file.csv", new MemoryStream([1, 2, 3]), "text/csv"));
    }
 
    [Fact]
    public async Task GetFileUrlAsync_WhenMinioUnavailable_ThrowsAnyException()
    {
        var sut = Build();
        
        await Assert.ThrowsAnyAsync<Exception>(() =>
            sut.GetFileUrlAsync("file.csv"));
    }
}
