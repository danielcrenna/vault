using System;

namespace expressions
{
    [Flags]
    public enum ExpressionFeatures : byte
    {
        None = 0,

        /// <summary> Assemblies are cached by expression </summary>
        L1Cache = 1 << 0,

        /// <summary> Method handlers are cached by expression and value types </summary>
        L2Cache = 1 << 1,

        /// <summary> Loads compiled assemblies into separate app domain, to avoid leaking memory </summary>
        IsolateCompilation = 1 << 2,

        /// <summary> Compiled expressions use dynamic binding to parameters rather than type permutation </summary>
        UseDynamicBinding = 1 << 3,

        All = byte.MaxValue,
    }
}