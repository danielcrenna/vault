using System.Collections.Generic;

namespace expressions
{
    public class AssemblyCache
    {
        private readonly IDictionary<string, IExecutionContext> _storage;

        public AssemblyCache(IDictionary<string, IExecutionContext> storage)
        {
            _storage = storage;
        }

        public bool TryGetAssembly(string expression, out IExecutionContext assembly)
        {
            return _storage.TryGetValue(expression, out assembly);
        }

        public IExecutionContext Add(string expression, IExecutionContext assembly)
        {
            _storage.Add(expression, assembly);

            return assembly;
        }

        public void Clear()
        {
            _storage.Clear();
        }
    }
}