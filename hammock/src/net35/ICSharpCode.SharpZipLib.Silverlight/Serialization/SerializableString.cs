namespace ICSharpCode.SharpZipLib.Silverlight.Serialization
{
    [Serializable]
    public class SerializableString : SerializableBase
    {
        public string String { get; set; }

        public SerializableString (string value)
        {
            String = value;
        }

        public SerializableString()
        {
            
        }

        protected override object GetValue(System.Reflection.FieldInfo field)
        {
            return field.DeclaringType == typeof (SerializableString) ? field.GetValue(this) : null;
        }

        public bool Equals(SerializableString other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            return ReferenceEquals(this, other) || Equals(other.String, String);
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return other.GetType() == typeof (SerializableString) && Equals((SerializableString) other);
        }

        public override int GetHashCode()
        {
            return (String != null ? String.GetHashCode() : 0);
        }

        protected override void SetValue(System.Reflection.FieldInfo field, object value)
        {
            if (field.DeclaringType == typeof(SerializableString))
            {
                field.SetValue(this, value);
            }
        }
    }
}