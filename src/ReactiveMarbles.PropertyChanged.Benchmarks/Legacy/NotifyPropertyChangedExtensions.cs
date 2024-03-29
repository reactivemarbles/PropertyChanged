﻿// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace ReactiveMarbles.PropertyChanged.Benchmarks.Legacy
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
        public static IObservable<TReturn> WhenChanged<TObj, TReturn>(
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

            System.Collections.Generic.List<MemberExpression> expressionChain = propertyExpression.Body.GetExpressionChain();

            if (expressionChain.Count == 0)
            {
                throw new ArgumentException("There are no fields in the expressions", nameof(propertyExpression));
            }

            int i = 0;
            foreach (MemberExpression memberExpression in expressionChain)
            {
                if (i == expressionChain.Count - 1)
                {
                    return currentObservable
                        .Where(parent => parent.Value != null)
                        .Select(parent => GenerateObservable<TReturn>(parent.Value, memberExpression))
                        .Switch();
                }

                currentObservable = currentObservable
                    .Where(parent => parent.Value != null)
                    .Select(parent => GenerateObservable<INotifyPropertyChanged>(parent.Value, memberExpression))
                    .Switch();

                i++;
            }

            throw new ArgumentException("Invalid expression", nameof(propertyExpression));
        }

        private static IObservable<(object Sender, T Value)> GenerateObservable<T>(INotifyPropertyChanged parent, MemberExpression memberExpression)
        {
            System.Reflection.MemberInfo memberInfo = memberExpression.Member;
            string memberName = memberInfo.Name;

            Func<INotifyPropertyChanged, T> func = GetMemberFuncCache<INotifyPropertyChanged, T>.GetCache(memberInfo);
            return Observable.FromEvent<PropertyChangedEventHandler, (object Sender, PropertyChangedEventArgs Args)>(
                    handler =>
                    {
                        void Handler(object sender, PropertyChangedEventArgs e)
                        {
                            handler((sender, e));
                        }

                        return Handler;
                    },
                    x => parent.PropertyChanged += x,
                    x => parent.PropertyChanged -= x)
                .Where(x => x.Args.PropertyName == memberName)
                .Select(x => (x.Sender, func.Invoke(parent)))
                .StartWith((parent, func.Invoke(parent)));
        }
    }
}
