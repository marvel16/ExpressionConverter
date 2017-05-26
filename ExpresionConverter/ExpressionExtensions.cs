using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpresionConverter
{
    public static class ExpressionExtensions
    {
        public static BlockExpression ForEach(this Expression enumerable, Action<ParameterExpression, ICollection<Expression>> getBodyContent)
        {
            var getEnumeratorCall = Expression.Call(enumerable, nameof(IEnumerable.GetEnumerator), Type.EmptyTypes);
            var enumeratorVar = Expression.Variable(getEnumeratorCall.Type, "enumerator");
            var currentPropertyInfo = enumeratorVar.Type.GetProperty(nameof(IEnumerator.Current));
            var loopVar = Expression.Parameter(currentPropertyInfo.PropertyType, "item");
            var bodyExpressions = new List<Expression> { Expression.Assign(loopVar, Expression.Property(enumeratorVar, currentPropertyInfo)) };
            getBodyContent(loopVar, bodyExpressions);

            var breakLabel = Expression.Label("loopBreak");
            return Expression.Block(new[] { enumeratorVar },
                Expression.Assign(enumeratorVar, getEnumeratorCall),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Call(enumeratorVar, nameof(IEnumerator.MoveNext), Type.EmptyTypes),
                        Expression.Block(new[] { loopVar }, bodyExpressions),
                        Expression.Break(breakLabel)),
                breakLabel)
            );
        }
    }
}
