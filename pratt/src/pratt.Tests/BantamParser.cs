using System.Collections.Generic;
using Pratt;
using pratt.Parselets;

namespace pratt.Tests
{
    /// <summary>
    ///  Extends the generic Parser class with support for parsing the actual Bantam grammar.
    /// </summary>
    public class BantamParser : Parser
    {
        public BantamParser(IEnumerator<Token> enumerator) : base(enumerator)
        {
            // Register all of the parselets for the grammar.

            // Register the tokens that need special parselets.
            Register(TokenType.Name, new NameParselet());
            Register(TokenType.Assign, new AssignParselet());
            Register(TokenType.Question, new ConditionalParselet());
            Register(TokenType.LeftParen, new GroupParselet());
            Register(TokenType.LeftParen, new CallParselet());

            // Register the simple operator parselets.
            Prefix(TokenType.Plus, Precedence.Prefix);
            Prefix(TokenType.Minus, Precedence.Prefix);
            Prefix(TokenType.Tilde, Precedence.Prefix);
            Prefix(TokenType.Bang, Precedence.Prefix);

            // For kicks, we'll make "!" both prefix and postfix, kind of like ++.
            Postfix(TokenType.Bang, Precedence.Postfix);

            InfixLeft(TokenType.Plus, Precedence.Sum);
            InfixLeft(TokenType.Minus, Precedence.Sum);
            InfixLeft(TokenType.Asterisk, Precedence.Product);
            InfixLeft(TokenType.Slash, Precedence.Product);
            InfixRight(TokenType.Caret, Precedence.Exponent);
        }

        /// <summary>
        /// Registers a postfix unary operator parselet for the given token and precedence
        /// </summary>
        public void Postfix(TokenType token, Precedence precedence)
        {
            Register(token, new PostfixOperatorParselet(token, precedence));
        }

        /// <summary>
        /// Registers a prefix unary operator parselet for the given token and precedence
        /// </summary>
        public void Prefix(TokenType token, Precedence precedence)
        {
            Register(token, new PrefixOperatorParselet(token, precedence));
        }

        /// <summary>
        /// Registers a left-associative binary operator parselet for the given token and precedence
        /// </summary>
        public void InfixLeft(TokenType token, Precedence precedence)
        {
            Register(token, new BinaryOperatorParselet(token, precedence, false));
        }
        
        /// <summary> 
        /// Registers a right-associative binary operator parselet for the given token and precedence
        /// </summary>
        /// <param name="token"></param>
        /// <param name="precedence"></param>
        public void InfixRight(TokenType token, Precedence precedence)
        {
            Register(token, new BinaryOperatorParselet(token, precedence, true));
        }
    }
}