﻿using System;
using System.Linq.Expressions;

namespace Aq.ExpressionJsonSerializer.Serializer
{
    internal partial class Serializer
    {
        private bool GotoExpression(Expression expr)
        {
            var expression = expr as GotoExpression;
            if (expression == null) return false;

            throw new NotImplementedException();
        }
    }
}