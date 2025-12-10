using System.Collections;

namespace Shared.Kernel.Errors;

public class Errors
{
    private readonly List<Error> _errors;

    public List<Error> List => _errors;
    public bool HasErrors() => _errors.Count > 0;
    public Errors(Error error)
    {
        _errors =  new List<Error> { error };
    }

    public Errors(IEnumerable<Error> errors)
    {
        _errors = new List<Error>(errors);
    }

    public override string ToString()
    {
        return string.Join(Environment.NewLine, _errors);
    }
}