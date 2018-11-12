using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Aq.ExpressionJsonSerializer
{
    partial class Deserializer
    {
        private static readonly Dictionary<string, Dictionary<string, Dictionary<string, Type>>>
            TypeCache = new Dictionary<string, Dictionary<string, Dictionary<string, Type>>>();

        private static readonly Dictionary<Type, Dictionary<string, Dictionary<string, ConstructorInfo>>>
            ConstructorCache = new Dictionary<Type, Dictionary<string, Dictionary<string, ConstructorInfo>>>();

        private Type Type(JToken token)
        {
            if (token == null || token.Type != JTokenType.Object) {
                return null;
            }

            var obj = (JObject) token;
            var assemblyName = Prop(obj, "assemblyName", t => t.Value<string>());
            var typeName = Prop(obj, "typeName", t => t.Value<string>());
            var generic = Prop(obj, "genericArguments", Enumerable(Type));

            if (!TypeCache.TryGetValue(assemblyName, out var assemblies)) {
                assemblies = new Dictionary<string, Dictionary<string, Type>>();
                TypeCache[assemblyName] = assemblies;
            }

            if (!assemblies.TryGetValue(assemblyName, out var types)) {
                types = new Dictionary<string, Type>();
                assemblies[assemblyName] = types;
            }


            if (!types.TryGetValue(typeName, out var type)) {
                var assembly = Assembly.Load(new AssemblyName(assemblyName));
                type = assembly.GetType(typeName);

                if (type != null)
                    types[typeName] = type;
                else
                    throw new Exception( 
                        "Type could not be found: "
                        + assemblyName + "." + typeName
                    );
            }

            if (generic != null && type.IsGenericTypeDefinition) {
                type = type.MakeGenericType(generic.ToArray());
            }

            return type;
        }

        private ConstructorInfo Constructor(JToken token)
        {
            if (token == null || token.Type != JTokenType.Object) {
                return null;
            }

            var obj = (JObject) token;
            var type = Prop(obj, "type", Type);
            var name = Prop(obj, "name").Value<string>();
            var signature = Prop(obj, "signature").Value<string>();

            ConstructorInfo constructor;
            Dictionary<string, ConstructorInfo> cache2;

            if (!ConstructorCache.TryGetValue(type, out var cache1)) {
                constructor = ConstructorInternal(type, name, signature);
                
                cache2 = new Dictionary<
                    string, ConstructorInfo>(1) {
                        {signature, constructor}
                    };

                cache1 = new Dictionary<
                    string, Dictionary<
                        string, ConstructorInfo>>(1) {
                            {name, cache2}
                        };
                
                ConstructorCache[type] = cache1;
            }
            else if (!cache1.TryGetValue(name, out cache2)) {
                constructor = ConstructorInternal(type, name, signature);
                
                cache2 = new Dictionary<
                    string, ConstructorInfo>(1) {
                        {signature, constructor}
                    };

                cache1[name] = cache2;
            }
            else if (!cache2.TryGetValue(signature, out constructor)) {
                constructor = ConstructorInternal(type, name, signature);
                cache2[signature] = constructor;
            }

            return constructor;
        }

        private ConstructorInfo ConstructorInternal(
            Type type, string name, string signature)
        {
            var constructor = type
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(c => c.Name == name && c.ToString() == signature);
            
            if (constructor == null) {
                constructor = type
                    .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(c => c.Name == name && c.ToString() == signature);
                
                if (constructor == null) {
                    throw new Exception(
                        "Constructor for type \""
                        + type.FullName +
                        "\" with signature \""
                        + signature +
                        "\" could not be found"
                    );
                }
            }

            return constructor;
        }

        private MethodInfo Method(JToken token)
        {
            if (token == null || token.Type != JTokenType.Object) {
                return null;
            }

            var obj = (JObject) token;
            var type = Prop(obj, "type", Type);
            var name = Prop(obj, "name").Value<string>();
            var signature = Prop(obj, "signature").Value<string>();
            var generic = Prop(obj, "generic", Enumerable(Type));

            var methods = type.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static
            );
            var method = methods.First(m => m.Name == name && m.ToString() == signature);

            if (generic != null && method.IsGenericMethodDefinition) {
                method = method.MakeGenericMethod(generic.ToArray());
            }

            return method;
        }

        private PropertyInfo Property(JToken token)
        {
            if (token == null || token.Type != JTokenType.Object) {
                return null;
            }

            var obj = (JObject) token;
            var type = Prop(obj, "type", Type);
            var name = Prop(obj, "name").Value<string>();
            var signature = Prop(obj, "signature").Value<string>();

            var properties = type.GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static
            );
            return properties.First(p => p.Name == name && p.ToString() == signature);
        }

        private MemberInfo Member(JToken token)
        {
            if (token == null || token.Type != JTokenType.Object) {
                return null;
            }

            var obj = (JObject) token;
            var type = Prop(obj, "type", Type);
            var name = Prop(obj, "name").Value<string>();
            var signature = Prop(obj, "signature").Value<string>();
            var memberType = (MemberTypes) Prop(obj, "memberType").Value<int>();

            var members = type.GetMembers(
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static
            );
            return members.First(p => p.MemberType == memberType
                && p.Name == name && p.ToString() == signature);
        }
    }
}
