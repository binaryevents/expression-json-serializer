using System.Linq.Expressions;

namespace Aq.ExpressionJsonSerializer
{
    internal partial class Serializer
    {
        private bool DefaultExpression(Expression expr)
        {
            var expression = expr as DefaultExpression;
            if (expression == null) return false;

            Prop("typeName", "default");

            return true;
        }
    }
}