using System;

namespace expressions
{
    public interface IExecutionContext
    {
        string Identifier { get; }
        object Invoke(ExpressionFeatures features, Type[] types, object[] values);
    }
}