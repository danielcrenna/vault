using System;

namespace Hammock.Silverlight.Compat
{
    [Flags]
    public enum DecompressionMethods
    {
        Deflate = 2,
        GZip = 4,
        None = 6
    }
}
