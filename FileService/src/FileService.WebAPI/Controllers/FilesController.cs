using FileService.Contracts.Dtos;
using FileService.Contracts.Requests;
using FileService.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Results;
using Features = FileService.Application.Features;
namespace FileService.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class FilesController() : ControllerBase
{
    [HttpPost]
    [Route("multipart/start")]
    public async Task<EndpointResult<StartMultipartUploadResponse>> StartMultipartUpload(
        [FromBody] StartMultipartUploadRequest request,
        [FromServices] Features.StartMultipartUpload.Handler handler,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(request, cancellationToken);
    }

    [HttpPost]
    [Route("multipart/url")]
    public async Task<EndpointResult<GetChunkUploadUrlResponse>> GetChunkUploadUrl(
        [FromBody] GetChunkUploadUrlRequest request,
        [FromServices] Features.GetChunkUploadUrl.Handler handler,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(request, cancellationToken);
    }
    
    [HttpPost]
    [Route("multipart/complete")]
    public async Task<EndpointResult<CompleteMultipartUploadResponse>> CompleteMultipartUpload(
        [FromBody] CompleteMultipartUploadRequest request,
        [FromServices] Features.CompleteMultipartUpload.Handler handler,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(request, cancellationToken);
    }
    
    [HttpPost]
    [Route("multipart/abort")]
    public async Task<EndpointResult<AbortMultipartUploadResponse>> GetChunkUploadUrl(
        [FromBody] AbortMultipartUploadRequest request,
        [FromServices] Features.AbortMultipartUpload.Handler handler,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(request, cancellationToken);
    }

    [HttpGet]
    [Route("{mediaAssetId:guid}")]
    public async Task<EndpointResult<MediaAssetInfoDto>> GetMediaAssetInfo(
        [FromRoute] Guid mediaAssetId,
        [FromServices] Features.GetMediaAssetInfo.Handler handler,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(mediaAssetId, cancellationToken);
    }
    
    [HttpPost]
    [Route("batch")]
    public async Task<EndpointResult<List<MediaAssetInfoDto>>> GetMediaAssetInfoBatch(
        [FromBody] List<Guid> mediaAssetIds,
        [FromServices] Features.GetMediaAssetsInfo.Handler handler,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(mediaAssetIds, cancellationToken);
    }
    
    [HttpGet]
    [Route("{mediaAssetId:guid}/url")]
    public async Task<EndpointResult<string>> GetMediaAssetDownloadUrl(
        [FromRoute]  Guid mediaAssetId,
        [FromServices] Features.GetMediaAssetDownloadUrl.Handler handler,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(mediaAssetId, cancellationToken);
    }
    
    [HttpDelete]
    [Route("{mediaAssetId:guid}")]
    public async Task<EndpointResult<Guid>> DeleteMediaAsset(
        [FromRoute]  Guid mediaAssetId,
        [FromServices] Features.DeleteMediaAsset.Handler handler,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(mediaAssetId, cancellationToken);
    }
}