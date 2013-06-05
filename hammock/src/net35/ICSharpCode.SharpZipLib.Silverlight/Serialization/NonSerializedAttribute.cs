using System;

namespace ICSharpCode.SharpZipLib.Silverlight.Serialization
{
    /// <summary>
    /// Code originally authored by Rockford Lhotka:
    /// http://www.lhotka.net/weblog/SilverlightSerialization.aspx,
    /// presented here with minor naming and code changes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NonSerializedAttribute : Attribute
    {
        //
    }
}