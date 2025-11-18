using FileService.Application.Abstractions;
using FileService.Infrastructure.Postgres;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Framework.Results;
using Features = FileService.Application.Features;
namespace FileService.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController() : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> UploadFileAsync(
        [FromServices] IS3Provider s3Client,
        [FromServices] FileServiceDbContext dbContext,
        // [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        var asset = await dbContext.MediaAssets.FirstAsync(cancellationToken);

        var result = await s3Client.DownloadFileAsync(asset.RawKey, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error.Message);

        var stream = result.Value;
        var contentType = asset.MediaData.ContentType.MimeType ?? "application/octet-stream";
        var fileName = asset.MediaData.FileName.Full ?? "file.bin";

        return File(stream, contentType, fileName);
    }
}