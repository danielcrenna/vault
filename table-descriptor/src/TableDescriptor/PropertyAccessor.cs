using System;
using System.Collections.Generic;
using System.Linq;

namespace TableDescriptor
{
    public class PropertyAccessor
    {
        private readonly TypeAccessor _accessor;
        public string Name { get; set; }
        public Type Type { get; set; }
        public IEnumerable<Attribute> Attributes { get; set; }
        
        public PropertyAccessor(TypeAccessor accessor, Type type, string name)
        {
            Type = type;
            _accessor = accessor;
            Name = name;
            ScanAttributes(accessor, name);
        }

        private void ScanAttributes(TypeAccessor accessor, string name)
        {
            var pi = accessor.PropertyInfos.SingleOrDefault(p => p.Name == name);
            if (pi == null) return;
            Attributes = pi.GetCustomAttributes(true).Cast<Attribute>();
        }

        public object Get(dynamic example)
        {
            var result = _accessor[example, Name];
            return result;
        }

        public void Set(object instance, object value)
        {
            _accessor[instance, Name] = value;
        }
    }
}