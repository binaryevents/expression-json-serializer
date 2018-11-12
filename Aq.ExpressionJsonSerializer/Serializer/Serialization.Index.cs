using System.Linq.Expressions;

namespace Aq.ExpressionJsonSerializer.Serializer
{
    internal partial class Serializer
    {
        private bool IndexExpression(Expression expr)
        {
            var expression = expr as IndexExpression;
            if (expression == null) return false;

            Prop("typeName", "index");
            Prop("object", Expression(expression.Object));
            Prop("indexer", Property(expression.Indexer));
            Prop("arguments", Enumerable(expression.Arguments, Expression));

            return true;
        }
    }
}