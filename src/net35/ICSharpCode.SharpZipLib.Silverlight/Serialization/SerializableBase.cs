using System;
using System.Reflection;

namespace ICSharpCode.SharpZipLib.Silverlight.Serialization
{
    /// <summary>
    /// Code originally authored by Rockford Lhotka:
    /// http://www.lhotka.net/weblog/SilverlightSerialization.aspx,
    /// presented here with minor naming and code changes.
    /// </summary>
    [Serializable]
    public abstract class SerializableBase : ISerializable
    {
        #region ISerializable Members

        void ISerializable.Serialize(SerializationInfo info, XmlFormatter formatter)
        {
            var thisType = GetType();
            info.TypeName = string.Format("{0},{1}", thisType.FullName, thisType.Assembly.FullName);
            Serialize(info, formatter);
        }

        void ISerializable.Deserialize(SerializationInfo info, XmlFormatter formatter)
        {
            Deserialize(info, formatter);
        }

        #endregion

        protected virtual void Serialize(SerializationInfo info, XmlFormatter formatter)
        {
            var currentType = GetType();

            while (currentType != null)
            {
                var fields = currentType.GetFields(
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.Public);

                foreach (var field in fields)
                {
                    if (field.IsNotSerialized || IsNonSerialized(field))
                    {
                        continue;
                    }

                    var value = GetValue(field);
                    var mobile = value as ISerializable;
                    info.AddValue(
                        string.Format("{0}!{1}", field.DeclaringType.Name, field.Name),
                        mobile == null ? value : formatter.SerializeObject(mobile)); 
                }
                currentType = currentType.BaseType;
            }
        }

        private static bool IsNonSerialized(ICustomAttributeProvider field)
        {
            var a = field.GetCustomAttributes(typeof(NonSerializedAttribute), false);
            return a.Length > 0;
        }

        protected virtual void Deserialize(SerializationInfo info, XmlFormatter formatter)
        {
            var currentType = GetType();

            while (currentType != null)
            {
                // get the list of fields in this type
                var fields = currentType.GetFields(
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.Public);

                foreach (var field in fields)
                {
                    // see if this field is marked as not undoable
                    if (field.IsNotSerialized || IsNonSerialized(field))
                    {
                        continue;
                    }

                    var value = info.GetValue(string.Format("{0}!{1}", field.DeclaringType.Name, field.Name));
                    var valueInfo = value as SerializationInfo;
                    if (valueInfo == null)
                    {
                        SetValue(field, Convert.ChangeType(value, field.FieldType, null));
                    }
                    else
                    {
                        SetValue(field, formatter.GetObject(valueInfo.ReferenceId));
                    }
                }
                currentType = currentType.BaseType;
            }
        }

        protected abstract void SetValue(FieldInfo field, object value);
        protected abstract object GetValue(FieldInfo field);
    }
}