using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared.Kernel.Errors;

namespace FileService.Domain;

public sealed record StorageKey
{
    private static readonly Regex BucketNameRegex = 
        new(@"^(?!\d+\.\d+\.\d+\.\d+$)(?!-)[a-z0-9]([a-z0-9.-]{1,61})[a-z0-9]$",
            RegexOptions.Compiled);
    
    public string Bucket { get; }
    public string? Prefix { get; }
    public string Key { get; }

    public string Value => Prefix switch
    {
        null or "" => Key,
        _ => $"{Prefix}/{Key}"
    };

    public string FullPath => $"{Bucket}/{Value}";
    
    public StorageKey None => new StorageKey("", null, "");
    
    private StorageKey(string bucket, string? prefix, string key)
    {
        Bucket = bucket;
        Prefix = prefix;
        Key = key;
    }

    
    private static Result<string, Error> NormalizePath(string? input, bool allowMultipleSegments)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var normalized = input.Replace('\\', '/');
        var segments = normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (!allowMultipleSegments && segments.Length > 1)
            return Error.Failure("invalid.argument", "key must not contain slashes");

        return string.Join('/', segments);
    }

    public Result<StorageKey, Error> AppendSegment(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
            return Error.Failure("invalid.argument", "Segment cannot be empty");
        
        var (_, isFailure, normalizedSegment, error) = NormalizePath(segment, allowMultipleSegments: false);
        if (isFailure)
            return error;
        
        var newPrefix = $"{Value}";
        return new StorageKey(Bucket, newPrefix, normalizedSegment);
    }
    
    public static Result<StorageKey, Error> Create(string bucket, string key, string? prefix = null)
    {
        if (string.IsNullOrWhiteSpace(bucket))
            return Error.Failure("invalid.argument", "'bucket' cannot be empty");
        
        if (string.IsNullOrWhiteSpace(key))
            return Error.Failure("invalid.argument", "'key' cannot be empty");
        
        if (!BucketNameRegex.IsMatch(bucket))
            return Error.Failure("invalid.argument", "'bucket' has invalid format");
        
        var normalizedPrefixResult = NormalizePath(prefix, allowMultipleSegments: true);
        if (normalizedPrefixResult.IsFailure)
            return normalizedPrefixResult.Error;
        
        var normalizedKeyResult = NormalizePath(key, allowMultipleSegments: false);
        if (normalizedKeyResult.IsFailure)
            return normalizedKeyResult.Error;
        
        return new StorageKey(bucket, normalizedKeyResult.Value, normalizedPrefixResult.Value);
    }
}