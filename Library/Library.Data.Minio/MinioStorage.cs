using Library.Abstraction.Storage;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Library.Data.Minio;

/// <inheritdoc cref="IFileStorage"/>
public class MinioStorage : IFileStorage
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;

    public MinioStorage(IOptions<MinioOptions> minioOptions)
    {
        _minioClient = new MinioClient()
            .WithEndpoint(minioOptions.Value.Endpoint)
            .WithCredentials(minioOptions.Value.AccessKey, minioOptions.Value.SecretKey)
            .Build();
        _bucketName = minioOptions.Value.BucketName;
    }

    /// <inheritdoc cref="IFileStorage.UploadFileAsync"/>
    public async Task UploadFileAsync(string fileName, Stream fileStream, string extension)
    {
        await CheckBucketAsync();
        
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Пустое название файла!");
        }

        if (fileStream == null)
        {
            throw new ArgumentNullException(nameof(fileStream));
        }

        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException("Пустое название типа файла!");
        }
        
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(fileName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(extension);
        
        await _minioClient.PutObjectAsync(putObjectArgs);
    }

    /// <inheritdoc cref="IFileStorage.GetFileUrlAsync"/>
    public async Task<string> GetFileUrlAsync(string fileName, int expiryMinutes = 60, string? bucketName = null)
    {
        await CheckBucketAsync();
        
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Пустое название файла!");
        }
        
        var presignedGetArgs = new PresignedGetObjectArgs()
            .WithBucket(bucketName ?? _bucketName)
            .WithObject(fileName)
            .WithExpiry(expiryMinutes * 60);
        
        return await _minioClient.PresignedGetObjectAsync(presignedGetArgs);
    }

    private async Task CheckBucketAsync()
    {
        var bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName));
        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
        }
    }
}