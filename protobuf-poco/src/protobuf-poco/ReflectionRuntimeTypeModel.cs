using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ProtoBuf.Meta;

namespace protobuf.Poco
{
    public class ReflectionRuntimeTypeModel
    {
        private static int _field = 1;

        public static RuntimeTypeModel Create()
        {
            var model = TypeModel.Create();
            model.AutoAddMissingTypes = true;
            model.AutoCompile = true;
            return model;
        }

        public static void AddType<T>(RuntimeTypeModel model, params Assembly[] assemblies)
        {
            var type = typeof(T);
            var meta = AddProperties(model, type);
            AddSubTypes(type, meta, model, assemblies);
        }

        private static void AddSubTypes(Type parentType, MetaType metaType, RuntimeTypeModel typeModel, IEnumerable<Assembly> assemblies)
        {
            foreach (var childType in GetBaseTypesOfParentInAssemblies(parentType, assemblies))
            {
                var subModel = typeModel.Add(childType, true);
                metaType.AddSubType(_field, childType);
                Interlocked.Increment(ref _field);
                AddSubTypes(parentType, subModel, typeModel, assemblies);
            }
        }

        private static IEnumerable<Type> GetBaseTypesOfParentInAssemblies(Type parentType, IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(assembly => assembly.GetTypes().Where(t => t.BaseType != null && t.BaseType == parentType));
        }

        private static MetaType AddProperties(RuntimeTypeModel model, Type type)
        {
            var @base = model.Add(type, false);
            foreach (var property in type.GetProperties())
            {
                @base.Add(_field, property.Name);
                Interlocked.Increment(ref _field);
            }
            return @base;
        }
    }
}