using System.IO;
using System.Text;
using NUnit.Framework;

namespace ebnf.Tests.EBNF
{
    [TestFixture]
    public class PreProcessorTests
    {
        private Lexer _lexer;

        [SetUp]
        public void SetUp()
        {
            _lexer = new Lexer();
        }

        [TestCase(" (* A comment is not a rule *) ", 0)]
        [TestCase(" (* Mixed * operators * are * allowed * in * a comment *) ", 0)]
        [TestCase(" (* Comments *) example (* can *) = (* go *) 'EXAMPLE' (* anywhere *)", 3)]
        [TestCase(" (* Oops hanging comment", 0)]
        [TestCase(" ( Not a comment at all *)", 4)]
        public void Comments_are_filtered_correctly(string input, int count)
        {
            var stream = Streamify(input);
            var tokens = _lexer.Scan(stream);
            tokens = PreProcessor.FilterComments(tokens);
            Assert.AreEqual(count, tokens.Length);
        }

        private static Stream Streamify(string input)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            return stream;
        }
    }
}