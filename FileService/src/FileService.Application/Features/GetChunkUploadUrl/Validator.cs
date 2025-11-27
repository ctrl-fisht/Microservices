using FileService.Contracts.Requests;
using FluentValidation;
using Shared.Kernel.Errors;

namespace FileService.Application.Features.GetChunkUploadUrl;

public class Validator : AbstractValidator<GetChunkUploadUrlRequest>
{
    public Validator()
    {
        RuleFor(r => r.MediaAssetId).NotEmpty()
            .WithMessage(AppErrors.Validation.CannotBeEmpty("MediaAssetId").Serialize());

        RuleFor(r => r.PartNumber)
            .GreaterThan(0)
            .WithMessage(Error.Validation(
                "invalid.part.number",
                "PartNumber must be greater than zero")
                .Serialize());
        
        RuleFor(r => r.UploadId).NotEmpty()
            .WithMessage(AppErrors.Validation.CannotBeEmpty("UploadId").Serialize());
    }
}