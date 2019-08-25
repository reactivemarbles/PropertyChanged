// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;

namespace ReactiveMarbles.PropertyChanged
{
    /// <summary>
    /// Provides extension methods for the notify property changed extensions.
    /// </summary>
    public static class NotifyPropertyChangedExtensions
    {
        /// <summary>
        /// Notifies when the specified property changes.
        /// </summary>
        /// <param name="objectToMonitor">The object to monitor.</param>
        /// <param name="propertyExpression">The expression to the object.</param>
        /// <typeparam name="TObj">The type of initial object.</typeparam>
        /// <typeparam name="TReturn">The eventual return value.</typeparam>
        /// <returns>An observable that signals when the property specified in the expression has changed.</returns>
        /// <exception cref="ArgumentNullException">Either the property expression or the object to monitor is null.</exception>
        /// <exception cref="ArgumentException">If there is an issue with the property expression.</exception>
        public static IObservable<TReturn> WhenPropertyChanges<TObj, TReturn>(
            this TObj objectToMonitor,
            Expression<Func<TObj, TReturn>> propertyExpression)
            where TObj : class, INotifyPropertyChanged
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            var currentObservable = Observable.Return((INotifyPropertyChanged)objectToMonitor);

            var expressionChain = GetExpressionChain(propertyExpression.Body);

            if (expressionChain.Count == 0)
            {
                throw new ArgumentException("There are no fields in the expressions", nameof(propertyExpression));
            }

            int i = 0;
            foreach (var currentExpression in expressionChain)
            {
                var memberExpression = (MemberExpression)currentExpression;

                if (i == expressionChain.Count - 1)
                {
                    return currentObservable
                        .Where(parent => parent != null)
                        .Select(parent => GenerateObservable<TReturn>(parent, memberExpression))
                        .Switch();
                }

                currentObservable = currentObservable
                    .Where(parent => parent != null)
                    .Select(parent => GenerateObservable<INotifyPropertyChanged>(parent, memberExpression))
                    .Switch();

                i++;
            }

            throw new ArgumentException("Invalid expression", nameof(propertyExpression));
        }

        private static IObservable<T> GenerateObservable<T>(INotifyPropertyChanged parent, MemberExpression memberExpression)
        {
            var memberInfo = memberExpression.Member;
            var memberName = memberInfo.Name;

            var func = GetValueCache<T>.GetCache(parent.GetType(), memberInfo);
            return Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    handler =>
                    {
                        void Handler(object sender, PropertyChangedEventArgs e) => handler(e);
                        return Handler;
                    },
                    x => parent.PropertyChanged += x,
                    x => parent.PropertyChanged -= x)
                .Where(x => x.PropertyName == memberName)
                .Select(x => func.Invoke(parent))
                .StartWith(func.Invoke(parent));
        }

        private static List<Expression> GetExpressionChain(Expression expression)
        {
            var expressions = new List<Expression>(16);

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

        private static class GetValueCache<TReturn>
        {
#if !UIKIT
            private static readonly
                ConcurrentDictionary<Type, Func<INotifyPropertyChanged, TReturn>> Cache
                    = new ConcurrentDictionary<Type, Func<INotifyPropertyChanged, TReturn>>();
#endif

            [SuppressMessage("Design", "CA1801: Parameter not used", Justification = "Used on some platforms")]
            public static Func<INotifyPropertyChanged, TReturn> GetCache(Type fromType, MemberInfo memberInfo)
            {
#if UIKIT
                switch (memberInfo)
                {
                    case PropertyInfo propertyInfo:
                        return input => (TReturn)propertyInfo.GetValue(input);
                    case FieldInfo fieldInfo:
                        return input => (TReturn)fieldInfo.GetValue(input);
                    default:
                        throw new ArgumentException($"Cannot handle member {memberInfo.Name}", nameof(memberInfo));
                }
#else
                return Cache.GetOrAdd(fromType, typeFrom =>
                {
                    var instance = Expression.Parameter(typeof(INotifyPropertyChanged), "instance");

                    var castInstance = Expression.Convert(instance, typeFrom);

                    Expression body;

                    switch (memberInfo)
                    {
                        case PropertyInfo propertyInfo:
                            body = Expression.Call(castInstance, propertyInfo.GetGetMethod());
                            break;
                        case FieldInfo fieldInfo:
                            body = Expression.Field(castInstance, fieldInfo);
                            break;
                        default:
                            throw new ArgumentException($"Cannot handle member {memberInfo.Name}", nameof(memberInfo));
                    }

                    var parameters = new[] { instance };

                    var lambdaExpression = Expression.Lambda<Func<INotifyPropertyChanged, TReturn>>(body, parameters);

                    return lambdaExpression.Compile();
                });
#endif
            }
        }
    }
}