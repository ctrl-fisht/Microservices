using FileService.Contracts.Requests;
using FluentValidation;
using Shared.Kernel.Errors;

namespace FileService.Application.Features.AbortMultipartUpload;

public sealed class Validator : AbstractValidator<AbortMultipartUploadRequest>
{
    public Validator()
    {
        RuleFor(r => r.MediaAssetId)
            .NotEmpty()
            .WithMessage(AppErrors.Validation.CannotBeEmpty("MediaAssetId").Serialize());
        
        RuleFor(r => r.UploadId)
            .NotEmpty()
            .WithMessage(AppErrors.Validation.CannotBeEmpty("UploadId").Serialize());
    }
}