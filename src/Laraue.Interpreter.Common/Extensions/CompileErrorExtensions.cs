namespace Laraue.Interpreter.Common.Extensions;

public static class CompileErrorExtensions
{
    /// <summary>
    /// Throws if any error in the passed list.
    /// </summary>
    /// <exception cref="CompileException"></exception>
    public static void ThrowOnAny(this ICollection<CompileError> errors)
    {
        if (errors.Count != 0)
        {
            throw new CompileException(errors);
        }
    }
}