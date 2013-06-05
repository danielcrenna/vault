using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ebnf.Tokens;

namespace ebnf.Tests.EBNF
{
    [TestFixture]
    public class LexerTests
    {
        private Lexer _lexer;

        [SetUp]
        public void SetUp()
        {
            _lexer = new Lexer();
        }

        [Test]
        public void Whitespace_stream_is_read_until_end()
        {
            Assert.DoesNotThrow(() => _lexer.Scan(Streamify("          ")));
        }

        [Test]
        public void Lexing_a_stream_is_disposal_safe()
        {
            Assert.DoesNotThrow(() =>
            {
                using (var stream = Streamify("          "))
                {
                    _lexer.Scan(stream);
                    _lexer.Scan(stream);
                    _lexer.Scan(stream);
                }
            });
        }

        [TestCase("|", 1)]
        [TestCase("A ", 0)]
        [TestCase(" ", 0)]
        [TestCase(" | ", 1)]
        [TestCase("  (|) ", 3)]
        [TestCase("A = white space", 1)]
        public void Operators_are_correctly_tokenized(string input, int count)
        {
            ICollection<Token> tokens;
            using (var stream = Streamify(input))
            {
                tokens = _lexer.Scan(stream);
            }
            var operators = tokens.OfType<Operator>().ToList();
            Assert.AreEqual(count, operators.Count, string.Format("'{0}' does not have {1} operators.", input, count));
        }

        [TestCase("A", 0)]
        [TestCase("'A'", 1)]
        [TestCase(" 'Identifier-Is-Literal' ", 1)]
        [TestCase(" '!@#$%' ", 1)]
        [TestCase("  '\"' ", 1)]
        [TestCase("  \"'\" ", 1)]
        public void Literals_are_correctly_tokenized(string input, int count)
        {
            ICollection<Token> tokens;
            using (var stream = Streamify(input))
            {
                tokens = _lexer.Scan(stream);
            }
            var literals = tokens.OfType<Literal>().ToList();
            Assert.AreEqual(count, literals.Count, string.Format("'{0}' does not have {1} literals.", input, count));
        }
        
        [TestCase("A", 1)]
        [TestCase("A A", 1)]
        [TestCase("A A ;", 1)]
        [TestCase(" (|A|) ", 1)]
        [TestCase(" Hyphenated-Identifier ", 1)]
        [TestCase(" 'Hyphenated-Identifier' ", 0)]
        public void Identifiers_are_correctly_tokenized(string input, int count)
        {
            ICollection<Token> tokens;
            using (var stream = Streamify(input))
            {
                tokens = _lexer.Scan(stream);
            }
            var identifiers = tokens.OfType<Identifier>().ToList();
            Assert.AreEqual(count, identifiers.Count, string.Format("'{0}' does not have {1} identifiers.", input, count));
        }

        [Test]
        public void Sample_grammar_is_scannable()
        {
            Assert.DoesNotThrow(() =>
            {
                ICollection<Token> tokens;
                using (var stream = File.OpenRead("InputFiles/wikipedia.ebnf"))
                {
                    tokens = _lexer.Scan(stream);
                }
                foreach (var token in tokens)
                {
                    Console.Write(token.Value + ' ');
                }
            });
        }

        [Test]
        public void Line_endings_are_correctly_counted()
        {
            var input = "A |\r\n A";         // CRLF
            Line_endings_are_correctly_counted(input);

            input = "A |\r A";               // CR
            Line_endings_are_correctly_counted(input);

            input = "A |\n A";               // LF
            Line_endings_are_correctly_counted(input);

            input = "A |\u000d\u000a A";     // Unicode CRLF
            Line_endings_are_correctly_counted(input);

            input = "A |\u000d A";           // Unicode CR
            Line_endings_are_correctly_counted(input);

            input = "A |\u000a A";           // Unicode LF
            Line_endings_are_correctly_counted(input);

            input = "A |\u2028 A";           // Unicode Line separator
            Line_endings_are_correctly_counted(input);

            input = "A |\u2029 A";           // Unicode Paragraph separator
            Line_endings_are_correctly_counted(input);

            input = "A |\u0003 A";           // Unicode End of Text
            Line_endings_are_correctly_counted(input);
        }

        [TestCase("A", 1)]
        [TestCase(" A", 2)]
        [TestCase("  A", 3)]
        [TestCase("\rA", 1)]
        [TestCase("\r\n A", 2)]
        [TestCase("\r\n \rA", 1)]
        [TestCase("aa = \"A\";", 9)]
        [TestCase("aa = \"A\";\r\nbb = 3 *", 8)]
        public void Token_line_position_is_correctly_counted(string input, int position)
        {
            Token token;
            using (var stream = Streamify(input))
            {
                var tokens = _lexer.Scan(stream);
                token = tokens.LastOrDefault();
            }
            Assert.AreEqual(position, token.Position, string.Format("Token is not at position {0}.", position));
        }

        private void Line_endings_are_correctly_counted(string input)
        {
            ICollection<Token> tokens;
            using (var stream = Streamify(input))
            {
                tokens = _lexer.Scan(stream);
                Assert.AreEqual(2, tokens.Last().Line, "The last line of the input was not the second line.");
            }
        }

        private static Stream Streamify(string input)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            return stream;
        }
    }
}
