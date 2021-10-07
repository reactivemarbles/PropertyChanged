// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ReactiveMarbles.PropertyChanged;

internal static class ExpressionExtensions
{
    private static readonly ConcurrentDictionary<string, object> _actionCache =
        new();

    internal static List<MemberExpression> GetExpressionChain(this Expression expression)
    {
        var expressions = new List<MemberExpression>(16);

        var node = expression;

        while (node.NodeType != ExpressionType.Parameter)
        {
            switch (node.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression)node;
                    expressions.Add(memberExpression);
                    node = memberExpression.Expression;
                    break;
                default:
                    throw new NotSupportedException($"Unsupported expression type: '{node.NodeType.ToString()}'");
            }
        }

        expressions.Reverse();

        return expressions;
    }

    internal static Action<T, TProperty> GetSetter<T, TProperty>(this Expression<Func<T, TProperty>> expression)
        => (Action<T, TProperty>)_actionCache.GetOrAdd(
            $"{typeof(T).FullName}|{typeof(TProperty).FullName}|{expression}",
            _ =>
            {
                var instanceParameter = expression.Parameters.Single();
                var valueParameter = Expression.Parameter(typeof(TProperty), "value");

                return Expression.Lambda<Action<T, TProperty>>(
                        Expression.Assign(expression.Body, valueParameter),
                        instanceParameter,
                        valueParameter)
                    .Compile();
            });
}
