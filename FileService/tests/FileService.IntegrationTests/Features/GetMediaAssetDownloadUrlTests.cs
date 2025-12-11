using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using FileService.Contracts.Requests;
using FileService.Contracts.Responses;
using FileService.Domain;
using FileService.IntegrationTests.Infrastructure;
using Shared.Kernel;
using Shared.Kernel.Errors;

namespace FileService.IntegrationTests.Features;

public class GetMediaAssetDownloadUrlTests : FileServiceTestBase
{
    public GetMediaAssetDownloadUrlTests(IntegrationTestsWebFactory factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task GetMediaAssetDownloadUrl_FileNotReady()
    {
        // arrange
        CancellationToken ct = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, TEST_FILE_FOLDER, TEST_FILENAME));

        // act
        var startMultipartUploadResponse = await StartMultipartUploadAsync(fileInfo, ct);
        var getResponse = await GetMediaAssetDownloadUrlResultAsync(startMultipartUploadResponse.MediaAssetId, ct);
        // assert
        Assert.True(getResponse.IsFailure);
        var asset = await GetMediaAssetFromDb(startMultipartUploadResponse.MediaAssetId);
        Assert.True(asset.Status == Status.Loading);
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
    
    private async Task<Result<string, Errors>> GetMediaAssetDownloadUrlResultAsync(Guid mediaAssetId, CancellationToken ct)
    {
        HttpResponseMessage getMediaAssetInfoResponse = await AppHttpClient
            .GetAsync($"/api/files/{mediaAssetId}/url", ct);
        var getMediaAssetInfoResult = await getMediaAssetInfoResponse.ToResult<string>(ct);
        return getMediaAssetInfoResult;
    }
}