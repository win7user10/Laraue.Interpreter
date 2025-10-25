namespace Laraue.Interpreter.Scanning;

/// <summary>
/// The result of tokens scan.
/// </summary>
/// <typeparam name="TTokenType"></typeparam>
public class ScanResult<TTokenType> where TTokenType : struct, Enum
{
    /// <summary>
    /// Tokens list. Can be empty then the result is unsuccessful.
    /// </summary>
    public required Token<TTokenType>[] Tokens { get; set; }
    
    /// <summary>
    /// Errors occured while scan.
    /// </summary>
    public required ScanError[] Errors { get; set; }
    
    /// <summary>
    /// Did any error occur?
    /// </summary>
    public bool HasErrors => Errors.Any();
}