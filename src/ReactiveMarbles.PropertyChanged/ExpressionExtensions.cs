// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace ReactiveMarbles.PropertyChanged
{
    internal static class ExpressionExtensions
    {
        internal static INotifyPropertyChanged GetParentForExpression(this Expression expression, INotifyPropertyChanged startItem)
        {
            var expressionChain = expression.GetExpressionChain();

            var current = startItem;
            foreach (var value in expressionChain.Take(expressionChain.Count - 1))
            {
                var valueFetcher = GetMemberFuncCache<object>.GetCache(value.Type, value.Member);

                current = (INotifyPropertyChanged)valueFetcher.Invoke(current);
            }

            return current;
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