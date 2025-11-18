using System.Diagnostics.Tracing;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Model.Internal.MarshallTransformations;
using Amazon.S3.Util;
using Microsoft.Extensions.Logging;

namespace FileService.Infrastructure.S3.S3BucketInitializer;

public class S3BucketInitializer
{
    private readonly IAmazonS3 _client;
    private readonly S3Options _options;
    private ILogger<S3BucketInitializer> _logger;
    public S3BucketInitializer(IAmazonS3 client, S3Options options, ILogger<S3BucketInitializer> logger)
    {
        _options = options;
        _client = client;
        _logger = logger;
    }

    private async Task<bool> CreateBucketIfNotExists(string bucket)
    {
        try
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_client, bucket);
            if (bucketExists)
            {
                _logger.LogInformation("[{BucketName}] Уже существует", bucket);
                return false;
            }
            _logger.LogInformation("[{BucketName}] Отсутсвует, создаём...", bucket);
            
            await _client.PutBucketAsync(bucket);
            _logger.LogInformation("[{BucketName}] Создан", bucket);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Возникла ошибка: {Exception}", ex.Message);
            throw;
        }
    }

    private async Task PutPublicPolicy(string bucket)
    {
        try
        {
            _logger.LogInformation("[{BucketName}] Устанавливаем Public Policy", bucket);
            await _client.PutBucketPolicyAsync(new PutBucketPolicyRequest()
            {
                BucketName = bucket,
                Policy = S3Options.PublicPolicy(bucket)
            });
            _logger.LogInformation("[{BucketName}] Установили Public Policy", bucket);
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Возникла ошибка: {Exception}", ex.Message);
        }
    }

    // если бакет был создан сервисом для него ставится public policy
    // а если бакет уже существовал, то public policy будет выставлен
    // только в случае если есть флаг OverwriteExistingPolicy
    public async Task Initialize()
    {
        using (_logger.BeginScope("InitId:{InitId}", Guid.NewGuid()))
        {
            var requiredBuckets = _options.RequiredBuckets;
            _logger.LogInformation("Проверяем существование бакетов: {RequiredBuckets}", string.Join(", ", requiredBuckets));
        
            foreach (var bucket in requiredBuckets)
            {
                var justCreated = await CreateBucketIfNotExists(bucket);
                if (justCreated)
                    await PutPublicPolicy(bucket);
                
                else if (_options.OverwriteExistingPolicies)
                {
                    _logger.LogInformation("[{BucketName}] Установлен флаг перезаписи", bucket);
                    await PutPublicPolicy(bucket);
                }
            }
        }
    }
}
