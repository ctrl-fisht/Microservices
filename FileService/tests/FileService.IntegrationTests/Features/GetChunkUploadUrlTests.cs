using System.Net.Http.Json;
using FileService.Contracts.Requests;
using FileService.Contracts.Responses;
using FileService.Domain;
using FileService.IntegrationTests.Infrastructure;
using Shared.Kernel;

namespace FileService.IntegrationTests.Features;

public class GetChunkUploadUrlTests : FileServiceTestBase
{
    public GetChunkUploadUrlTests(IntegrationTestsWebFactory factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task MultipartUpload_GetChunkUploadUrl_Success()
    {
        // arrange
        CancellationToken ct = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, TEST_FILE_FOLDER, TEST_FILENAME));

        // act
        var startMultipartUploadResponse = await StartMultipartUploadAsync(fileInfo, ct);
        var getChunkUrlResponse = await GetChunkUploadUrlAsync(startMultipartUploadResponse, ct);
        // assert
        Assert.True(getChunkUrlResponse.UploadUrl is not null);
        var assetLoadingStage = await GetMediaAssetFromDb(startMultipartUploadResponse.MediaAssetId);
        Assert.True(assetLoadingStage.Status == Status.Loading);
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
    
    private async Task<GetChunkUploadUrlResponse> GetChunkUploadUrlAsync(
        StartMultipartUploadResponse startResponse, CancellationToken ct)
    {
        var request = new GetChunkUploadUrlRequest()
        {
            MediaAssetId = startResponse.MediaAssetId,
            UploadId = startResponse.UploadId,
            PartNumber = 1
        };
        HttpResponseMessage getChunkUrlResponse = await AppHttpClient
            .PostAsJsonAsync("/api/files/multipart/url", request, ct);
        var getChunkUrlResult = await getChunkUrlResponse.ToResult<GetChunkUploadUrlResponse>(ct);
        return getChunkUrlResult.Value;
    }
    
}

