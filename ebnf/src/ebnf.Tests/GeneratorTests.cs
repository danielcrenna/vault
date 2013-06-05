using NUnit.Framework;

namespace ebnf.Tests
{
    [TestFixture]
    public class GeneratorTests
    {
        [TestCase("boo", "Boo")]
        [TestCase("Boo", "Boo")]
        [TestCase("javaScript", "JavaScript")]
        [TestCase("C#", "CSharp")]
        [TestCase("c++", "CPlusPlus")]
        public void Grammar_names_are_normalized_to_pascal_case(string input, string expected)
        {
            var actual = Generator.TransformGrammarName(input);
            Assert.AreEqual(expected, actual);
        }
    }
}
