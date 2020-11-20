// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
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
        public static IObservable<TReturn> WhenPropertyValueChanges<TObj, TReturn>(
            this TObj objectToMonitor,
            Expression<Func<TObj, TReturn>> propertyExpression)
            where TObj : class, INotifyPropertyChanged
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            return WhenPropertyChanges(objectToMonitor, propertyExpression).Select(x => x.Value);
        }

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
        public static IObservable<(object Sender, TReturn Value)> WhenPropertyChanges<TObj, TReturn>(
            this TObj objectToMonitor,
            Expression<Func<TObj, TReturn>> propertyExpression)
            where TObj : class, INotifyPropertyChanged
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            IObservable<(object Sender, INotifyPropertyChanged Value)> currentObservable = Observable.Return(((object)null, (INotifyPropertyChanged)objectToMonitor));

            var expressionChain = propertyExpression.Body.GetExpressionChain();

            if (expressionChain.Count == 0)
            {
                throw new ArgumentException("There are no properties in the expressions", nameof(propertyExpression));
            }

            var i = 0;
            foreach (var memberExpression in expressionChain)
            {
                var memberInfo = memberExpression.Member;

                if (i == expressionChain.Count - 1)
                {
                    var function = GetMemberFuncCache<INotifyPropertyChanged, TReturn>.GetCache(memberInfo);
                    return currentObservable
                        .Where(parent => parent.Value != null)
                        .Select(parent => GenerateObservable(parent.Value, memberInfo, function))
                        .Switch();
                }

                var iFunction = GetMemberFuncCache<INotifyPropertyChanged, INotifyPropertyChanged>.GetCache(memberInfo);
                currentObservable = currentObservable
                    .Where(parent => parent.Value != null)
                    .Select(parent => GenerateObservable(parent.Value, memberInfo, iFunction))
                    .Switch();

                i++;
            }

            throw new ArgumentException("Invalid expression", nameof(propertyExpression));
        }

        private static IObservable<(object Sender, T Value)> GenerateObservable<T>(
            INotifyPropertyChanged parent,
            MemberInfo memberInfo,
            Func<INotifyPropertyChanged, T> getter)
        {
            var memberName = memberInfo.Name;
            return Observable.FromEvent<PropertyChangedEventHandler, (object Sender, PropertyChangedEventArgs Args)>(
                    handler =>
                    {
                        void Handler(object sender, PropertyChangedEventArgs e) => handler((sender, e));
                        return Handler;
                    },
                    x => parent.PropertyChanged += x,
                    x => parent.PropertyChanged -= x)
                .Where(x => x.Args.PropertyName == memberName)
                .Select(x => (x.Sender, getter(parent)))
                .StartWith((parent, getter(parent)));
        }
    }
}