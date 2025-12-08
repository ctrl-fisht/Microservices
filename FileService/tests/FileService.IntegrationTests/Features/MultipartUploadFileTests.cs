using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos;
using FileService.Contracts.Requests;
using FileService.Contracts.Responses;
using FileService.Domain;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Shared.Kernel;
using Shared.Kernel.Errors;

namespace FileService.IntegrationTests.Features;

public class MultipartUploadFileTests : FileServiceTestBase
{
    public MultipartUploadFileTests(IntegrationTestsWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task MultipartUpload_FullUpload_SaveMediaAsset()
    {
        // arrange
        CancellationToken ct = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, TEST_FILE_FOLDER, TEST_FILENAME));

        // act
        var startMultipartUploadResponse = await StartMultipartUploadAsync(fileInfo, ct);
        var partETags = await UploadChunksAsync(fileInfo, startMultipartUploadResponse, ct);
        var completeMultipartUploadResponse =
            await CompleteMultipartUploadAsync(partETags, startMultipartUploadResponse, ct);
        
        // assert
        Assert.True(completeMultipartUploadResponse.MediaAssetId == startMultipartUploadResponse.MediaAssetId);
        var assetUploadedStage = await GetMediaAssetFromDb(completeMultipartUploadResponse.MediaAssetId);
        Assert.True(assetUploadedStage.Status == Status.Uploaded);
    }
    
    private async Task<StartMultipartUploadResponse> StartMultipartUploadAsync(FileInfo fileInfo, CancellationToken ct)
    {
        var request = new StartMultipartUploadRequest
        {
            FileName = fileInfo.Name,
            AssetType = "video",
            ContentType = "video/mp4",
            Size = fileInfo.Length,
            Context = "department",
            ContextId = Guid.NewGuid()
        };
        HttpResponseMessage startUploadResponse = await AppHttpClient.PostAsJsonAsync("/api/files/multipart/start", request, ct);
        var startUploadResult = await startUploadResponse.ToResult<StartMultipartUploadResponse>(ct);
        return startUploadResult.Value;
    }

    private async Task<List<PartETagDto>> UploadChunksAsync(
        FileInfo fileInfo,
        StartMultipartUploadResponse startMultipartUploadResponse,
        CancellationToken ct)
    {
       
        await using var stream = fileInfo.OpenRead();


        var parts = new List<PartETagDto>();
        foreach (ChunkUploadUrlDto chunkUploadUrl in startMultipartUploadResponse.ChunkUrls)
        {
            var chunk = new byte[startMultipartUploadResponse.ChunkSize];
            await stream.ReadExactlyAsync(chunk, 0, startMultipartUploadResponse.ChunkSize, ct);
            var content = new ByteArrayContent(chunk);
            var response = await HttpClient.PutAsync(chunkUploadUrl.Value, content, ct);
            var etag = response.Headers.ETag!.Tag.Trim('"');
            parts.Add(new PartETagDto(chunkUploadUrl.PartNumber, etag));
        }
        return parts;
    }

    private async Task<CompleteMultipartUploadResponse> CompleteMultipartUploadAsync(
        List<PartETagDto> partETags,
        StartMultipartUploadResponse startMultipartUploadResponse,
        CancellationToken ct)
    {
        var request = new CompleteMultipartUploadRequest()
        {
            MediaAssetId = startMultipartUploadResponse.MediaAssetId,
            PartETags = partETags,
            UploadId = startMultipartUploadResponse.UploadId
        };
        var completeUploadResponse = await AppHttpClient
            .PostAsJsonAsync("/api/files/multipart/complete", request, ct);
        var completeUploadResult = await completeUploadResponse.ToResult<CompleteMultipartUploadResponse>(ct);
        return completeUploadResult.IsSuccess 
            ? completeUploadResult.Value 
            : throw new Exception($"Failed to complete upload: Errors = {completeUploadResult.Error}");
    }
}


public static class HttpResponseExtensions
{
    public static async Task<Result<TResponse, Errors>> ToResult<TResponse>(
        this HttpResponseMessage response, 
        CancellationToken cancellationToken = default)
    where TResponse : class
    {
        Envelope<TResponse>? envelope = await response.Content.ReadFromJsonAsync<Envelope<TResponse>>(cancellationToken);
        if (envelope is null) throw new BadHttpRequestException("Response is not envelope");

        Result<TResponse, Errors> result;
        if (envelope.Errors is null && envelope.Result is not null)
            return envelope.Result;
        if (envelope.Errors is not null)
            return new Errors(envelope.Errors);
        throw new ArgumentOutOfRangeException("Envelope invariant");
    }
}