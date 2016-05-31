namespace ICSharpCode.SharpZipLib.Silverlight.Serialization
{
    /// <summary>
    /// Code originally authored by Rockford Lhotka:
    /// http://www.lhotka.net/weblog/SilverlightSerialization.aspx,
    /// presented here with minor naming and code changes.
    /// </summary>
    public interface ISerializable
    {
        void Serialize(SerializationInfo info, XmlFormatter formatter);
        void Deserialize(SerializationInfo info, XmlFormatter formatter);
    }
}