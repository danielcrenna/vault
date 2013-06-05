namespace Pratt
{
    public enum TokenType
    {
        LeftParen,
        RightParen,
        Comma,
        Assign,
        Plus,
        Minus,
        Asterisk,
        Slash,
        Caret,
        Tilde,
        Bang,
        Question,
        Colon,
        Name,
        EOF
    }

    internal static class TokenTypeExtensions
    {
        public static char? Punctuator(this TokenType type)
        {
            switch (type)
            {
                case TokenType.LeftParen:
                    return '(';
                case TokenType.RightParen:
                    return ')';
                case TokenType.Comma:
                    return ',';
                case TokenType.Assign:
                    return '=';
                case TokenType.Plus:
                    return '+';
                case TokenType.Minus:
                    return '-';
                case TokenType.Asterisk:
                    return '*';
                case TokenType.Slash:
                    return '/';
                case TokenType.Caret:
                    return '^';
                case TokenType.Tilde:
                    return '~';
                case TokenType.Bang:
                    return '!';
                case TokenType.Question:
                    return '?';
                case TokenType.Colon:
                    return ':';
                default:
                    return null;
            }
        }
    }
}