using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace ebnf
{
    public class CodeDomCompiler : Compiler
    {
        private readonly CSharpCodeProvider _provider;
        private readonly CompilerParameters _parameters;

        public CodeDomCompiler()
        {
            _provider = new CSharpCodeProvider();
            _parameters = new CompilerParameters { GenerateInMemory = true };
        }

        public Assembly CompileFromFiles(params string[] files)
        {
            var sources = new string[files.Length];
            for(var i = 0; i < files.Length; i++)
            {
                sources[i] = File.ReadAllText(files[i]);
            }
            return CompileFromSources(sources);
        }

        public Assembly CompileFromSources(params string[] sources)
        {
            var results = _provider.CompileAssemblyFromSource(_parameters, sources);
            if (results.Errors.HasErrors)
            {
                var sb = new StringBuilder();
                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine(error.ToString());
                }
            }
            return results.CompiledAssembly;
        }
    }
}