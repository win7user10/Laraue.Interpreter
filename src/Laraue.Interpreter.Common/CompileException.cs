using System.Text;

namespace Laraue.Interpreter.Common;

/// <summary>
/// The compiler exception with a human-readable message.
/// </summary>
public class CompileException : Exception
{
    public ICollection<CompileError> Errors { get; }

    /// <summary>
    /// Consume the error list to create the exception with a readable message.
    /// </summary>
    /// <param name="errors"></param>
    public CompileException(ICollection<CompileError> errors)
        : base(GetErrorMessage(errors))
    {
        Errors = errors;
    }

    private static string GetErrorMessage(ICollection<CompileError> errors)
    {
        var sb = new StringBuilder();
        
        sb.AppendJoin(Environment.NewLine, errors.Select(e => e.Message));

        return sb.ToString();
    }
}