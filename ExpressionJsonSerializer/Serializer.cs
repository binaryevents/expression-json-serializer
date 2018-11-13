using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;

namespace binaryevents.ExpressionJsonSerializer
{
    internal class Serializer
    {
        private readonly JsonSerializer _serializer;
        private readonly JsonWriter _writer;

        private Serializer(JsonWriter writer, JsonSerializer serializer)
        {
            _writer = writer;
            _serializer = serializer;
        }

        public static void Serialize(
            JsonWriter writer,
            JsonSerializer serializer,
            Expression expression)
        {
            var s = new Serializer(writer, serializer);
            s.ExpressionInternal(expression);
        }

        private void Serialize(Expression expression)
        {
            switch (expression)
            {
                case BinaryExpression exp:
                    BinaryExpression(exp);
                    return;
                case BlockExpression exp:
                    BlockExpression(exp);
                    return;
                case ConditionalExpression exp:
                    ConditionalExpression(exp);
                    return;
                case ConstantExpression exp:
                    ConstantExpression(exp);
                    return;
                case DefaultExpression exp:
                    DefaultExpression(exp);
                    return;
                case IndexExpression exp:
                    IndexExpression(exp);
                    return;
                case InvocationExpression exp:
                    InvocationExpression(exp);
                    return;
                case LambdaExpression exp:
                    LambdaExpression(exp);
                    return;
                case MemberExpression exp:
                    MemberExpression(exp);
                    return;
                case MethodCallExpression exp:
                    MethodCallExpression(exp);
                    return;
                case NewArrayExpression exp:
                    NewArrayExpression(exp);
                    return;
                case NewExpression exp:
                    NewExpression(exp);
                    return;
                case ParameterExpression exp:
                    ParameterExpression(exp);
                    return;
                case RuntimeVariablesExpression exp:
                    RuntimeVariablesExpression(exp);
                    return;
                case TypeBinaryExpression exp:
                    TypeBinaryExpression(exp);
                    return;
                case UnaryExpression exp:
                    UnaryExpression(exp);
                    return;
                default:
                    throw new NotImplementedException($"Expression of type {expression.GetType().Name} can not be serialized yet.");
            }
        }

        private Action Serialize(object value, Type type)
        {
            return () => _serializer.Serialize(_writer, value, type);
        }

        private void Prop(string name, bool value)
        {
            _writer.WritePropertyName(name);
            _writer.WriteValue(value);
        }

        private void Prop(string name, int value)
        {
            _writer.WritePropertyName(name);
            _writer.WriteValue(value);
        }

        private void Prop(string name, string value)
        {
            _writer.WritePropertyName(name);
            _writer.WriteValue(value);
        }

        private void Prop(string name, Action valueWriter)
        {
            _writer.WritePropertyName(name);
            valueWriter();
        }

        private Action Enum<TEnum>(TEnum value)
        {
            return () => EnumInternal(value);
        }

        private void EnumInternal<TEnum>(TEnum value)
        {
            _writer.WriteValue(System.Enum.GetName(typeof(TEnum), value));
        }

        private Action Enumerable<T>(IEnumerable<T> items, Func<T, Action> func)
        {
            return () => EnumerableInternal(items, func);
        }

        private Action Expression(Expression expression)
        {
            return () => ExpressionInternal(expression);
        }

        private void EnumerableInternal<T>(IEnumerable<T> items, Func<T, Action> func)
        {
            if (items == null)
            {
                _writer.WriteNull();
            }
            else
            {
                _writer.WriteStartArray();
                foreach (var item in items) func(item)();
                _writer.WriteEndArray();
            }
        }

        private void ExpressionInternal(Expression expression)
        {
            if (expression == null)
            {
                _writer.WriteNull();
                return;
            }

            while (expression.CanReduce) expression = expression.Reduce();

            _writer.WriteStartObject();

            Prop("nodeType", Enum(expression.NodeType));
            Prop("type", Type(expression.Type));

            Serialize(expression);
            _writer.WriteEndObject();
        }

        private void UnaryExpression(UnaryExpression expression)
        {
            Prop("typeName", "unary");
            Prop("operand", Expression(expression.Operand));
            Prop("method", Method(expression.Method));
        }

        private void ConstantExpression(ConstantExpression expression)
        {
            Prop("typeName", "constant");
            if (expression.Value == null)
                Prop("value", () => _writer.WriteNull());
            else
            {
                var value = expression.Value;
                var type = value.GetType();
                Prop("value", () =>
                {
                    _writer.WriteStartObject();
                    Prop("type", Type(type));
                    Prop("value", Serialize(value, type));
                    _writer.WriteEndObject();
                });
            }
        }

        private void ConditionalExpression(ConditionalExpression expression)
        {
            Prop("typeName", "conditional");
            Prop("test", Expression(expression.Test));
            Prop("ifTrue", Expression(expression.IfTrue));
            Prop("ifFalse", Expression(expression.IfFalse));
        }

        private void BlockExpression(BlockExpression expression)
        {
            Prop("typeName", "block");
            Prop("expressions", Enumerable(expression.Expressions, Expression));
            Prop("variables", Enumerable(expression.Variables, Expression));
         
        }

        private void BinaryExpression(BinaryExpression expression)
        {
            Prop("typeName", "binary");
            Prop("left", Expression(expression.Left));
            Prop("right", Expression(expression.Right));
            Prop("method", Method(expression.Method));
            Prop("conversion", Expression(expression.Conversion));
            Prop("liftToNull", expression.IsLiftedToNull);
        }

        private static readonly Dictionary<Type, Tuple<string, string, Type[]>>
            TypeCache = new Dictionary<Type, Tuple<string, string, Type[]>>();

        private Action Type(Type type)
        {
            return () => TypeInternal(type);
        }

        private void TypeInternal(Type type)
        {
            if (type == null)
            {
                _writer.WriteNull();
            }
            else
            {
                if (!TypeCache.TryGetValue(type, out var tuple))
                {
                    var assemblyName = type.Assembly.FullName;
                    if (type.IsGenericType)
                    {
                        var def = type.GetGenericTypeDefinition();
                        tuple = new Tuple<string, string, Type[]>(
                            def.Assembly.FullName, def.FullName,
                            type.GetGenericArguments()
                        );
                    }
                    else
                    {
                        tuple = new Tuple<string, string, Type[]>(
                            assemblyName, type.FullName, null);
                    }

                    TypeCache[type] = tuple;
                }

                _writer.WriteStartObject();
                Prop("assemblyName", tuple.Item1);
                Prop("typeName", tuple.Item2);
                Prop("genericArguments", Enumerable(tuple.Item3, Type));
                _writer.WriteEndObject();
            }
        }

        private Action Constructor(ConstructorInfo constructor)
        {
            return () => ConstructorInternal(constructor);
        }

        private void ConstructorInternal(ConstructorInfo constructor)
        {
            if (constructor == null)
            {
                _writer.WriteNull();
            }
            else
            {
                _writer.WriteStartObject();
                Prop("type", Type(constructor.DeclaringType));
                Prop("name", constructor.Name);
                Prop("signature", constructor.ToString());
                _writer.WriteEndObject();
            }
        }

        private Action Method(MethodInfo method)
        {
            return () => MethodInternal(method);
        }

        private void MethodInternal(MethodInfo method)
        {
            if (method == null)
            {
                _writer.WriteNull();
            }
            else
            {
                _writer.WriteStartObject();
                if (method.IsGenericMethod)
                {
                    var meth = method.GetGenericMethodDefinition();
                    var generic = method.GetGenericArguments();

                    Prop("type", Type(meth.DeclaringType));
                    Prop("name", meth.Name);
                    Prop("signature", meth.ToString());
                    Prop("generic", Enumerable(generic, Type));
                }
                else
                {
                    Prop("type", Type(method.DeclaringType));
                    Prop("name", method.Name);
                    Prop("signature", method.ToString());
                }

                _writer.WriteEndObject();
            }
        }

        private Action Property(PropertyInfo property)
        {
            return () => PropertyInternal(property);
        }

        private void PropertyInternal(PropertyInfo property)
        {
            if (property == null)
            {
                _writer.WriteNull();
            }
            else
            {
                _writer.WriteStartObject();
                Prop("type", Type(property.DeclaringType));
                Prop("name", property.Name);
                Prop("signature", property.ToString());
                _writer.WriteEndObject();
            }
        }

        private Action Member(MemberInfo member)
        {
            return () => MemberInternal(member);
        }

        private void MemberInternal(MemberInfo member)
        {
            if (member == null)
            {
                _writer.WriteNull();
            }
            else
            {
                _writer.WriteStartObject();
                Prop("type", Type(member.DeclaringType));
                Prop("memberType", (int) member.MemberType);
                Prop("name", member.Name);
                Prop("signature", member.ToString());
                _writer.WriteEndObject();
            }
        }

        private void TypeBinaryExpression(TypeBinaryExpression expression)
        {
            Prop("typeName", "typeBinary");
            Prop("expression", Expression(expression.Expression));
            Prop("typeOperand", Type(expression.TypeOperand));
        }


        private void DefaultExpression(DefaultExpression expression)
        {
            Prop("typeName", "default");
        }

        private void IndexExpression(IndexExpression expression)
        {
            Prop("typeName", "index");
            Prop("object", Expression(expression.Object));
            Prop("indexer", Property(expression.Indexer));
            Prop("arguments", Enumerable(expression.Arguments, Expression));
        }

        private void InvocationExpression(InvocationExpression expression)
        {
            Prop("typeName", "invocation");
            Prop("expression", Expression(expression.Expression));
            Prop("arguments", Enumerable(expression.Arguments, Expression));
        }

        private void LambdaExpression(LambdaExpression expression)
        {
            Prop("typeName", "lambda");
            Prop("name", expression.Name);
            Prop("parameters", Enumerable(expression.Parameters, Expression));
            Prop("body", Expression(expression.Body));
            Prop("tailCall", expression.TailCall);
        }

        private void MemberExpression(MemberExpression expression)
        {
            
            Prop("typeName", "member");
            Prop("expression", Expression(expression.Expression));
            Prop("member", Member(expression.Member));
        }

        private void NewExpression(NewExpression expression)
        {
            
            Prop("typeName", "new");
            Prop("constructor", Constructor(expression.Constructor));
            Prop("arguments", Enumerable(expression.Arguments, Expression));
            Prop("members", Enumerable(expression.Members, Member));
        }

        private void NewArrayExpression(NewArrayExpression expression)
        {
            
            Prop("typeName", "newArray");
            Prop("elementType", Type(expression.Type.GetElementType()));
            Prop("expressions", Enumerable(expression.Expressions, Expression));
        }

        private void RuntimeVariablesExpression(RuntimeVariablesExpression expression)
        {
            Prop("typeName", "runtimeVariables");
            Prop("variables", Enumerable(expression.Variables, Expression));
        }
        
        

        private void MethodCallExpression(MethodCallExpression expression)
        {
            Prop("typeName", "methodCall");
            Prop("object", Expression(expression.Object));
            Prop("method", Method(expression.Method));
            Prop("arguments", Enumerable(expression.Arguments, Expression));
        }

        private readonly Dictionary<ParameterExpression, string>
            _parameterExpressions = new Dictionary<ParameterExpression, string>();

        private void ParameterExpression(ParameterExpression expression)
        {
            if (!_parameterExpressions.TryGetValue(expression, out var name))
            {
                name = expression.Name ?? "p_" + Guid.NewGuid().ToString("N");
                _parameterExpressions[expression] = name;
            }

            Prop("typeName", "parameter");
            Prop("name", name);
        }

    }
}