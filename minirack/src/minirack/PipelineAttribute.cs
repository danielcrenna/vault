using System;
using System.Web;

namespace minirack
{
    /// <summary>
    /// Apply this attribute to an <see cref="IHttpModule"/> to have it registered dynamically.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PipelineAttribute : Attribute
    {
        public int Order { get; set; }
    }
}