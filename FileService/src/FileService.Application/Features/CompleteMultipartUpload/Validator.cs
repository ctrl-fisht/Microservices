using FileService.Contracts.Requests;
using FluentValidation;
using Shared.Kernel.Errors;

namespace FileService.Application.Features.CompleteMultipartUpload;

public class Validator : AbstractValidator<CompleteMultipartUploadRequest>
{
    public Validator()
    {
        RuleFor(r => r.MediaAssetId)
            .NotEmpty()
            .WithMessage(AppErrors.Validation.CannotBeEmpty("MediaAssetId").Serialize());
        
        RuleFor(r => r.UploadId)
            .NotEmpty()
            .WithMessage(AppErrors.Validation.CannotBeEmpty("UploadId").Serialize());
        
        RuleFor(r => r.PartETags)
            .NotEmpty()
            .WithMessage(AppErrors.Validation.CannotBeEmpty("PartETags").Serialize());
    }
}