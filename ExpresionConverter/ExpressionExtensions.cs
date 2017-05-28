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

        public static BlockExpression ForEach(this Expression enumerable, Action<ParameterExpression, ParameterExpression, ICollection<Expression>> getBodyContent)
        {
            var getEnumeratorCall = Expression.Call(enumerable, nameof(IEnumerable.GetEnumerator), Type.EmptyTypes);
            var enumeratorVar = Expression.Variable(getEnumeratorCall.Type, "enumerator");
            var currentPropertyInfo = enumeratorVar.Type.GetProperty(nameof(IEnumerator.Current));
            var loopVar = Expression.Parameter(currentPropertyInfo.PropertyType, "item");
            var bodyExpressions = new List<Expression> { Expression.Assign(loopVar, Expression.Property(enumeratorVar, currentPropertyInfo)) };
            var index = Expression.Variable(typeof(int), "index");
            Expression.Assign(index, Expression.Constant(0, typeof(int)));

            getBodyContent(index, loopVar, bodyExpressions);

            bodyExpressions.Add(Expression.PostIncrementAssign(index));

            var breakLabel = Expression.Label("loopBreak");
            return Expression.Block(new[] { enumeratorVar, index },
                Expression.Assign(enumeratorVar, getEnumeratorCall),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Call(enumeratorVar, nameof(IEnumerator.MoveNext), Type.EmptyTypes),
                        Expression.Block(new[] { loopVar }, bodyExpressions),
                        Expression.Break(breakLabel)),
                breakLabel)
            );
        }

        private static readonly Expression<Action<string>> cw = x => Console.WriteLine(x);
        public static Expression WriteLine(string msg)
        {
           return Expression.Invoke(cw, Expression.Constant(msg));
        }

        public static Expression WriteLine(Expression expr)
        {
            return Expression.Invoke(cw, Expression.Call(expr, typeof(string).GetMethod(nameof(string.ToString))));

        }
    }
}
