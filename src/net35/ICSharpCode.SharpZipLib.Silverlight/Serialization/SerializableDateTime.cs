using System;

namespace ICSharpCode.SharpZipLib.Silverlight.Serialization
{
    [Serializable]
    public class SerializableDateTime : SerializableBase
    {
        public DateTime DateTime { get; set; }
       
        public SerializableDateTime(int year, int month, int day)
        {
            DateTime = new DateTime(year, month, day);
        }

        public SerializableDateTime()
        {
            
        }

        protected override object GetValue(System.Reflection.FieldInfo field)
        {
            return field.DeclaringType == typeof (SerializableDateTime) ? field.GetValue(this) : null;
        }

        protected override void SetValue(System.Reflection.FieldInfo field, object value)
        {
            if (field.DeclaringType == typeof(SerializableDateTime))
            {
                field.SetValue(this, value);
            }
        }

        public bool Equals(SerializableDateTime other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            return ReferenceEquals(this, other) || Equals(other.DateTime, DateTime);
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
            return other.GetType() == typeof (SerializableDateTime) && Equals((SerializableDateTime) other);
        }

        public override int GetHashCode()
        {
            return DateTime.GetHashCode();
        }
    }
}