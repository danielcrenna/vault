using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace expressions
{
    public class ExpressionService : IDisposable
    {
        private static readonly string[] NoParameters = new string[0];
        private static readonly object[] NoValues = new object[0];
        private static readonly Type[] NoTypes = new Type[0];

        private readonly ExpressionFeatures _features;

        #region Feature Storage 

        private readonly Lazy<AssemblyCache> _assemblyCache = new Lazy<AssemblyCache>(() => new AssemblyCache(new ConcurrentDictionary<string, IExecutionContext>()));

        private readonly Lazy<AppDomain> _container = new Lazy<AppDomain>(() =>
        {
            string friendlyName = Guid.NewGuid().ToString();

            return AppDomain.CreateDomain(friendlyName, AppDomain.CurrentDomain.Evidence, new AppDomainSetup
            {
                ApplicationName = friendlyName,
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                ConfigurationFile = $"{friendlyName}.dll.config"
            });
        });
        
        public void Dispose()
        {
            if (_assemblyCache.IsValueCreated)
                _assemblyCache.Value.Clear();

            if (_container.IsValueCreated)
                AppDomain.Unload(_container.Value);
        }

        #endregion

        public ExpressionService(ExpressionFeatures features = ExpressionFeatures.Default)
        {
            _features = features;
        }

        public object Evaluate(string expression, string[] parameterNames, object[] values, Type[] types)
        {
            parameterNames = parameterNames ?? NoParameters;
            values = values ?? NoValues;
            types = types ?? NoTypes;

            Debug.Assert(parameterNames.Length == values.Length && values.Length == types.Length);
            IExecutionContext executor = GetOrCompile(expression, parameterNames);
            object result = executor.Invoke(_features, types, values);
            return result;
        }
        
        private IExecutionContext GetOrCompile(string expression, string[] parameterNames)
        {
            AppDomain domain = null;

            // Isolation:
            if (_features.HasFlag(ExpressionFeatures.IsolateCompilation))
                domain = _container.Value;

            // L1 Cache: Assembly (expression is key)
            if (_features.HasFlag(ExpressionFeatures.L1Cache))
            {
                IExecutionContext assembly;
                return !_assemblyCache.Value.TryGetAssembly(expression, out assembly)
                    ? _assemblyCache.Value.Add(expression, ExpressionCompiler.Compile(_features, expression, parameterNames, domain))
                    : assembly;
            }

            return ExpressionCompiler.Compile(_features, expression, parameterNames, domain);
        }
    }
}