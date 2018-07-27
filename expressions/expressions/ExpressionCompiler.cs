using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
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
            Template.RegisterFilter(typeof (WhitelistFilter));
        }

        public static IExecutionContext Compile(string expression, string[] parameterNames, TraceSource trace,
            ExpressionFeatures features, string @namespace = "MyNamespace", AppDomain domain = null)
        {
            const string ls = "{{";
            const string le = "}}";

            string expressionLine = $"{ls}expression{le}";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"using System.ComponentModel;");
            sb.AppendLine();
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine($"{{");
            sb.AppendLine($"    [Description(\"{expression}\")]");
            sb.AppendLine($"    public static class Expression");
            sb.AppendLine($"    {{");

            EmitMethodSignatures(features, sb, expressionLine, parameterNames, trace);

            sb.AppendLine($"    }}");
            sb.AppendLine($"}}");
            string source = sb.ToString();

            Template template = Template.Parse(source);
            string code = template.Render(Hash.FromAnonymousObject(new
            {
                expression
            }));

            trace?.TraceInformation($"Compiling expression: {expression}");
            trace?.TraceInformation(code);
            trace?.Flush();

            // Note: GenerateInMemory still creates an assembly on disk!
            CompilerParameters p = new CompilerParameters
            {
                WarningLevel = 4,
                GenerateExecutable = false,
                GenerateInMemory = domain == null,
                CompilerOptions = "/optimize"
            };

            p.ReferencedAssemblies.Add(typeof (DescriptionAttribute).Assembly.Location);

            if (features.HasFlag(ExpressionFeatures.UseDynamicBinding))
            {
                p.ReferencedAssemblies.Add(typeof (DynamicAttribute).Assembly.Location);
                p.ReferencedAssemblies.Add(typeof (Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location);
            }

            CodeDomProvider compiler = new CSharpCodeProvider();
            CompilerResults r = compiler.CompileAssemblyFromSource(p, code);

            if (r.Errors.Count > 0)
            {
                trace?.TraceEvent(TraceEventType.Error, 500, "Compiler errors encountered");
                foreach (CompilerError error in r.Errors.OfType<CompilerError>().Where(e => e.IsWarning))
                    trace?.TraceEvent(TraceEventType.Warning, 500, error.ToString());
                foreach (CompilerError error in r.Errors.OfType<CompilerError>().Where(e => !e.IsWarning))
                    trace?.TraceEvent(TraceEventType.Error, 500, error.ToString());
            }

            // load into AppDomain.Current
            if (domain == null || domain == AppDomain.CurrentDomain)
                return new LocalExecutor(r.CompiledAssembly);

            // load assembly into isolated app domain (without leaking into this one)
            RemoteExecutor remote =
                (RemoteExecutor)
                    domain.CreateInstanceAndUnwrap(typeof (RemoteExecutor).Assembly.FullName,
                        typeof (RemoteExecutor).FullName);

            remote.LoadFrom(r.PathToAssembly);
            return remote;
        }

        private static void EmitMethodSignatures(ExpressionFeatures features, StringBuilder sb, string expressionLine,
            string[] parameterNames, TraceSource trace)
        {
            Func<string, string> parameterLine = type => EmitExpressionParameters(type, parameterNames);

            if (parameterNames.Length == 0)
            {
                // Nothing:
                sb.AppendLine($"        public static object Evaluate()");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            return {expressionLine};");
                sb.AppendLine($"        }}");
            }
            else
            {
                if (features.HasFlag(ExpressionFeatures.UseDynamicBinding))
                {
                    // Dynamic:
                    sb.AppendLine($"        public static object Evaluate({parameterLine("dynamic")})");
                    sb.AppendLine($"        {{");
                    sb.AppendLine($"            return {expressionLine};");
                    sb.AppendLine($"        }}");
                }
                else
                {
                    // Permutations:
                    EmitTypePermutation(sb, expressionLine, parameterNames, TypePermutations1);
                }
            }
        }

        private static void EmitTypePermutation(StringBuilder sb, string expressionLine, string[] parameterNames, Type[] permutationTypes)
        {
            IEnumerable<IEnumerable<int>> permutations = M(parameterNames.Length, 0, permutationTypes.Length - 1);
            foreach (IEnumerable<int> permutation in permutations)
            {
                IReadOnlyList<Type> types = permutation.Select(i => permutationTypes[i]).ToList();

                sb.AppendLine($"        public static object Evaluate({BuildPermutationParameters(parameterNames, types)})");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            return {expressionLine};");
                sb.AppendLine($"        }}");
            }
        }

        private static string BuildPermutationParameters(string[] parameterNames, IReadOnlyList<Type> types)
        {
            int i = 0;
            var spool = new StringBuilder();
            for (var j = 0; j < types.Count; j++)
            {
                spool.Append(i < parameterNames.Length - 1
                    ? $"{types[j]} {parameterNames[i]}, "
                    : $"{types[j]} {parameterNames[i]}");
                i++;
            }
            return spool.ToString();
        }

        private static readonly Type[] TypePermutations1 = { typeof (sbyte), typeof (short), typeof (int), typeof (long), typeof(double) };
        
        #region Permutations (source: http://stackoverflow.com/a/4326669)

        private static IEnumerable<T> Prepend<T>(T first, IEnumerable<T> rest)
        {
            yield return first;
            foreach (var item in rest)
                yield return item;
        }

        private static IEnumerable<IEnumerable<int>> M(int p, int t1, int t2)
        {
            if (p == 0)
                yield return Enumerable.Empty<int>();
            else
                for (int first = t1; first <= t2; ++first)
                    foreach (var rest in M(p - 1, first, t2))
                        yield return Prepend(first, rest);
        }

        #endregion

        private static string EmitExpressionParameters(string type, IReadOnlyList<string> parameterNames)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < parameterNames.Count; i++)
            {
                var parameter = parameterNames[i];
                if (i < parameterNames.Count - 1)
                    sb.Append($"{type} {parameter}, ");
                else
                    sb.Append($"{type} {parameter}");
            }

            return sb.ToString();
        }

        public static class WhitelistFilter
        {
            public static string Textilize(string input)
            {
                return input;
            }
        }

        public abstract class ExecutionBase : IExecutionContext
        {
            protected readonly Lazy<MethodCache> MethodCache =
                new Lazy<MethodCache>(() => new MethodCache(new ConcurrentDictionary<string, MethodInfo>()));

            public abstract string Identifier { get; }
            public abstract object Invoke(ExpressionFeatures features, Type[] types, object[] values);
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
        }

        public class RemoteExecutor : MarshalByRefObject, IExecutionContext
        {
            protected readonly Lazy<MethodCache> MethodCache =
                new Lazy<MethodCache>(() => new MethodCache(new ConcurrentDictionary<string, MethodInfo>()));

            private Type _evaluatorType;

            public void Load(string path)
            {
                var a = Assembly.Load(path);
                Identifier = a.FullName;
                _evaluatorType = a.GetType(EvaluateMethod);
            }

            public void LoadFrom(string path)
            {
                var a = Assembly.LoadFrom(path);
                Identifier = a.FullName;
                _evaluatorType = a.GetType(EvaluatorType);
            }

            public string Identifier { get; private set; }

            public Type GetEvaluatorType()
            {
                return _evaluatorType;
            }

            public MethodInfo GetMethod(Type[] types)
            {
                Type evaluatorType = GetEvaluatorType();
                MethodInfo method = evaluatorType.GetMethod(EvaluateMethod, types);
                return method;
            }

            public object Invoke(ExpressionFeatures features, Type[] types, object[] values)
            {
                MethodInfo eval = GetEvaluationMethod(features, types);
                object result = eval.Invoke(null, values);
                return result;
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
        }
    }
}
