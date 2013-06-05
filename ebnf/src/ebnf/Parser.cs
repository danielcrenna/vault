using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ebnf.Nodes;
using ebnf.Tokens;
using Identifier = ebnf.Tokens.Identifier;
using Operator = ebnf.Tokens.Operator;

namespace ebnf
{
    public class Parser
    {
        public static Tree Parse(string path)
        {
            return Parse(File.OpenRead(path));
        }

        public static Tree Parse(Stream stream)
        {
            var lexer = new Lexer();
            var parser = new Parser();
            using (stream)
            {
                var tokens = lexer.Scan(stream);
                var tree = parser.Parse(tokens);
                return tree;
            }
        }

        public Tree Parse(Token[] tokens)
        {
            var tree = new Tree();

            tokens = PreProcessor.FilterComments(tokens);

            if(tokens == null || tokens.Length == 0)
            {
                tree.Errors.Add(CreateError(null, ErrorStrings.NoRulesFound));
                return tree;
            }
            
            for(var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                
                if(token is Identifier)
                {
                    ParseRule(tree, token, tokens, ref i);
                }
            }

            if(tree.Rules.Count == 0)
            {
                tree.Errors.Add(CreateError(null, ErrorStrings.NoRulesFound));
            }

            return tree;
        }
        
        private static void ParseRule(Tree tree, Token token, Token[] tokens, ref int index)
        {
            if (token is Identifier)
            {
                var ruleToken = token;

                var rule = new Rule { Identifier = token.Value };

                token = NextToken(tokens, ref index);

                if (IsDefinition(token))
                {
                    token = NextToken(tokens, ref index);

                    rule.Expression = ParseExpression(tree, ref token, tokens, ref index, ';');

                    tree.Add(rule);
                }
                else
                {
                    ruleToken.Position += 2;
                    tree.Errors.Add(CreateError(ruleToken, string.Format(ErrorStrings.ExpectedDefinition, token == null ? "nothing" : token.Value)));
                }
            }
            else
            {
                tree.Errors.Add(CreateError(token, ErrorStrings.RulesMustBeginWithIdentifier));
            }
        }

        private static bool IsDefinition(Token token)
        {
            return token != null && (token is Operator) && token.Value.Equals("=");
        }

        private static Token NextToken(IList<Token> tokens, ref int index)
        {
            return tokens.Count - 1 > index ? tokens[++index] : null;
        }

        private static Expression ParseExpression(Tree tree, ref Token token, Token[] tokens, ref int index, char terminator)
        {
            var node = new Expression();
            
            while(ParseTerms(node, tree, ref token, tokens, ref index, terminator)) { }
            
            return node;
        }

        private static bool ParseTerms(Expression node, Tree tree, ref Token token, Token[] tokens, ref int index, char terminator)
        {
            var term = ParseTerm(tree, ref token, tokens, ref index);

            node.Terms.Add(term);

            if (token == null || token is Operator && token.Value.Equals(terminator.ToString(CultureInfo.InvariantCulture)))
            {
                return false;
            }

            while (IsExpressionOperator(token))
            {
                if (IsAlternation(token))
                {
                    node.Operators.Add(new Alternation { Value = token.Value });
                }
                if (IsConcatenation(token))
                {
                    node.Operators.Add(new Concatenation { Value = token.Value });
                }
                if (IsRepeat(token))
                {
                    node.Operators.Add(new Repeat { Value = token.Value });
                }
                if (IsExcept(token))
                {
                    node.Operators.Add(new Except { Value = token.Value });
                }
                token = NextToken(tokens, ref index);
            }

            return true;
        }

        private static bool IsExpressionOperator(Token token)
        {
            return token is Operator && (token.Value.Equals("|") || token.Value.Equals(",") || token.Value.Equals("*") || token.Value.Equals("-"));
        }

        private static bool IsExcept(Token token)
        {
            return token is Operator && token.Value.Equals("-");
        }

        private static bool IsAlternation(Token token)
        {
            return token is Operator && token.Value.Equals("|");
        }

        private static bool IsConcatenation(Token token)
        {
            return token is Operator && token.Value.Equals(",");
        }

        private static bool IsRepeat(Token token)
        {
            return token is Operator && token.Value.Equals("*");
        }

        private static Term ParseTerm(Tree tree, ref Token token, Token[] tokens, ref int index)
        {
            var node = new Term();

            var first = ParseFactor(tree, token, tokens, ref index);

            if(first != null)
            {
                node.Factors.Add(first);
            }

            token = NextToken(tokens, ref index);

            if (token == null)
            {
                return node;
            }

            while (token is Literal || !EqualsAny(token.Value, new[] 
            {
                '-', // Except
                '*', // Repetition 
                ',', // Concatentation
                ';', // Termination
                '=', // Definition
                '|', // Alternation
                ')', // End Grouping
                ']', // End Option
                '}', // End Repetition
                '?'  // Special Sequence
            }))
            {
                var additional = ParseFactor(tree, token, tokens, ref index);

                if (additional != null)
                {
                    node.Factors.Add(additional);
                }

                token = NextToken(tokens, ref index);

                if(token == null)
                {
                    return node;
                }
            }

            return node;
        }

        private static Factor ParseFactor(Tree tree, Token token, Token[] tokens, ref int index)
        {
            if(token is Identifier)
            {
                var node = new ebnf.Nodes.Identifier {Value = token.Value};
                return node;
            }
            if(token is Literal)
            {
                var node = new Terminal { Value = token.Value };
                return node;
            }
            if(token is Operator && token.Value.Equals("("))
            {
                token = NextToken(tokens, ref index);
                var node = new Grouping
                {
                    Expression = ParseExpression(tree, ref token, tokens, ref index, ')')
                };
                if(!(token is Operator) || !token.Value.Equals(")"))
                {
                    tree.Errors.Add(CreateError(token, ErrorStrings.HangingGrouping));
                }
                return node;
            }
            if (token is Operator && token.Value.Equals("["))
            {
                token = NextToken(tokens, ref index);
                var node = new Option
                {
                    Expression = ParseExpression(tree, ref token, tokens, ref index, ']')
                };
                if (!(token is Operator) || !token.Value.Equals("]"))
                {
                    tree.Errors.Add(CreateError(token, ErrorStrings.HangingOption));
                }
                return node;
            }
            if (token is Operator && token.Value.Equals("{"))
            {
                token = NextToken(tokens, ref index);
                var node = new Repetition
                {
                    Expression = ParseExpression(tree, ref token, tokens, ref index, '}')
                };
                if (!(token is Operator) || !token.Value.Equals("}"))
                {
                    tree.Errors.Add(CreateError(token, ErrorStrings.HangingRepetition));
                }
                return node;
            }
            if (token is Operator && token.Value.Equals("?"))
            {
                token = NextToken(tokens, ref index);
                var node = new SpecialSequence
                {
                    Expression = ParseExpression(tree, ref token, tokens, ref index, '?')
                };
                if (!(token is Operator) || !token.Value.Equals("?"))
                {
                    tree.Errors.Add(CreateError(token, ErrorStrings.HangingSpecialSequence));
                }
                return node;
            }
            tree.Errors.Add(CreateError(token, string.Format(ErrorStrings.ExpectedFactor, token == null ? "nothing" : token.Value)));
            return null;
        }

        private static Error CreateError(Token token, string description)
        {
            return new Error { Description = description, Line = token != null ? token.Line : -1, Position = token != null ? token.Position : -1 };
        }

        private static bool EqualsAny(string value, IEnumerable<char> set)
        {
            var match = false;
            foreach(var c in set)
            {
                match |= value[0] == c;
            }
            return match;
        }
    }
}
