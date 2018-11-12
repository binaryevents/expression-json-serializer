﻿using System;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using Expr = System.Linq.Expressions.Expression;

namespace Aq.ExpressionJsonSerializer
{
    partial class Deserializer
    {
        private IndexExpression IndexExpression(
            ExpressionType nodeType, Type type, JObject obj)
        {
            var expression = Prop(obj, "object", Expression);
            var indexer = Prop(obj, "indexer", Property);
            var arguments = Prop(obj, "arguments", Enumerable(Expression));

            switch (nodeType) {
                case ExpressionType.Index:
                    return Expr.MakeIndex(expression, indexer, arguments);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
