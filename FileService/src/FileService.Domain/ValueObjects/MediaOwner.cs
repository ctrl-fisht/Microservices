using CSharpFunctionalExtensions;
using Shared.Kernel.Errors;

namespace FileService.Domain;

public sealed record MediaOwner
{
    private static List<string> _contextWhiteList = 
        ["lesson", "course", "user", "department"];
    
    public string Context { get; }
    public Guid EntityId { get; }

    private MediaOwner(string context, Guid entityId)
    {
        Context = context;
        EntityId = entityId;
    }
    
    private static Result<MediaOwner, Error> Create(string context, Guid entityId)
    {
        if (string.IsNullOrWhiteSpace(context))
            return Error.Failure("invalid.argument", "Context cannot be empty");
        
        if (context.Length > 50)
            return Error.Failure("invalid.argument", "Context cannot be longer than 50 characters");
        
        
        if (entityId == Guid.Empty)
            return Error.Failure("invalid.argument", "EntityId cannot be empty");

        var contextLowercase = context.ToLowerInvariant();
        
        if (!_contextWhiteList.Contains(context.ToLowerInvariant()))
            return Error.Failure("invalid.argument", "Context not in whitelist");
        
        return new MediaOwner(contextLowercase, entityId);
    }

    public static Result<MediaOwner, Error> ForLesson(Guid entityId)
        => Create("lesson", entityId);
    public static Result<MediaOwner, Error> ForCourse(Guid entityId)
        => Create("course", entityId);
    public static Result<MediaOwner, Error> ForUser(Guid entityId)
        => Create("user", entityId);
    public static Result<MediaOwner, Error> ForDepartment(Guid entityId)
        => Create("department", entityId);
}