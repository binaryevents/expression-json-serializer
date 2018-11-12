using System;
using System.Linq.Expressions;

namespace Aq.ExpressionJsonSerializer.Serializer
{
    internal partial class Serializer
    {
        private bool LoopExpression(Expression expr)
        {
            var expression = expr as DefaultExpression;
            if (expression == null) return false;

            throw new NotImplementedException();
        }
    }
}