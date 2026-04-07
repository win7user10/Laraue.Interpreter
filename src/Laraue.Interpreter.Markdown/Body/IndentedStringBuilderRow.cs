using System.Text;

namespace Laraue.Interpreter.Markdown.Body
{
    /// <summary>
    /// One row of <see cref="SqlBuilder"/>.
    /// </summary>
    public sealed record IndentedStringBuilderRow
    {
        /// <summary>
        /// Row ident.
        /// </summary>
        public int Ident { get; set; }

        /// <summary>
        /// Row content.
        /// </summary>
        public StringBuilder StringBuilder { get; } = new ();

        /// <summary>
        /// Initializes a new row with the passed ident.
        /// </summary>
        /// <param name="ident"></param>
        public IndentedStringBuilderRow(int ident)
        {
            Ident = ident;
        }
        
        /// <summary>
        /// Initializes a new row with the passed ident and string value.
        /// </summary>
        /// <param name="ident"></param>
        /// <param name="value"></param>
        public IndentedStringBuilderRow(int ident, string? value) : this(ident)
        {
            StringBuilder.Append(value);
        }
    }
}