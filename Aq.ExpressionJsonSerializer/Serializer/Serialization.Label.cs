﻿using System;
using System.Linq.Expressions;

namespace Aq.ExpressionJsonSerializer.Serializer
{
    internal partial class Serializer
    {
        private bool LabelExpression(Expression expr)
        {
            var expression = expr as LabelExpression;
            if (expression == null) return false;

            throw new NotImplementedException();
        }
    }
}