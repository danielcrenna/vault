using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DotLiquid;
using Microsoft.CSharp;

namespace expressions
{
    public static class ExpressionCompiler
    {
        private const string EvaluatorType = "Expressions.Expression";
        private const string EvaluateMethod = "Evaluate";

        static ExpressionCompiler()
        {
            InternalFunctions.Register();
        }
        
        public static IExecutionContext Compile(ExpressionFeatures features, string expression, string[] parameterNames, AppDomain domain = null)
        {
            string expressionLine = $"{{{{ '{expression}' | inline_functions | clean }}}}";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"namespace Expressions");
            sb.AppendLine($"{{");
            sb.AppendLine($"    public static class Expression");
            sb.AppendLine($"    {{");

            EmitMethodSignatures(features, sb, expressionLine, parameterNames);

            sb.AppendLine($"    }}");
            sb.AppendLine($"}}");
            string source = sb.ToString();

            Template template = Template.Parse(source);
            string code = template.Render(Hash.FromAnonymousObject(new
            {
                expression
            }));

            // Note: GenerateInMemory still creates an assembly on disk!
            CompilerParameters p = new CompilerParameters
            {
                WarningLevel = 4,
                GenerateExecutable = false,
                GenerateInMemory = domain == null,
                CompilerOptions = "/optimize"
            };

            if (features.HasFlag(ExpressionFeatures.UseDynamic))
            {
                p.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
                p.ReferencedAssemblies.Add("System.Core.dll");
            }

            CodeDomProvider compiler = new CSharpCodeProvider();
            CompilerResults r = compiler.CompileAssemblyFromSource(p, code);

            // load into AppDomain.Current
            if (domain == null || domain == AppDomain.CurrentDomain)
                return new LocalExecutor(r.CompiledAssembly);

            // load assembly into another app domain (without leaking into this one)
            RemoteExecutor remote = (RemoteExecutor)domain.CreateInstanceAndUnwrap(typeof(RemoteExecutor).Assembly.FullName, typeof (RemoteExecutor).FullName);
            remote.LoadFrom(r.PathToAssembly);
            return remote;
        }

        public abstract class ExecutionBase : IExecutionContext
        {
            protected readonly Lazy<MethodCache> MethodCache = new Lazy<MethodCache>(() => new MethodCache(new ConcurrentDictionary<string, MethodInfo>()));
            public abstract string Identifier { get; }
            public abstract object Invoke(ExpressionFeatures features, Type[] types, object[] values);
            public abstract void Dispose();
        }

        public class LocalExecutor : ExecutionBase
        {
            private readonly Assembly _assembly;

            public LocalExecutor(Assembly assembly)
            {
                _assembly = assembly;
            }

            public override string Identifier => _assembly.FullName;

            public override object Invoke(ExpressionFeatures features, Type[] types, object[] values)
            {
                MethodInfo eval = GetEvaluationMethod(features, types);
                object result = eval.Invoke(null, values);
                return result;
            }

            private Type GetEvaluatorType()
            {
                return _assembly.GetType(EvaluatorType);
            }

            private MethodInfo GetMethod(Type[] types)
            {
                Type evaluatorType = GetEvaluatorType();
                MethodInfo method = evaluatorType.GetMethod(EvaluateMethod, types);
                return method;
            }

            private MethodInfo GetEvaluationMethod(ExpressionFeatures features, Type[] types)
            {
                // L2 Cache: MethodInfo (expression + value types are key)
                if (features.HasFlag(ExpressionFeatures.L2Cache))
                {
                    string key = $"{Identifier}_{types.Select(t => t.Name)}";
                    MethodInfo eval;
                    if (MethodCache.Value.TryGetMethod(key, out eval))
                        return eval;
                    return MethodCache.Value.Add(key, GetMethod(types));
                }

                return GetMethod(types);
            }

            public override void Dispose()
            {
                if (MethodCache.IsValueCreated)
                    MethodCache.Value.Clear();
            }
        }

        public class RemoteExecutor : MarshalByRefObject, IExecutionContext
        {
            protected readonly Lazy<MethodCache> MethodCache = new Lazy<MethodCache>(() => new MethodCache(new ConcurrentDictionary<string, MethodInfo>()));

            private Type _evaluatorType;
            private string _path;

            public void LoadFrom(string path)
            {
                var a = Assembly.LoadFrom(path);
                Identifier = a.FullName;
                _evaluatorType = a.GetType(EvaluatorType);
                _path = path;
            }

            public string Identifier { get; private set; }

            public Type GetEvaluatorType()
            {
                return _evaluatorType;
            }

            public object Invoke(ExpressionFeatures features, Type[] types, object[] values)
            {
                MethodInfo eval = GetEvaluationMethod(features, types);
                object result = eval.Invoke(null, values);
                return result;
            }

            private MethodInfo GetEvaluationMethod(ExpressionFeatures features, Type[] types)
            {
                // L2 Cache: MethodInfo
                if (features.HasFlag(ExpressionFeatures.L2Cache))
                {
                    string key = $"{Identifier}_{types.Select(t => t.Name)}";
                    MethodInfo eval;
                    if (MethodCache.Value.TryGetMethod(key, out eval))
                        return eval;
                    return MethodCache.Value.Add(key, GetMethod(types));
                }

                return GetMethod(types);
            }

            private MethodInfo GetMethod(Type[] types)
            {
                Type evaluatorType = GetEvaluatorType();
                MethodInfo method = evaluatorType.GetMethod(EvaluateMethod, types);
                return method;
            }

            public void Dispose()
            {
                try
                {
                    if(MethodCache.IsValueCreated)
                        MethodCache.Value.Clear();

                    if (!string.IsNullOrWhiteSpace(_path) && File.Exists(_path))
                        File.Delete(_path);
                } catch { }
            }
        }
        
        private static void EmitMethodSignatures(ExpressionFeatures features, StringBuilder sb, string expressionLine, string[] parameterNames)
        {
            Func<string, string> parameterLine = type => EmitExpressionParameters(type, parameterNames);
            
            if (parameterNames.Length == 0)
            {
                // Nothing:
                sb.AppendLine($"        public static object Evaluate()");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            return {expressionLine};");
                sb.AppendLine($"        }}");

                return;
            }

            if (features.HasFlag(ExpressionFeatures.UseDynamic))
            {
                // Dynamic:
                sb.AppendLine($"        public static dynamic Evaluate({parameterLine("dynamic")})");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            return {expressionLine};");
                sb.AppendLine($"        }}");

                return;
            }

            // Byte:
            sb.AppendLine($"        public static object Evaluate({parameterLine("byte")})");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            return {expressionLine};");
            sb.AppendLine($"        }}");

            // Int16:
            sb.AppendLine($"        public static object Evaluate({parameterLine("short")})");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            return {expressionLine};");
            sb.AppendLine($"        }}");

            // Int32:
            sb.AppendLine($"        public static object Evaluate({parameterLine("int")})");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            return {expressionLine};");
            sb.AppendLine($"        }}");

            // Int64:
            sb.AppendLine($"        public static object Evaluate({parameterLine("long")})");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            return {expressionLine};");
            sb.AppendLine($"        }}");

            // Single:
            sb.AppendLine($"        public static object Evaluate({parameterLine("float")})");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            return {expressionLine};");
            sb.AppendLine($"        }}");

            // Double:
            sb.AppendLine($"        public static object Evaluate({parameterLine("double")})");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            return {expressionLine};");
            sb.AppendLine($"        }}");

            // Decimal:
            sb.AppendLine($"        public static object Evaluate({parameterLine("decimal")})");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            return {expressionLine};");
            sb.AppendLine($"        }}");
        }

        private static string EmitExpressionParameters(string type, string[] parameterNames)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < parameterNames.Length; i++)
            {
                var parameter = parameterNames[i];
                if(i < parameterNames.Length - 1)
                    sb.Append($"{type} {parameter}, ");
                else
                    sb.Append($"{type} {parameter}");
            }

            return sb.ToString();
        }
    }
}
