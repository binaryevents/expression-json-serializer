﻿using System;
using System.Linq.Expressions;

namespace Aq.ExpressionJsonSerializer
{
    partial class Serializer
    {
        private bool TypeBinaryExpression(Expression expr)
        {
            var expression = expr as TypeBinaryExpression;
            if (expression == null) { return false; }

            Prop("typeName", "typeBinary");
            Prop("expression", Expression(expression.Expression));
            Prop("typeOperand", Type(expression.TypeOperand));

            return true;
        }
    }
}
