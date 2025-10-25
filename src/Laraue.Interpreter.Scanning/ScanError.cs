namespace Laraue.Interpreter.Scanning;

/// <summary>
/// The error occured while scanning.
/// </summary>
public class ScanError
{
    /// <summary>
    /// The error start position.
    /// </summary>
    public required int StartPosition { get; set; }
    
    /// <summary>
    /// The error end position.
    /// </summary>
    public required int EndPosition { get; set; }
    
    /// <summary>
    /// The error line.
    /// </summary>
    public required int LineNumber { get; set; }
    
    /// <summary>
    /// The error text.
    /// </summary>
    public required string Error { get; set; }
}