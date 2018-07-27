using System;

namespace expressions
{
    [Flags]
    public enum ExpressionFeatures : byte
    {
        None = 0,

        /// <summary> Assemblies are cached by expression. </summary>
        L1Cache = 1 << 0,

        /// <summary> Method handlers are cached by expression and value types. </summary>
        L2Cache = 1 << 1,

        /// <summary> Uses dynamic method signatures, rather than static. This may decrease boxing costs over the long run, but is slightly slower. </summary>
        UseDynamic  = 1 << 2,

        /// <summary> Loads compiled assemblies into separate app domain sandbox, to avoid leaking memory. </summary>
        IsolateCompilation = 1 << 3,

        /// <summary> Currently best performing options </summary>
        Default = L1Cache | L2Cache | UseDynamic,
    }
}