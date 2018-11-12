using System;
using System.Linq.Expressions;

namespace Aq.ExpressionJsonSerializer.Serializer
{
    internal partial class Serializer
    {
        private bool DebugInfoExpression(Expression expr)
        {
            var expression = expr as ConditionalExpression;
            if (expression == null) return false;

            throw new NotImplementedException();
        }
    }
}