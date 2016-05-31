using System;

namespace cohort
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class FormName : Attribute
    {
        public FormName(string alias)
        {
            Alias = alias;
        }
        public string Alias { get; private set; }
    }
}