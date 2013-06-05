using System.Collections.Generic;
using System.IO;
using System.Text;
using ebnf.Tokens;

namespace ebnf
{
    public class Lexer
    {
        private const int BufferLength = 1024;

        public Token[] Scan(Stream stream)
        {
            return Scan(stream, Encoding.UTF8);
        }

        public Token[] Scan(Stream stream, Encoding encoding)
        {
            var tokens = new List<Token>();
            var sb = new StringBuilder();
            
            using(var sr = new StreamReader(stream, encoding))
            {
                var buffer = new char[BufferLength];
                var index = 0;
                var length = 0;
                var line = 1;
                var cr = false;
                var position = 0;

                char? c;
                while((c = NextChar(sr, buffer, ref index, ref length)).HasValue)
                {
                    var value = c.Value;
                    position++;

                    if(IsLineEnding(value))
                    {
                        var lf = IsLineFeed(value);
                        if(!lf || !cr)
                        {
                            line++;
                        }
                        cr = IsCarriageReturn(value);
                        position = 0;
                    }
                    else
                    {
                        cr = false;
                    }
                    
                    if (IsQuotation(value))
                    {
                        var found = position;
                        var sq = IsSingleQuote(value);
                        sb.Clear();
                        while ((c = NextChar(sr, buffer, ref index, ref length)).HasValue)
                        {
                            position++;
                            if (!(sq ? IsSingleQuote(c.Value) : IsDoubleQuote(c.Value)))
                            {
                                sb.Append(c);
                            }
                            else
                            {
                                break;
                            }
                        }
                        tokens.Add(CreateToken<Literal>(sb.ToString(), line, found));
                    }
                    else if (IsIdentifier(value))
                    {
                        var found = position;
                        sb.Clear();
                        sb.Append(value);
                        while ((c = NextChar(sr, buffer, ref index, ref length)).HasValue)
                        {
                            position++;
                            if(IsIdentifier(c.Value) || IsWhiteSpace(c.Value))
                            {
                                sb.Append(c);
                            }
                            else
                            {
                                break;
                            }
                        }
                        var identifier = sb.ToString().Trim();
                        tokens.Add(CreateToken<Identifier>(identifier, line, found));
                    }

                    ScanOperator(position, c, line, tokens);
                }
            }
            return tokens.ToArray();
        }

        private static void ScanOperator(int position, char? c, int line, ICollection<Token> tokens)
        {
            if(!c.HasValue)
            {
                return;
            }

            switch (c)
            {
                case ' ':
                    break;
                case '*': // Off-spec, '*' is used as part of a comment block convention, i.e. (* ... *)
                case ',': // Off-spec, ',' is used for the concatenation convention
                case '?': // Off-spec, '?' is used for the special sequence convention, i.e. ? ... ?
                case '-': // Off-spec, '-' is used for the exception convention
                case '=':
                case '{':
                case '}':
                case '(':
                case ')':
                case '|':
                case '.':
                case ';':
                case '[':
                case ']':
                    tokens.Add(CreateToken<Operator>(new string(new[] {c.Value}), line, position));
                    break;
            }
        }

        private static Token CreateToken<T>(string value, int line, int position) where T : Token, new()
        {
            var token = new T {Value = value, Line = line, Position = position};
            return token;
        }

        private static bool IsLineEnding(char c)
        {
            return Is(c, new[] { '\r', '\n', '\u000d', '\u000a', '\u2028', '\u2029', '\u0003' });
        }

        private static bool IsCarriageReturn(char c)
        {
            return Is(c, new[] { '\r', '\u000d' });
        }

        private static bool IsLineFeed(char c)
        {
            return Is(c, new[] { '\n', '\u000a' });
        }

        private static bool IsQuotation(char c)
        {
            return IsSingleQuote(c) || IsDoubleQuote(c);
        }

        private static bool IsSingleQuote(char c)
        {
            return Is(c, new[] { '\'' });
        }

        private static bool IsDoubleQuote(char c)
        {
            return Is(c, new[] { '"' });
        }

        private static bool IsWhiteSpace(char c)
        {
            return c == ' ';
        }
        
        private static bool IsIdentifier(char c)
        {
            // Identifiers allow whitespace, and having two identifiers in a row without an alternation (|) or concatenation (,) is malformed
            return Is(c, new []
            { 
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '_', '-'
            });
        }

        private static bool Is(char c, IEnumerable<char> range)
        {
            foreach(var r in range)
            {
                if(c == r)
                {
                    return true;
                }
            }
            return false;
        }

        private static char? NextChar(TextReader sr, char[] buffer, ref int index, ref int length)
        {
            if(index >= length)
            {
                index = 0;
                length = sr.Read(buffer, 0, BufferLength);
                if(length == 0)
                {
                    return null;
                }
            }
            var c = buffer[index++];
            return c;
        }

        private class StreamReader : System.IO.StreamReader
        {
            public StreamReader(Stream stream, Encoding encoding) : base(stream, encoding)
            {
                
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(false);
            }
        }
    }
}