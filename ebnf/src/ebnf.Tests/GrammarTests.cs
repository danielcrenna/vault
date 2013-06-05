using NUnit.Framework;

namespace ebnf.Tests
{
    [TestFixture]
    public class GrammarTests
    {
        [Test]
        public void Grammar_can_instantiate_from_file_path()
        {
            var grammar = new Grammar("Wikipedia", "InputFiles/wikipedia.ebnf");
            Assert.IsNotNull(grammar.Tree);
        }
    }
}
