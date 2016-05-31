namespace TableDescriptor
{
    public class PropertyToColumn
    {
        public PropertyAccessor Property { get; private set; }
        public string ColumnName { get; private set; }
        public bool IsComputed { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsKey { get; set; }

        public PropertyToColumn(PropertyAccessor typedPropertyDescriptor)
        {
            Property = typedPropertyDescriptor;
            ColumnName = Property.Name;
            Property.Name = Property.Name; // TODO allow for this to differ
        }
    }
}