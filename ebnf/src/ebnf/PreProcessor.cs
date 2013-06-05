using System.Collections.Generic;
using System.Linq;
using ebnf.Tokens;

namespace ebnf
{
    public class PreProcessor
    {
        public static Token[] FilterComments(Token[] tokens)
        {
            var before = tokens.Length;
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                var last = (tokens.Length - 1 <= i);

                if (!(token is Operator) || !token.Value.Equals("(") || last)
                {
                    continue;
                }

                var next = tokens[i + 1];
                if (!(next is Operator) || !next.Value.Equals("*"))
                {
                    continue;
                }

                var start = i;
                var count = 2;
                NextToken(tokens, ref i);

                token = NextToken(tokens, ref i);
                while (token != null &&
                       !(token.Value.Equals("*") && PeekNextToken(tokens, i) != null &&
                         PeekNextToken(tokens, i).Value.Equals(")")))
                {
                    count++;
                    token = NextToken(tokens, ref i);
                }

                if (token != null)
                {
                    count += 2;
                }

                tokens = RemoveRange(tokens, start, count);
            }
            var after = tokens.Length;
            
            if(after < before)
            {
                tokens = FilterComments(tokens);
            }

            return tokens;
        }

        private static Token[] RemoveRange(Token[] array, int index, int count)
        {
            // TODO no LINQ
            var list = array.ToList();
            list.RemoveRange(index, count);
            array = list.ToArray();
            return array;
        }

        private static Token NextToken(IList<Token> tokens, ref int index)
        {
            return tokens.Count - 1 > index ? tokens[++index] : null;
        }

        private static Token PeekNextToken(IList<Token> tokens, int index)
        {
            return tokens.Count - 1 > index ? tokens[index + 1] : null;
        }
    }
}