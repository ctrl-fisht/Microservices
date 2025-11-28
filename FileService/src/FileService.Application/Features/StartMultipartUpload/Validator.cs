using FileService.Contracts.Requests;
using FileService.Domain.Entities;
using FluentValidation;
using Shared.Kernel.Errors;

namespace FileService.Application.Features.StartMultipartUpload;

public class Validator : AbstractValidator<StartMultipartUploadRequest>
{
    private static readonly string[] AllowedAssetTypes = ["video", "preview"];
    private static readonly string[] AllowedContexts = ["lesson", "course", "user", "department"];

    public Validator()
    {
        RuleFor(r => r.FileName)
            .NotEmpty()
                .WithMessage(AppErrors.Validation.CannotBeEmpty("filename").Serialize());

        RuleFor(r => r.AssetType)
            .NotEmpty()
                .WithMessage(AppErrors.Validation.CannotBeEmpty("asset_type").Serialize())
            .Must(asset => AllowedAssetTypes.Contains(asset.ToLowerInvariant()))
                .WithMessage(Error.Validation(
                    "asset.not.allowed",
                    $"Allowed assets: {string.Join(",", AllowedAssetTypes)} ").Serialize());

        RuleFor(r => r.ContentType)
            .NotEmpty()
                .WithMessage(AppErrors.Validation.CannotBeEmpty("content_type").Serialize());

        RuleFor(r => r.Size)
            .GreaterThan(0)
                .WithMessage(AppErrors.Validation.MustBeGreaterThan("size", 0).Serialize())
            .LessThanOrEqualTo(VideoAsset.MAX_SIZE)
                .WithMessage(Error.Validation(
                    "size.too.big", 
                    $"Size must be <= {VideoAsset.MAX_SIZE} bytes.").Serialize());

        RuleFor(r => r.Context)
            .NotEmpty()
                .WithMessage(AppErrors.Validation.CannotBeEmpty("context").Serialize())
            .Must(ctx => AllowedContexts.Contains(ctx.ToLowerInvariant()))
                .WithMessage(Error
                    .Validation(
                     "context.not.allowed", 
                     $"Allowed contexts: {string.Join(",", AllowedContexts)}").Serialize());
        RuleFor(r => r.ContextId)
            .NotEmpty()
                .WithMessage(AppErrors.Validation.CannotBeEmpty("context_id").Serialize());
    }    
}