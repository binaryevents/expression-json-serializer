﻿using System.Linq.Expressions;

namespace Aq.ExpressionJsonSerializer.Serializer
{
    internal partial class Serializer
    {
        private bool ConditionalExpression(Expression expr)
        {
            var expression = expr as ConditionalExpression;
            if (expression == null) return false;

            Prop("typeName", "conditional");
            Prop("test", Expression(expression.Test));
            Prop("ifTrue", Expression(expression.IfTrue));
            Prop("ifFalse", Expression(expression.IfFalse));

            return true;
        }
    }
}