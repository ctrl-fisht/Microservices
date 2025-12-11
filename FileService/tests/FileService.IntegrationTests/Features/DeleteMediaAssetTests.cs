using System.Net.Http.Json;
using FileService.Contracts.Requests;
using FileService.Contracts.Responses;
using FileService.Domain;
using FileService.IntegrationTests.Infrastructure;
using Shared.Kernel;

namespace FileService.IntegrationTests.Features;

public class DeleteMediaAssetTests : FileServiceTestBase
{
    public DeleteMediaAssetTests(IntegrationTestsWebFactory factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task DeleteMediaAsset_Should_Success()
    {
        // arrange
        CancellationToken ct = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, TEST_FILE_FOLDER, TEST_FILENAME));

        // act
        var startMultipartUploadResponse = await StartMultipartUploadAsync(fileInfo, ct);
        var deleteResponse = await DeleteMediaAssetAsync(startMultipartUploadResponse.MediaAssetId, ct);
        // assert
        Assert.True(deleteResponse == startMultipartUploadResponse.MediaAssetId);
        var assetDeletedStatus = await GetMediaAssetFromDb(startMultipartUploadResponse.MediaAssetId);
        Assert.True(assetDeletedStatus.Status == Status.Deleted);
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

    private async Task<Guid> DeleteMediaAssetAsync(
        Guid mediaAssetId, CancellationToken ct)
    {
        HttpResponseMessage deleteResponse = await AppHttpClient
            .DeleteAsync($"/api/files/{mediaAssetId}", ct);
        var deleteResult = await deleteResponse.ToResult<Guid>(ct);
        return deleteResult.Value;
    }
}