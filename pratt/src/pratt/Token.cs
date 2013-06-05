using System.Diagnostics;

namespace Pratt
{
    [DebuggerDisplay("{Type}:{Value}")]
    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
    }
}
