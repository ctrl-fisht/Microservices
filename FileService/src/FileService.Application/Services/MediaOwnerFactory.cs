using CSharpFunctionalExtensions;
using FileService.Domain.ValueObjects;
using Shared.Kernel.Errors;

namespace FileService.Application.Services;

public sealed class MediaOwnerFactory
{
    public Result<MediaOwner, Error> Create(string context, Guid entityId)
        => context.ToLowerInvariant() switch
        {
            "course" => MediaOwner.ForCourse(entityId),
            "lesson" => MediaOwner.ForLesson(entityId),
            "user" => MediaOwner.ForUser(entityId),
            "department" => MediaOwner.ForDepartment(entityId),
            _ => Error.Failure("unknown.media.owner", "Media owner not allowed")
        };
}