using System;
using System.Text;
using NUnit.Framework;
using Pratt;

namespace pratt.Tests
{
    [TestFixture]
    public class PortedTests
    {
        private int _passed;
        private int _failed;
        
        [Test]
        public void Can_run_ported_tests()
        {
            // Function call.
            Test("a()", "a()");
            Test("a(b)", "a(b)");
            Test("a(b, c)", "a(b, c)");
            Test("a(b)(c)", "a(b)(c)");
            Test("a(b) + c(d)", "(a(b) + c(d))");
            Test("a(b ? c : d, e + f)", "a((b ? c : d), (e + f))");

            // Unary precedence.
            Test("~!-+a", "(~(!(-(+a))))");
            Test("a!!!", "(((a!)!)!)");

            // Unary and binary predecence.
            Test("-a * b", "((-a) * b)");
            Test("!a + b", "((!a) + b)");
            Test("~a ^ b", "((~a) ^ b)");
            Test("-a!", "(-(a!))");
            Test("!a!", "(!(a!))");

            // Binary precedence.
            Test("a = b + c * d ^ e - f / g", "(a = ((b + (c * (d ^ e))) - (f / g)))");

            // Binary associativity.
            Test("a = b = c", "(a = (b = c))");
            Test("a + b - c", "((a + b) - c)");
            Test("a * b / c", "((a * b) / c)");
            Test("a ^ b ^ c", "(a ^ (b ^ c))");

            // Conditional operator.
            Test("a ? b : c ? d : e", "(a ? b : (c ? d : e))");
            Test("a ? b ? c : d : e", "(a ? (b ? c : d) : e)");
            Test("a + b ? c * d : e / f", "((a + b) ? (c * d) : (e / f))");

            // Grouping.
            Test("a + (b + c) + d", "((a + (b + c)) + d)");
            Test("a ^ (b + c)", "(a ^ (b + c))");
            Test("(!a)!", "((!a)!)");

            // Show the results.
            if (_failed == 0)
            {
                Console.Out.WriteLine("Passed all {0} tests.", _passed);
            }
            else
            {
                Console.Out.WriteLine("----");
                Console.Out.WriteLine("Failed {0} out of {1} tests.", _failed, (_failed + _passed));
            }

            Assert.AreEqual(0, _failed);
        }
        
        /// <summary>
        /// Parses the given chunk of code and verifies that it matches the expected pretty-printed result
        /// </summary>
        /// <param name="source"></param>
        /// <param name="expected"></param>
        public void Test(string source, string expected)
        {
            var lexer = new Lexer(source);
            Parser parser = new BantamParser(lexer);

            try
            {
                var result = parser.ParseExpression();
                var builder = new StringBuilder();
                result.Print(builder);
                var actual = builder.ToString();

                if (expected.Equals(actual))
                {
                    _passed++;
                }
                else
                {
                    _failed++;
                    Console.Out.WriteLine("[FAIL] Expected: " + expected);
                    Console.Out.WriteLine("         Actual: " + actual);
                }
            }
            catch (ParseException ex)
            {
                _failed++;
                Console.Out.WriteLine("[FAIL] Expected: " + expected);
                Console.Out.WriteLine("          Error: " + ex.Message);
            }
        }
    }
}

