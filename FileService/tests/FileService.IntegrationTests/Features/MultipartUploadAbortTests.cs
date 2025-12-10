using System.Net.Http.Json;
using FileService.Contracts.Dtos;
using FileService.Contracts.Requests;
using FileService.Contracts.Responses;
using FileService.Domain;
using FileService.IntegrationTests.Infrastructure;
using Shared.Kernel;

namespace FileService.IntegrationTests.Features;

public class MultipartUploadAbortTests : FileServiceTestBase
{
    public MultipartUploadAbortTests(IntegrationTestsWebFactory factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task MultipartUpload_UploadAbort_Aborted()
    {
        // arrange
        CancellationToken ct = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, TEST_FILE_FOLDER, TEST_FILENAME));

        // act
        var startMultipartUploadResponse = await StartMultipartUploadAsync(fileInfo, ct);
        var abortResponse = await AbortMultipartUploadAsync(startMultipartUploadResponse, ct);
        // assert
        Assert.True(abortResponse.Success);
        var assetAbortedStage = await GetMediaAssetFromDb(startMultipartUploadResponse.MediaAssetId);
        Assert.True(assetAbortedStage.Status == Status.Deleted);
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

    private async Task<AbortMultipartUploadResponse> AbortMultipartUploadAsync(
        StartMultipartUploadResponse startResponse, CancellationToken ct)
    {
        var request = new AbortMultipartUploadRequest
        {
            MediaAssetId = startResponse.MediaAssetId,
            UploadId = startResponse.UploadId
        };
        HttpResponseMessage abortUploadResponse = await AppHttpClient
            .PostAsJsonAsync("/api/files/multipart/abort", request, ct);
        var abortUploadResult = await abortUploadResponse.ToResult<AbortMultipartUploadResponse>(ct);
        return abortUploadResult.Value;
    }
}