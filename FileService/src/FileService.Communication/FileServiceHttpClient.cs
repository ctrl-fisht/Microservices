using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos;
using FileService.Contracts.Requests;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Errors;
using Shared.Kernel;

namespace FileService.Communication;

public class FileServiceHttpClient : IFileService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FileServiceHttpClient> _logger;

    private async Task<Result<T, Errors>> SendAsync<T>(Func<Task<HttpResponseMessage>> httpCall,
        CancellationToken ct)
    {
        try
        {
            using HttpResponseMessage response = await httpCall();
            return await response.ToResult<T>(ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected FileService error");
            return new Errors(Error.Failure("unexpected.fs.error", "Unexpected FileService error"));
        }
    }
    
    public FileServiceHttpClient(HttpClient httpClient, ILogger<FileServiceHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public Task<Result<MediaAssetInfoDto, Errors>> GetMediaAssetInfo(Guid mediaAssetId, CancellationToken ct)
    {
        return SendAsync<MediaAssetInfoDto>(
            async () => await _httpClient.GetAsync($"/api/files/{mediaAssetId}", ct), ct);
    }

    public Task<Result<List<MediaAssetInfoDto>, Errors>> GetMediaAssetsInfo(GetMediaAssetsInfoRequest request, CancellationToken ct)
    {
        return SendAsync<List<MediaAssetInfoDto>>(
            async () => await _httpClient.PostAsJsonAsync($"/api/files/batch", request, ct), ct);
    }
}