using System;
using System.Collections.Generic;
using Pratt;
using pratt.Expressions;
using pratt.Parselets;

namespace pratt
{
    /// <summary>
    /// A top down parser using Pratt's precedence
    /// This is an idiomatic port of the parser at https://github.com/munificent/bantam
    /// </summary>
    /// <seealso href="https://github.com/munificent/bantam" />
    /// <seealso href="http://en.wikipedia.org/wiki/Pratt_parser" />
    /// <seealso href="http://en.wikipedia.org/wiki/Recursive_descent_parser" />
    /// <seealso href="http://effbot.org/zone/simple-top-down-parsing.htm" />
    public class Parser
    {
        public Parser(IEnumerator<Token> tokens)
        {
            _tokens = tokens;
        }

        public void Register(TokenType token, IPrefixParselet parselet)
        {
            _prefixParselets.Add(token, parselet);
        }

        public void Register(TokenType token, IInfixParselet parselet)
        {
            _infixParselets.Add(token, parselet);
        }

        public IExpression ParseExpression(Precedence precedence)
        {
            var token = Consume();

            var prefix = _prefixParselets.ContainsKey(token.Type) ? _prefixParselets[token.Type] : null;
            if (prefix == null)
            {
                throw new ParseException(string.Format("Could not parse \"{0}\".", token.Value));
            }

            var left = prefix.Parse(this, token);

            while (precedence < GetPrecedence())
            {
                token = Consume();

                var infix = _infixParselets[token.Type];
                left = infix.Parse(this, left, token);
            }

            return left;
        }

        public IExpression ParseExpression()
        {
            return ParseExpression(0);
        }

        public bool Match(TokenType expected)
        {
            var token = LookAhead(0);
            if (token.Type != expected)
            {
                return false;
            }

            Consume();
            return true;
        }

        public Token Consume(TokenType expected)
        {
            var token = LookAhead(0);
            if (token.Type != expected)
            {
                throw new ApplicationException(string.Format("Expected token {0} and found {1}", expected, token.Type));
            }

            return Consume();
        }

        public Token Consume()
        {
            LookAhead(0);

            var token = _readTokens[0];
            
            _readTokens.RemoveAt(0);

            return token;
        }

        private Token LookAhead(int distance)
        {
            while (distance >= _readTokens.Count)
            {
                _tokens.MoveNext();
                _readTokens.Add(_tokens.Current);
            }
            return _readTokens[distance];
        }

        private Precedence GetPrecedence()
        {
            if(_infixParselets.ContainsKey(LookAhead(0).Type))
            {
                var parser = _infixParselets[LookAhead(0).Type];
                if (parser != null)
                {
                    return parser.Precedence;
                }    
            }
            return Precedence.Unknown;
        }

        private readonly IEnumerator<Token> _tokens;
        private readonly IList<Token> _readTokens = new List<Token>();
        private readonly IDictionary<TokenType, IPrefixParselet> _prefixParselets = new Dictionary<TokenType, IPrefixParselet>();
        private readonly IDictionary<TokenType, IInfixParselet> _infixParselets = new Dictionary<TokenType, IInfixParselet>();
    }
}
