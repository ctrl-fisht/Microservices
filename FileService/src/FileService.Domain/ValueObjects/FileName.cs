using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using Shared.Kernel.Errors;

namespace FileService.Domain.ValueObjects;

public sealed record FileName
{
    
    public string Name { get; }
    
    public string Extension { get; }

    [JsonConstructor]
    private FileName(string name, string extension)
    {
        Name = name;
        Extension = extension;
    }

    public static Result<FileName, Error> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Failure("invalid.argument", "File 'name' cannot be empty");

        var splitted = name.Split('.');
        
        if (splitted.Length != 2)
            return Error.Failure("invalid.argument", "File 'name' must be 'xxxxxx.yyy' (with extension)");

        var filename = splitted[0];
        var extension = splitted[1];

        if (extension.ToLower() != extension)
            return Error.Failure("invalid.argument", "File 'name' extensions must be lowercase");

        return new FileName(filename, extension);
    }

    public static FileName FromDb(string name, string extension) => new(name, extension);
}