using System.Reflection;

namespace Hammock.Attributes
{
    public interface IValidatingAttribute
    {
        string TransformValue(PropertyInfo property, object value);
    }
}