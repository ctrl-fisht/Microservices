using System.Net.Http.Json;
using FileService.Contracts.Dtos;
using FileService.Contracts.Requests;
using FileService.Contracts.Responses;
using FileService.Domain;
using FileService.IntegrationTests.Infrastructure;
using Shared.Kernel;

namespace FileService.IntegrationTests.Features;

public class GetMediaAssetInfoBatchTests : FileServiceTestBase
{
    public GetMediaAssetInfoBatchTests(IntegrationTestsWebFactory factory) : base(factory)
    {
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

    [Fact]
    public async Task GetMediaAssetInfoBatch_Success()
    {
        // arrange
        CancellationToken ct = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, TEST_FILE_FOLDER, TEST_FILENAME));

        // act
        var startMultipartUploadResponse1 = await StartMultipartUploadAsync(fileInfo, ct);
        var startMultipartUploadResponse2 = await StartMultipartUploadAsync(fileInfo, ct);
        var batchInfo = await GetMediaAssetInfoBatchAsync(new List<Guid>(
        [
            startMultipartUploadResponse1.MediaAssetId,
            startMultipartUploadResponse2.MediaAssetId
        ]), ct);
        
        // assert
        Assert.True(batchInfo.Count == 2);
        Assert.True(batchInfo[0].Id == startMultipartUploadResponse1.MediaAssetId);
        Assert.True(batchInfo[1].Id == startMultipartUploadResponse2.MediaAssetId);
    }

    private async Task<List<MediaAssetInfoDto>> GetMediaAssetInfoBatchAsync(List<Guid> mediaAssetIds, CancellationToken ct)
    {
        HttpResponseMessage getBatchResponse = await AppHttpClient
            .PostAsJsonAsync($"/api/files/batch", mediaAssetIds, ct);
        var getBatchResult = await getBatchResponse.ToResult<List<MediaAssetInfoDto>>(ct);
        return getBatchResult.Value;
    }
}