using System.Dynamic;
using DotLiquid;

namespace email
{
    /// <summary>
    /// Allows a dynamic hash to access values that don't exist
    /// </summary>
    internal class SafeHash : DynamicObject
    {
        private readonly Hash _child;
        public SafeHash(Hash child)
        {
            _child = child;
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_child.ContainsKey(binder.Name))
            {
                _child[binder.Name] = value;
            }
            return true;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = _child.ContainsKey(binder.Name) ? _child[binder.Name] : null;
            return true;
        }
    }
}