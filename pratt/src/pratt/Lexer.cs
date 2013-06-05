using System;
using System.Collections;
using System.Collections.Generic;

namespace Pratt
{
    /// <summary>
    /// A very primitive lexer; it takes a string and splits it into a series of
    /// Tokens. Operators and punctuation are mapped to unique keywords. Names,
    /// which can be any series of letters, are turned into NAME tokens, and all other
    /// characters are ignored (except to separate names). Numbers and strings are
    /// not supported. This is really just the bare minimum to give the parser
    /// something to work with.
    /// </summary>
    public class Lexer : IEnumerator<Token>
    {
        public Lexer(string text)
        {
            _index = 0;
            _text = text;

            // Register all of the TokenTypes that are explicit punctuators
            foreach (TokenType type in Enum.GetValues(typeof(TokenType)))
            {
                var punctuator = type.Punctuator();
                if (punctuator.HasValue)
                {
                    _punctuators.Add(punctuator.Value, type);
                }
            }
        }

        private readonly IDictionary<char, TokenType> _punctuators = new Dictionary<char, TokenType>();
        private readonly String _text;
        private int _index;

        public void Dispose()
        {
            
        }

        public bool MoveNext()
        {
            while (_index < _text.Length)
            {
                var c = _text[_index++];

                if (!_punctuators.ContainsKey(c))
                {
                    if (char.IsLetter(c))
                    {
                        var start = _index - 1;
                        while (_index < _text.Length)
                        {
                            if (!char.IsLetter(_text[_index])) break;
                            _index++;
                        }

                        var length = _index - start;
                        var name = _text.Substring(start, length);
                        Current = new Token { Type = TokenType.Name, Value = name };
                        return true;
                    }
                }
                else
                {
                    Current = new Token { Type = _punctuators[c], Value = char.ToString(c) };
                    return true;
                }
            }

            // Once we've reached the end of the string, just return EOF tokens. We'll
            // just keeping returning them as many times as we're asked so that the
            // parser's lookahead doesn't have to worry about running out of tokens.
            Current = new Token { Type = TokenType.EOF, Value = "" };

            return true;
        }

        public void Reset()
        {
            _index = 0;
        }

        public Token Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}