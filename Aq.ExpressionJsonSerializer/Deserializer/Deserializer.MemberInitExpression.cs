﻿using System;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using Expr = System.Linq.Expressions.Expression;

namespace Aq.ExpressionJsonSerializer
{
    internal partial class Deserializer
    {
        private MemberInitExpression MemberInitExpression(
            ExpressionType nodeType, Type type, JObject obj)
        {
            throw new NotImplementedException();
        }
    }
}