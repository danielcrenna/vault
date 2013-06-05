using NUnit.Framework;

namespace ebnf.Tests.CSharp
{
    [TestFixture]
    public class CodeDomCompilerTests
    {
        [Test]
        public void Compiler_can_compile_single_file_into_memory()
        {
            var compiler = new CodeDomCompiler();
            var assembly = compiler.CompileFromFiles("InputFiles/HelloWorld.cs");
            Assert.IsNotNull(assembly);
        }
    }
}
