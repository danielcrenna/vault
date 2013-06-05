using System.Reflection;

namespace ebnf
{
    public interface Compiler
    {
        Assembly CompileFromFiles(params string[] files);
        Assembly CompileFromSources(params string[] sources);
    }
}