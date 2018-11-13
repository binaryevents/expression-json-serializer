﻿using System.Linq.Expressions;

namespace Aq.ExpressionJsonSerializer.Serializer
{
    internal partial class Serializer
    {
        private bool ConstantExpression(Expression expr)
        {
            var expression = expr as ConstantExpression;
            if (expression == null) return false;

            Prop("typeName", "constant");
            if (expression.Value == null)
            {
                Prop("value", () => _writer.WriteNull());
            }
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

            return true;
        }
    }
}