using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ebnf.Nodes;
using ebnf.Tokens;

namespace ebnf.Tests.EBNF
{
    [TestFixture]
    public class ParserTests
    {
        private Lexer _lexer;
        private Parser _parser;

        [SetUp]
        public void SetUp()
        {
            _lexer = new Lexer();
            _parser = new Parser();
        }

        [Test]
        public void Empty_token_stream_reports_no_rules_error()
        {
            Token[] tokens;
            var tree = ParseStream(Streamify(""), out tokens);
            Assert.AreEqual(0, tokens.Length);
            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.Errors.Count);
            AssertErrorContent(tree, ErrorStrings.NoRulesFound);
        }

        [Test]
        public void Dangling_rule_reports_missing_definition_error_with_no_token()
        {
            Token[] tokens;
            var tree = ParseStream(Streamify("An-Identifier"), out tokens);
            Assert.AreEqual(1, tokens.Length);
            Assert.IsNotNull(tree);
            Assert.AreEqual(2, tree.Errors.Count);
            AssertErrorContent(tree, ErrorStrings.ExpectedDefinition, "nothing");
        }

        [Test]
        public void Simple_example_parses_with_no_errors()
        {
            var tree = ParseIsErrorFree("A = 'A';");
            Assert.AreEqual(1, tree.Rules.Count);
            Assert.IsTrue(tree.Rules[0] is Rule);
        }

        [Test]
        public void Terminator_separates_rules()
        {
            var tree = ParseIsErrorFree("A = 'A'; B = 'B'");
            Assert.AreEqual(2, tree.Rules.Count);
            Assert.IsTrue(tree.Rules[0] is Rule);
            Assert.IsTrue(tree.Rules[1] is Rule);
        }

        [Test]
        public void Identifiers_in_an_expression_can_have_white_space()
        {
            var tree = ParseIsErrorFree("A = white space");
            Assert.AreEqual(1, tree.Rules.Count);
            Assert.IsTrue(tree.Rules[0] is Rule);
            Assert.AreEqual(1, ((Rule)tree.Rules[0]).Expression.Terms[0].Factors.Count, "'white space' must evaluate to a single identifier");
        }

        [Test]
        public void Scoped_expressions_parse_correctly()
        {
            ParseIsErrorFree("A = { white space }");
        }

        [TestCase("bb = 3 * aa", 2)]
        [TestCase("cc = 3 * [aa], \"C\"", 3)]
        public void Repeat_operator_identifies_correct_number_of_terms(string input, int count)
        {
            var tree = ParseIsErrorFree(input);
            Assert.AreEqual(count, ((Rule)tree.Rules[0]).Expression.Terms.Count);
        }

        [Test]
        public void Multiple_terms_on_previous_line_does_not_jeopardize_next_rule()
        {
            var line1 = "cc = 3 * [aa], \"C\";";    // 3 terms
            var line2 = "dd = {aa}, \"D\"";         // 2 terms
            var input = line1 + Environment.NewLine + line2;

            var tree = ParseIsErrorFree(input);
            Assert.AreEqual(3, ((Rule)tree.Rules[0]).Expression.Terms.Count);
            Assert.AreEqual(2, ((Rule)tree.Rules[1]).Expression.Terms.Count);
        }

        [Test]
        public void Alternations_are_handled_correctly()
        {
            var input = "digit = '0' | '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9' ;";
            var tree = ParseIsErrorFree(input);
            Assert.AreEqual(10, ((Rule)tree.Rules[0]).Expression.Terms.Count);
        }

        [TestCase("digit = '0' | '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9' ;")]
        public void Expression_nodes_are_correctly_ordered(string input)
        {
            var tree = ParseIsErrorFree(input);
            var expr = ((Rule) tree.Rules[0]).Expression;
            var nodes = expr.Nodes;
            Assert.AreEqual(19, nodes.Count);
            Assert.IsTrue(expr.Nodes[0] is Term);
            Assert.IsTrue(expr.Nodes[1] is Alternation);
            Assert.IsTrue(expr.Nodes[2] is Term);
            Assert.IsTrue(expr.Nodes[3] is Alternation);
            Assert.IsTrue(expr.Nodes[4] is Term);
            Assert.IsTrue(expr.Nodes[5] is Alternation);
            Assert.IsTrue(expr.Nodes[6] is Term);
            Assert.IsTrue(expr.Nodes[7] is Alternation);
            Assert.IsTrue(expr.Nodes[8] is Term);
            Assert.IsTrue(expr.Nodes[9] is Alternation);
            Assert.IsTrue(expr.Nodes[10] is Term);
            Assert.IsTrue(expr.Nodes[11] is Alternation);
            Assert.IsTrue(expr.Nodes[12] is Term);
            Assert.IsTrue(expr.Nodes[13] is Alternation);
            Assert.IsTrue(expr.Nodes[14] is Term);
            Assert.IsTrue(expr.Nodes[15] is Alternation);
            Assert.IsTrue(expr.Nodes[16] is Term);
            Assert.IsTrue(expr.Nodes[17] is Alternation);
            Assert.IsTrue(expr.Nodes[18] is Term);
        }

        [Test]
        public void Literals_can_have_terminator_terminal_when_in_a_repetition()
        {
            var input = "stuff = { something, ';', something else }";
            var tree = ParseIsErrorFree(input);
            Assert.AreEqual(1, ((Rule)tree.Rules[0]).Expression.Terms.Count);
        }

        [Test]
        public void Special_sequences_parse_correctly()
        {
            ParseIsErrorFree("white space = ? white space characters ? ;");
        }

        [Test]
        public void Precedence_example_parses_with_no_errors()
        {
            Token[] tokens;
            var tree = ParseStream(File.OpenRead("InputFiles/precedence.ebnf"), out tokens);
            Assert.IsNotNull(tree);
            foreach(var error in tree.Errors)
            {
                Console.WriteLine(error);
            }
            Assert.AreEqual(0, tree.Errors.Count);
        }

        [Test]
        public void Wikipedia_example_parses_with_no_errors()
        {
            ParseIsErrorFree(File.OpenRead("InputFiles/wikipedia.ebnf"));
        }

        [Test]
        public void Calculator_example_parses_with_no_errors()
        {
            ParseIsErrorFree(File.OpenRead("InputFiles/calc.ebnf"));
        }

        [Test]
        public void Parser_accepts_tokens()
        {
            Assert.DoesNotThrow(() =>
            {
                Token[] tokens;
                ParseStream(File.OpenRead("InputFiles/wikipedia.ebnf"), out tokens);
            });
        }

        private Tree ParseIsErrorFree(string input)
        {
            var stream = Streamify(input);
            return ParseIsErrorFree(stream);
        }

        private Tree ParseIsErrorFree(Stream stream)
        {
            Token[] tokens;
            var tree = ParseStream(stream, out tokens);
            Assert.IsNotNull(tree);
            foreach (var error in tree.Errors)
            {
                Console.WriteLine(error);
            }
            Assert.AreEqual(0, tree.Errors.Count);
            return tree;
        }

        private static Stream Streamify(string input)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            return stream;
        }

        private static void AssertErrorContent(Tree tree, string error, params object[] args)
        {
            var first = tree.Errors.First();
            Trace.WriteLine(first.Description);

            if(args.Length > 0)
            {
                Assert.AreEqual(string.Format(error, args), first.Description);
            }
            else
            {
                Assert.AreEqual(error, first.Description);    
            }
        }

        private Tree ParseStream(Stream stream, out Token[] tokens)
        {
            using (stream)
            {
                tokens = _lexer.Scan(stream);
                var tree =_parser.Parse(tokens);
                return tree;
            }
        }
    }
}