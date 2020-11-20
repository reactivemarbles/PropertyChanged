// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ReactiveMarbles.PropertyChanged.Benchmarks.Legacy
{
    internal static class ExpressionExtensions
    {
        internal static object GetParentForExpression(this List<Func<object, object>> chain, object startItem)
        {
            var current = startItem;
            foreach (var valueFetcher in chain)
            {
                current = valueFetcher.Invoke(current);
            }

            return current;
        }

        internal static List<Func<object, object>> GetGetValueMemberChain(this Expression expression)
        {
            var returnValue = new List<Func<object, object>>();
            var expressionChain = expression.GetExpressionChain();

            foreach (var value in expressionChain.Take(expressionChain.Count - 1))
            {
                var valueFetcher = GetMemberFuncCache<object, object>.GetCache(value.Member);

                returnValue.Add(valueFetcher);
            }

            return returnValue;
        }

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
                        throw new NotSupportedException($"Unsupported expression type: '{node.NodeType}'");
                }
            }

            expressions.Reverse();

            return expressions;
        }
    }
}