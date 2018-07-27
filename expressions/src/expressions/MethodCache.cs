using System.Collections.Generic;
using System.Reflection;

namespace expressions
{
    public class MethodCache
    {
        private readonly IDictionary<string, MethodInfo> _storage;

        public MethodCache(IDictionary<string, MethodInfo> storage)
        {
            _storage = storage;
        }

        public bool TryGetMethod(string key, out MethodInfo method)
        {
            return _storage.TryGetValue(key, out method);
        }

        public MethodInfo Add(string key, MethodInfo method)
        {
            _storage.Add(key, method);
            return method;
        }

        public void Clear()
        {
            _storage.Clear();
        }
    }
}