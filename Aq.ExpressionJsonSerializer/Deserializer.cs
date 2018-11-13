using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Aq.ExpressionJsonSerializer
{
    internal class Deserializer
    {
        public static Expression Deserialize(JToken token)
        {
            var d = new Deserializer();
            return d.Expression(token);
        }
        
        private static object Deserialize(JToken token, Type type)
        {
            return token.ToObject(type);
        }

        private static T Prop<T>(JObject obj, string name, Func<JToken, T> result)
        {
            var prop = obj.Property(name);
            return result(prop?.Value);
        }

        private static JToken Prop(JObject obj, string name)
        {
            return obj.Property(name).Value;
        }

        private static T Enum<T>(JToken token)
        {
            return (T) System.Enum.Parse(typeof(T), token.Value<string>());
        }

        private static Func<JToken, IEnumerable<T>> Enumerable<T>(Func<JToken, T> result)
        {
            return token =>
            {
                if (token == null || token.Type != JTokenType.Array) return null;
                var array = (JArray) token;
                return array.Select(result);
            };
        }

        private Expression Expression(JToken token)
        {
            if (token == null || token.Type != JTokenType.Object) return null;

            var obj = (JObject) token;
            var nodeType = Prop(obj, "nodeType", Enum<ExpressionType>);
            var type = Prop(obj, "type", Type);
            var typeName = Prop(obj, "typeName", t => t.Value<string>());

            switch (typeName)
            {
                case "binary": return BinaryExpression(nodeType, obj);
                case "block": return BlockExpression(nodeType, type, obj);
                case "conditional": return ConditionalExpression(nodeType, type, obj);
                case "constant": return ConstantExpression(nodeType, type, obj);
                
                case "default": return DefaultExpression(nodeType, type);
                case "index": return IndexExpression(nodeType, obj);
                case "invocation": return InvocationExpression(nodeType, obj);
                case "lambda": return LambdaExpression(nodeType, obj);
                case "member": return MemberExpression(nodeType, obj);
                case "methodCall": return MethodCallExpression(nodeType, obj);
                case "newArray": return NewArrayExpression(nodeType, obj);
                case "new": return NewExpression(nodeType, obj);
                case "parameter": return ParameterExpression(nodeType, type, obj);
                case "runtimeVariables": return RuntimeVariablesExpression(nodeType, obj);
                case "typeBinary": return TypeBinaryExpression(nodeType, obj);
                case "unary": return UnaryExpression(nodeType, type, obj);
                case "goto": 
                case "label": 
                case "listInit": 
                case "loop": 
                case "memberInit": 
                case "switch": 
                case "try": 
                case "dynamic":
                case "debugInfo": 
                    throw new NotImplementedException($"Expression type {typeName} is not implemented yet.");
            }

            throw new NotSupportedException();
        }

        private LambdaExpression LambdaExpression(
            ExpressionType nodeType, JObject obj)
        {
            var body = Prop(obj, "body", Expression);
            var tailCall = Prop(obj, "tailCall").Value<bool>();
            var parameters = Prop(obj, "parameters", Enumerable(ParameterExpression));

            switch (nodeType)
            {
                case ExpressionType.Lambda:
                    return System.Linq.Expressions.Expression.Lambda(body, tailCall, parameters);
                default:
                    throw new NotSupportedException();
            }
        }

        private LambdaExpression LambdaExpression(JToken token)
        {
            if (token == null || token.Type != JTokenType.Object) return null;

            var obj = (JObject) token;
            var nodeType = Prop(obj, "nodeType", Enum<ExpressionType>);
            Prop(obj, "type", Type);
            var typeName = Prop(obj, "typeName", t => t.Value<string>());

            if (typeName != "lambda") return null;

            return LambdaExpression(nodeType, obj);
        }

        private InvocationExpression InvocationExpression(
            ExpressionType nodeType, JObject obj)
        {
            var expression = Prop(obj, "expression", Expression);
            var arguments = Prop(obj, "arguments", Enumerable(Expression));

            switch (nodeType)
            {
                case ExpressionType.Invoke:
                    if (arguments == null) return System.Linq.Expressions.Expression.Invoke(expression);
                    return System.Linq.Expressions.Expression.Invoke(expression, arguments);
                default:
                    throw new NotSupportedException();
            }
        }

        private IndexExpression IndexExpression(ExpressionType nodeType, JObject obj)
        {
            var expression = Prop(obj, "object", Expression);
            var indexer = Prop(obj, "indexer", Property);
            var arguments = Prop(obj, "arguments", Enumerable(Expression));

            switch (nodeType)
            {
                case ExpressionType.Index:
                    return System.Linq.Expressions.Expression.MakeIndex(expression, indexer, arguments);
                default:
                    throw new NotSupportedException();
            }
        }

        private static DefaultExpression DefaultExpression(
            ExpressionType nodeType, Type type)
        {
            switch (nodeType)
            {
                case ExpressionType.Default:
                    return System.Linq.Expressions.Expression.Default(type);
                default:
                    throw new NotSupportedException();
            }
        }

        private BlockExpression BlockExpression(
            ExpressionType nodeType, Type type, JObject obj)
        {
            var expressions = Prop(obj, "expressions", Enumerable(Expression));
            var variables = Prop(obj, "variables", Enumerable(ParameterExpression));

            switch (nodeType)
            {
                case ExpressionType.Block:
                    return System.Linq.Expressions.Expression.Block(type, variables, expressions);
                default:
                    throw new NotSupportedException();
            }
        }

        private BinaryExpression BinaryExpression(
            ExpressionType nodeType, JObject obj)
        {
            var left = Prop(obj, "left", Expression);
            var right = Prop(obj, "right", Expression);
            var method = Prop(obj, "method", Method);
            var conversion = Prop(obj, "conversion", LambdaExpression);
            var liftToNull = Prop(obj, "liftToNull").Value<bool>();

            switch (nodeType)
            {
                case ExpressionType.Add: return System.Linq.Expressions.Expression.Add(left, right, method);
                case ExpressionType.AddAssign: return System.Linq.Expressions.Expression.AddAssign(left, right, method, conversion);
                case ExpressionType.AddAssignChecked: return System.Linq.Expressions.Expression.AddAssignChecked(left, right, method, conversion);
                case ExpressionType.AddChecked: return System.Linq.Expressions.Expression.AddChecked(left, right, method);
                case ExpressionType.And: return System.Linq.Expressions.Expression.And(left, right, method);
                case ExpressionType.AndAlso: return System.Linq.Expressions.Expression.AndAlso(left, right, method);
                case ExpressionType.AndAssign: return System.Linq.Expressions.Expression.AndAssign(left, right, method, conversion);
                case ExpressionType.ArrayIndex: return System.Linq.Expressions.Expression.ArrayIndex(left, right);
                case ExpressionType.Assign: return System.Linq.Expressions.Expression.Assign(left, right);
                case ExpressionType.Coalesce: return System.Linq.Expressions.Expression.Coalesce(left, right, conversion);
                case ExpressionType.Divide: return System.Linq.Expressions.Expression.Divide(left, right, method);
                case ExpressionType.DivideAssign: return System.Linq.Expressions.Expression.DivideAssign(left, right, method, conversion);
                case ExpressionType.Equal: return System.Linq.Expressions.Expression.Equal(left, right, liftToNull, method);
                case ExpressionType.ExclusiveOr: return System.Linq.Expressions.Expression.ExclusiveOr(left, right, method);
                case ExpressionType.ExclusiveOrAssign: return System.Linq.Expressions.Expression.ExclusiveOrAssign(left, right, method, conversion);
                case ExpressionType.GreaterThan: return System.Linq.Expressions.Expression.GreaterThan(left, right, liftToNull, method);
                case ExpressionType.GreaterThanOrEqual: return System.Linq.Expressions.Expression.GreaterThanOrEqual(left, right, liftToNull, method);
                case ExpressionType.LeftShift: return System.Linq.Expressions.Expression.LeftShift(left, right, method);
                case ExpressionType.LeftShiftAssign: return System.Linq.Expressions.Expression.LeftShiftAssign(left, right, method, conversion);
                case ExpressionType.LessThan: return System.Linq.Expressions.Expression.LessThan(left, right, liftToNull, method);
                case ExpressionType.LessThanOrEqual: return System.Linq.Expressions.Expression.LessThanOrEqual(left, right, liftToNull, method);
                case ExpressionType.Modulo: return System.Linq.Expressions.Expression.Modulo(left, right, method);
                case ExpressionType.ModuloAssign: return System.Linq.Expressions.Expression.ModuloAssign(left, right, method, conversion);
                case ExpressionType.Multiply: return System.Linq.Expressions.Expression.Multiply(left, right, method);
                case ExpressionType.MultiplyAssign: return System.Linq.Expressions.Expression.MultiplyAssign(left, right, method, conversion);
                case ExpressionType.MultiplyAssignChecked:
                    return System.Linq.Expressions.Expression.MultiplyAssignChecked(left, right, method, conversion);
                case ExpressionType.MultiplyChecked: return System.Linq.Expressions.Expression.MultiplyChecked(left, right, method);
                case ExpressionType.NotEqual: return System.Linq.Expressions.Expression.NotEqual(left, right, liftToNull, method);
                case ExpressionType.Or: return System.Linq.Expressions.Expression.Or(left, right, method);
                case ExpressionType.OrAssign: return System.Linq.Expressions.Expression.OrAssign(left, right, method, conversion);
                case ExpressionType.OrElse: return System.Linq.Expressions.Expression.OrElse(left, right, method);
                case ExpressionType.Power: return System.Linq.Expressions.Expression.Power(left, right, method);
                case ExpressionType.PowerAssign: return System.Linq.Expressions.Expression.PowerAssign(left, right, method, conversion);
                case ExpressionType.RightShift: return System.Linq.Expressions.Expression.RightShift(left, right, method);
                case ExpressionType.RightShiftAssign: return System.Linq.Expressions.Expression.RightShiftAssign(left, right, method, conversion);
                case ExpressionType.Subtract: return System.Linq.Expressions.Expression.Subtract(left, right, method);
                case ExpressionType.SubtractAssign: return System.Linq.Expressions.Expression.SubtractAssign(left, right, method, conversion);
                case ExpressionType.SubtractAssignChecked:
                    return System.Linq.Expressions.Expression.SubtractAssignChecked(left, right, method, conversion);
                case ExpressionType.SubtractChecked: return System.Linq.Expressions.Expression.SubtractChecked(left, right, method);
                default: throw new NotSupportedException();
            }
        }

        private static ConstantExpression ConstantExpression(
            ExpressionType nodeType, Type type, JObject obj)
        {
            object value;

            var valueTok = Prop(obj, "value");
            if (valueTok == null || valueTok.Type == JTokenType.Null)
            {
                value = null;
            }
            else
            {
                var valueObj = (JObject) valueTok;
                var valueType = Prop(valueObj, "type", Type);
                value = Deserialize(Prop(valueObj, "value"), valueType);
            }

            switch (nodeType)
            {
                case ExpressionType.Constant:
                    return System.Linq.Expressions.Expression.Constant(value, type);
                default:
                    throw new NotSupportedException();
            }
        }

        private RuntimeVariablesExpression RuntimeVariablesExpression(
            ExpressionType nodeType, JObject obj)
        {
            var variables = Prop(obj, "variables", Enumerable(ParameterExpression));

            switch (nodeType)
            {
                case ExpressionType.RuntimeVariables:
                    return System.Linq.Expressions.Expression.RuntimeVariables(variables);
                default:
                    throw new NotSupportedException();
            }
        }

        private static readonly Dictionary<string, Dictionary<string, Dictionary<string, Type>>>
            TypeCache = new Dictionary<string, Dictionary<string, Dictionary<string, Type>>>();

        private static readonly Dictionary<Type, Dictionary<string, Dictionary<string, ConstructorInfo>>>
            ConstructorCache = new Dictionary<Type, Dictionary<string, Dictionary<string, ConstructorInfo>>>();

        private static Type Type(JToken token)
        {
            if (token == null || token.Type != JTokenType.Object) return null;

            var obj = (JObject) token;
            var assemblyName = Prop(obj, "assemblyName", t => t.Value<string>());
            var typeName = Prop(obj, "typeName", t => t.Value<string>());
            var generic = Prop(obj, "genericArguments", Enumerable(Type));

            if (!TypeCache.TryGetValue(assemblyName, out var assemblies))
            {
                assemblies = new Dictionary<string, Dictionary<string, Type>>();
                TypeCache[assemblyName] = assemblies;
            }

            if (!assemblies.TryGetValue(assemblyName, out var types))
            {
                types = new Dictionary<string, Type>();
                assemblies[assemblyName] = types;
            }


            if (!types.TryGetValue(typeName, out var type))
            {
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

            if (generic != null && type.IsGenericTypeDefinition) type = type.MakeGenericType(generic.ToArray());

            return type;
        }

        private static ConstructorInfo Constructor(JToken token)
        {
            if (token == null || token.Type != JTokenType.Object) return null;

            var obj = (JObject) token;
            var type = Prop(obj, "type", Type);
            var name = Prop(obj, "name").Value<string>();
            var signature = Prop(obj, "signature").Value<string>();

            ConstructorInfo constructor;
            Dictionary<string, ConstructorInfo> cache2;

            if (!ConstructorCache.TryGetValue(type, out var cache1))
            {
                constructor = ConstructorInternal(type, name, signature);

                cache2 = new Dictionary<
                    string, ConstructorInfo>(1)
                {
                    {signature, constructor}
                };

                cache1 = new Dictionary<
                    string, Dictionary<
                        string, ConstructorInfo>>(1)
                {
                    {name, cache2}
                };

                ConstructorCache[type] = cache1;
            }
            else if (!cache1.TryGetValue(name, out cache2))
            {
                constructor = ConstructorInternal(type, name, signature);

                cache2 = new Dictionary<
                    string, ConstructorInfo>(1)
                {
                    {signature, constructor}
                };

                cache1[name] = cache2;
            }
            else if (!cache2.TryGetValue(signature, out constructor))
            {
                constructor = ConstructorInternal(type, name, signature);
                cache2[signature] = constructor;
            }

            return constructor;
        }

        private static ConstructorInfo ConstructorInternal(
            Type type, string name, string signature)
        {
            var constructor = type
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(c => c.Name == name && c.ToString() == signature);

            if (constructor == null)
            {
                constructor = type
                    .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(c => c.Name == name && c.ToString() == signature);

                if (constructor == null)
                    throw new Exception(
                        "Constructor for type \""
                        + type.FullName +
                        "\" with signature \""
                        + signature +
                        "\" could not be found"
                    );
            }

            return constructor;
        }

        public MethodInfo Method(JToken token)
        {
            if (token == null || token.Type != JTokenType.Object) return null;

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

            if (generic != null && method.IsGenericMethodDefinition)
                method = method.MakeGenericMethod(generic.ToArray());

            return method;
        }

        private static PropertyInfo Property(JToken token)
        {
            if (token == null || token.Type != JTokenType.Object) return null;

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

        private static MemberInfo Member(JToken token)
        {
            if (token == null || token.Type != JTokenType.Object) return null;

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

        private NewExpression NewExpression(
            ExpressionType nodeType, JObject obj)
        {
            var constructor = Prop(obj, "constructor", Constructor);
            var arguments = Prop(obj, "arguments", Enumerable(Expression));
            var members = Prop(obj, "members", Enumerable(Member));

            switch (nodeType)
            {
                case ExpressionType.New:
                    if (arguments == null)
                    {
                        if (members == null) return System.Linq.Expressions.Expression.New(constructor);
                        return System.Linq.Expressions.Expression.New(constructor, new Expression[0], members);
                    }

                    if (members == null) return System.Linq.Expressions.Expression.New(constructor, arguments);
                    return System.Linq.Expressions.Expression.New(constructor, arguments, members);
                default:
                    throw new NotSupportedException();
            }
        }

        private NewArrayExpression NewArrayExpression(
            ExpressionType nodeType, JObject obj)
        {
            var elementType = Prop(obj, "elementType", Type);
            var expressions = Prop(obj, "expressions", Enumerable(Expression));

            switch (nodeType)
            {
                case ExpressionType.NewArrayInit:
                    return System.Linq.Expressions.Expression.NewArrayInit(elementType, expressions);
                case ExpressionType.NewArrayBounds:
                    return System.Linq.Expressions.Expression.NewArrayBounds(elementType, expressions);
                default:
                    throw new NotSupportedException();
            }
        }

        private MethodCallExpression MethodCallExpression(
            ExpressionType nodeType, JObject obj)
        {
            var instance = Prop(obj, "object", Expression);
            var method = Prop(obj, "method", Method);
            var arguments = Prop(obj, "arguments", Enumerable(Expression));

            switch (nodeType)
            {
                case ExpressionType.ArrayIndex:
                    return System.Linq.Expressions.Expression.ArrayIndex(instance, arguments);
                case ExpressionType.Call:
                    return System.Linq.Expressions.Expression.Call(instance, method, arguments);
                default:
                    throw new NotSupportedException();
            }
        }

        private readonly Dictionary<string, ParameterExpression>
            _parameterExpressions = new Dictionary<string, ParameterExpression>();

        private ParameterExpression ParameterExpression(
            ExpressionType nodeType, Type type, JObject obj)
        {
            var name = Prop(obj, "name", t => t.Value<string>());

            if (_parameterExpressions.TryGetValue(name, out var result)) return result;

            switch (nodeType)
            {
                case ExpressionType.Parameter:
                    result = System.Linq.Expressions.Expression.Parameter(type, name);
                    break;
                default:
                    throw new NotSupportedException();
            }

            _parameterExpressions[name] = result;
            return result;
        }

        private ParameterExpression ParameterExpression(JToken token)
        {
            if (token == null || token.Type != JTokenType.Object) return null;

            var obj = (JObject) token;
            var nodeType = Prop(obj, "nodeType", Enum<ExpressionType>);
            var type = Prop(obj, "type", Type);
            var typeName = Prop(obj, "typeName", t => t.Value<string>());

            if (typeName != "parameter") return null;

            return ParameterExpression(nodeType, type, obj);
        }

        private UnaryExpression UnaryExpression(
            ExpressionType nodeType, Type type, JObject obj)
        {
            var operand = Prop(obj, "operand", Expression);
            var method = Prop(obj, "method", Method);

            switch (nodeType)
            {
                case ExpressionType.ArrayLength: return System.Linq.Expressions.Expression.ArrayLength(operand);
                case ExpressionType.Convert: return System.Linq.Expressions.Expression.Convert(operand, type, method);
                case ExpressionType.ConvertChecked: return System.Linq.Expressions.Expression.ConvertChecked(operand, type, method);
                case ExpressionType.Decrement: return System.Linq.Expressions.Expression.Decrement(operand, method);
                case ExpressionType.Increment: return System.Linq.Expressions.Expression.Increment(operand, method);
                case ExpressionType.IsFalse: return System.Linq.Expressions.Expression.IsFalse(operand, method);
                case ExpressionType.IsTrue: return System.Linq.Expressions.Expression.IsTrue(operand, method);
                case ExpressionType.Negate: return System.Linq.Expressions.Expression.Negate(operand, method);
                case ExpressionType.NegateChecked: return System.Linq.Expressions.Expression.NegateChecked(operand, method);
                case ExpressionType.Not: return System.Linq.Expressions.Expression.Not(operand, method);
                case ExpressionType.OnesComplement: return System.Linq.Expressions.Expression.OnesComplement(operand, method);
                case ExpressionType.PostDecrementAssign: return System.Linq.Expressions.Expression.PostDecrementAssign(operand, method);
                case ExpressionType.PostIncrementAssign: return System.Linq.Expressions.Expression.PostIncrementAssign(operand, method);
                case ExpressionType.PreDecrementAssign: return System.Linq.Expressions.Expression.PreDecrementAssign(operand, method);
                case ExpressionType.PreIncrementAssign: return System.Linq.Expressions.Expression.PreIncrementAssign(operand, method);
                case ExpressionType.Quote: return System.Linq.Expressions.Expression.Quote(operand);
                case ExpressionType.Throw: return System.Linq.Expressions.Expression.Throw(operand, type);
                case ExpressionType.TypeAs: return System.Linq.Expressions.Expression.TypeAs(operand, type);
                case ExpressionType.UnaryPlus: return System.Linq.Expressions.Expression.UnaryPlus(operand, method);
                case ExpressionType.Unbox: return System.Linq.Expressions.Expression.Unbox(operand, type);
                default: throw new NotSupportedException();
            }
        }

        private TypeBinaryExpression TypeBinaryExpression(
            ExpressionType nodeType, JObject obj)
        {
            var expression = Prop(obj, "expression", Expression);
            var typeOperand = Prop(obj, "typeOperand", Type);

            switch (nodeType)
            {
                case ExpressionType.TypeIs:
                    return System.Linq.Expressions.Expression.TypeIs(expression, typeOperand);
                case ExpressionType.TypeEqual:
                    return System.Linq.Expressions.Expression.TypeEqual(expression, typeOperand);
                default:
                    throw new NotSupportedException();
            }
        }

        private MemberExpression MemberExpression(
            ExpressionType nodeType, JObject obj)
        {
            var expression = Prop(obj, "expression", Expression);
            var member = Prop(obj, "member", Member);

            switch (nodeType)
            {
                case ExpressionType.MemberAccess:
                    return System.Linq.Expressions.Expression.MakeMemberAccess(expression, member);
                default:
                    throw new NotSupportedException();
            }
        }

        private ConditionalExpression ConditionalExpression(
            ExpressionType nodeType, Type type, JObject obj)
        {
            var test = Prop(obj, "test", Expression);
            var ifTrue = Prop(obj, "ifTrue", Expression);
            var ifFalse = Prop(obj, "ifFalse", Expression);

            switch (nodeType)
            {
                case ExpressionType.Conditional:
                    return System.Linq.Expressions.Expression.Condition(test, ifTrue, ifFalse, type);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}