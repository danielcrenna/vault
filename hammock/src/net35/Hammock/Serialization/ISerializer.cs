using System;
using System.Text;

namespace Hammock.Serialization
{
    public interface ISerializer
    {
        string Serialize(object instance, Type type);
        string ContentType { get; }
        Encoding ContentEncoding { get; }
    }
}