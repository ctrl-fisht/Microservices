using System.Net.Http.Json;
using FileService.Contracts.Dtos;
using FileService.Contracts.Requests;
using FileService.Contracts.Responses;
using FileService.Domain;
using FileService.IntegrationTests.Infrastructure;

namespace FileService.IntegrationTests.Features;

public class GetMediaAssetInfoTests : FileServiceTestBase
{
    public GetMediaAssetInfoTests(IntegrationTestsWebFactory factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task GetMediaAssetInfo_LoadingStage_Success()
    {
        // arrange
        CancellationToken ct = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, TEST_FILE_FOLDER, TEST_FILENAME));

        // act
        var startMultipartUploadResponse = await StartMultipartUploadAsync(fileInfo, ct);
        var mediaAssetInfo = await GetMediaAssetInfoAsync(startMultipartUploadResponse.MediaAssetId, ct);
        
        // assert
        var assetLoadingStage = await GetMediaAssetFromDb(startMultipartUploadResponse.MediaAssetId);
        Assert.True(assetLoadingStage.Status == Status.Loading);
        Assert.True(mediaAssetInfo.Id == assetLoadingStage.Id);
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

    private async Task<MediaAssetInfoDto> GetMediaAssetInfoAsync(Guid mediaAssetId, CancellationToken ct)
    {
        HttpResponseMessage getMediaAssetInfoResponse = await AppHttpClient
            .GetAsync($"/api/files/{mediaAssetId}", ct);
        var getMediaAssetInfoResult = await getMediaAssetInfoResponse.ToResult<MediaAssetInfoDto>(ct);
        return getMediaAssetInfoResult.Value;
    }
    
}