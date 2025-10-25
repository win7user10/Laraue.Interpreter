using Laraue.Interpreter.Common;
using Laraue.Interpreter.Common.Extensions;

namespace Laraue.Interpreter.Scanning.Extensions;

public static class ScanResultExtensions
{
    /// <summary>
    /// Get errors from the scan result as <see cref="CompileError"/> list.
    /// </summary>
    public static ICollection<CompileError> GetCompileErrors<TTokenType>(this ScanResult<TTokenType> scanResult)
        where TTokenType : struct, Enum
    {
        var errors = new List<CompileError>();
        
        foreach (var scanError in scanResult.Errors)
        {
            errors.Add(new CompileError
            {
                Message = $"Syntax error: {scanError.Error}",
                StartPosition = scanError.StartPosition,
                EndPosition = scanError.EndPosition,
                StartLineNumber = scanError.LineNumber,
                EndLineNumber = scanError.LineNumber,
            });
        }
        
        return errors;
    }

    /// <summary>
    /// Throw if any error exists in the scan result,
    /// </summary>
    public static void ThrowOnAny<TTokenType>(this ScanResult<TTokenType> scanResult)
        where TTokenType : struct, Enum
    {
        scanResult.GetCompileErrors().ThrowOnAny();
    }
}