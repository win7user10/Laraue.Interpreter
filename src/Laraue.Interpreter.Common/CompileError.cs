namespace Laraue.Interpreter.Common;

/// <summary>
/// Error that happened in the compile time.
/// </summary>
public class CompileError
{
    /// <summary>
    /// Text of the error.
    /// </summary>
    public required string Message { get; init; }
    
    /// <summary>
    /// Start token of the query where the error occured.
    /// </summary>
    public required int StartPosition { get; init; }
    
    /// <summary>
    /// End token of the query where the error occured.
    /// </summary>
    public required int EndPosition { get; init; }
    
    /// <summary>
    /// Start line number of the query where the error occured.
    /// </summary>
    public required int StartLineNumber { get; init; }
    
    /// <summary>
    /// End line number of the query where the error occured.
    /// </summary>
    public required int EndLineNumber { get; init; }
}