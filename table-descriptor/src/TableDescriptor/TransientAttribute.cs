using System;

namespace TableDescriptor
{
    /// <summary>
    /// The database has no concept of this column, so don't try to insert it
    /// <remarks>
    /// Use this for object-only computed, read-only properties, and generally anything that's not in the database
    /// </remarks>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class TransientAttribute : Attribute
    {

    }
}