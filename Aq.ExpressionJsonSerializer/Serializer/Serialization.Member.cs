﻿using System.Linq.Expressions;

namespace Aq.ExpressionJsonSerializer.Serializer
{
    internal partial class Serializer
    {
        private bool MemberExpression(Expression expr)
        {
            var expression = expr as MemberExpression;
            if (expression == null) return false;

            Prop("typeName", "member");
            Prop("expression", Expression(expression.Expression));
            Prop("member", Member(expression.Member));

            return true;
        }
    }
}