using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Pratt;
using pratt.Expressions;

namespace pratt.Parselets
{
    /// <summary>
    /// Parselet to parse a function call like "a(b, c, d)".
    /// </summary>
    public class CallParselet : IInfixParselet
    {
        public IExpression Parse(Parser parser, IExpression left, Token token)
        {
            // Parse the comma-separated arguments until we hit, ")".
            var args = new List<IExpression>();
    
            // There may be no arguments at all.
            if (!parser.Match(TokenType.RightParen))
            {
                do
                {
                    args.Add(parser.ParseExpression());
                } 
                while (parser.Match(TokenType.Comma));
                
                parser.Consume(TokenType.RightParen);
            }

            return new CallExpression(left, args);
        }

        public Precedence Precedence
        {
            get { return Precedence.Call; }
        }
    }
}

