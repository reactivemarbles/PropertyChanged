// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace ReactiveMarbles.PropertyChanged.Benchmarks.Legacy
{
    /// <summary>
    /// Set of extension methods that handle binding.
    /// </summary>
    public static class BindExtensions
    {
        /// <summary>
        /// Performs one way binding between a property on the host to a target property.
        /// </summary>
        /// <param name="fromObject">The object which contains the host property.</param>
        /// <param name="targetObject">The object which contains the target property.</param>
        /// <param name="fromProperty">A expression to the host property.</param>
        /// <param name="toProperty">A expression to the target property.</param>
        /// <param name="scheduler">A scheduler for performing the binding on. Defaults to ImmediateScheduler.</param>
        /// <typeparam name="TFrom">The type of property the host is.</typeparam>
        /// <typeparam name="TPropertyType">The property types.</typeparam>
        /// <typeparam name="TTarget">The target property.</typeparam>
        /// <returns>A disposable which when disposed the binding will stop.</returns>
        /// <exception cref="ArgumentException">If there is a invalid expression.</exception>
        public static IDisposable OneWayBind<TFrom, TPropertyType, TTarget>(
            this TFrom fromObject,
            TTarget targetObject,
            Expression<Func<TFrom, TPropertyType>> fromProperty,
            Expression<Func<TTarget, TPropertyType>> toProperty,
            IScheduler scheduler = null)
            where TFrom : class, INotifyPropertyChanged
        {
            if (fromObject == null)
            {
                throw new ArgumentNullException(nameof(fromObject));
            }

            return OneWayBindImplementation(targetObject, fromObject.WhenChanged(fromProperty), toProperty, scheduler);
        }

        /// <summary>
        /// Performs one way binding between a property on the host to a target property.
        /// </summary>
        /// <param name="fromObject">The object which contains the host property.</param>
        /// <param name="targetObject">The object which contains the target property.</param>
        /// <param name="fromProperty">A expression to the host property.</param>
        /// <param name="toProperty">A expression to the target property.</param>
        /// <param name="conversionFunc">A converter which will convert the property from the host to the target property.</param>
        /// <param name="scheduler">A scheduler for performing the binding on. Defaults to ImmediateScheduler.</param>
        /// <typeparam name="TFrom">The type of property the host is.</typeparam>
        /// <typeparam name="TFromProperty">The property from type.</typeparam>
        /// <typeparam name="TTarget">The target property.</typeparam>
        /// <typeparam name="TTargetProperty">The property to type.</typeparam>
        /// <returns>A disposable which when disposed the binding will stop.</returns>
        /// <exception cref="ArgumentException">If there is a invalid expression.</exception>
        public static IDisposable OneWayBind<TFrom, TFromProperty, TTarget, TTargetProperty>(
            this TFrom fromObject,
            TTarget targetObject,
            Expression<Func<TFrom, TFromProperty>> fromProperty,
            Expression<Func<TTarget, TTargetProperty>> toProperty,
            Func<TFromProperty, TTargetProperty> conversionFunc,
            IScheduler scheduler = null)
            where TFrom : class, INotifyPropertyChanged
        {
            if (fromObject == null)
            {
                throw new ArgumentNullException(nameof(fromObject));
            }

            IObservable<TTargetProperty> hostObs = fromObject.WhenChanged(fromProperty)
                .Select(conversionFunc);

            return OneWayBindImplementation(targetObject, hostObs, toProperty, scheduler);
        }

        /// <summary>
        /// Performs one way binding between a property on the host to a target property.
        /// </summary>
        /// <param name="fromObject">The object which contains the host property.</param>
        /// <param name="targetObject">The object which contains the target property.</param>
        /// <param name="fromProperty">A expression to the host property.</param>
        /// <param name="toProperty">A expression to the target property.</param>
        /// <param name="hostToTargetConv">A converter which will convert the property from the host to the target property.</param>
        /// <param name="targetToHostConv">A converter which will convert the property from the target to the host property.</param>
        /// <param name="scheduler">A scheduler for performing the binding on. Defaults to ImmediateScheduler.</param>
        /// <typeparam name="TFrom">The type of property the host is.</typeparam>
        /// <typeparam name="TFromProperty">The property from type.</typeparam>
        /// <typeparam name="TTarget">The target property.</typeparam>
        /// <typeparam name="TTargetProperty">The property to type.</typeparam>
        /// <returns>A disposable which when disposed the binding will stop.</returns>
        /// <exception cref="ArgumentException">If there is a invalid expression.</exception>
        public static IDisposable Bind<TFrom, TFromProperty, TTarget, TTargetProperty>(
            this TFrom fromObject,
            TTarget targetObject,
            Expression<Func<TFrom, TFromProperty>> fromProperty,
            Expression<Func<TTarget, TTargetProperty>> toProperty,
            Func<TFromProperty, TTargetProperty> hostToTargetConv,
            Func<TTargetProperty, TFromProperty> targetToHostConv,
            IScheduler scheduler = null)
            where TFrom : class, INotifyPropertyChanged
            where TTarget : class, INotifyPropertyChanged
        {
            IObservable<(object value, bool isHost)> hostObs = fromObject.WhenChanged(fromProperty)
                .Select(hostToTargetConv)
                .Select(x => (value: (object)x, isHost: true));
            IObservable<(object value, bool isHost)> targetObs = targetObject.WhenChanged(toProperty)
                .Skip(1) // We have the host to win first off.
                .Select(targetToHostConv)
                .Select(x => (value: (object)x, isHost: false));

            return BindImplementation(fromObject, targetObject, hostObs, targetObs, fromProperty, toProperty, scheduler);
        }

        /// <summary>
        /// Performs one way binding between a property on the host to a target property.
        /// </summary>
        /// <param name="fromObject">The object which contains the host property.</param>
        /// <param name="targetObject">The object which contains the target property.</param>
        /// <param name="fromProperty">A expression to the host property.</param>
        /// <param name="toProperty">A expression to the target property.</param>
        /// <param name="scheduler">A scheduler for performing the binding on. Defaults to ImmediateScheduler.</param>
        /// <typeparam name="TFrom">The type of property the host is.</typeparam>
        /// <typeparam name="TProperty">The property from type.</typeparam>
        /// <typeparam name="TTarget">The target property.</typeparam>
        /// <returns>A disposable which when disposed the binding will stop.</returns>
        /// <exception cref="ArgumentException">If there is a invalid expression.</exception>
        public static IDisposable Bind<TFrom, TProperty, TTarget>(
            this TFrom fromObject,
            TTarget targetObject,
            Expression<Func<TFrom, TProperty>> fromProperty,
            Expression<Func<TTarget, TProperty>> toProperty,
            IScheduler scheduler = null)
            where TFrom : class, INotifyPropertyChanged
            where TTarget : class, INotifyPropertyChanged
        {
            IObservable<(TProperty value, bool isHost)> hostObs = fromObject.WhenChanged(fromProperty)
                .Select(x => (value: x, isHost: true));
            IObservable<(TProperty value, bool isHost)> targetObs = targetObject.WhenChanged(toProperty)
                .Skip(1) // We have the host to win first off.
                .Select(x => (value: x, isHost: false));

            return BindImplementation(fromObject, targetObject, hostObs, targetObs, fromProperty, toProperty, scheduler);
        }

        private static IDisposable BindImplementation<TFrom, TTarget, TPropertyType>(
            TFrom fromObject,
            TTarget targetObject,
            IObservable<(TPropertyType Value, bool IsHost)> hostObs,
            IObservable<(TPropertyType Value, bool IsHost)> targetObs,
            LambdaExpression fromProperty,
            LambdaExpression toProperty,
            IScheduler scheduler)
        {
            if (hostObs == null)
            {
                throw new ArgumentNullException(nameof(hostObs));
            }

            if (toProperty == null)
            {
                throw new ArgumentNullException(nameof(toProperty));
            }

            if (fromProperty == null)
            {
                throw new ArgumentNullException(nameof(fromProperty));
            }

            if (!(toProperty.Body is MemberExpression targetMemberExpression))
            {
                throw new ArgumentException("The expression does not bind to a valid member.");
            }

            if (!(fromProperty.Body is MemberExpression fromMemberExpression))
            {
                throw new ArgumentException("The expression does not bind to a valid member.");
            }

            scheduler = scheduler ?? ImmediateScheduler.Instance;

            Action<object, TPropertyType> setTargetFunc =
                SetMemberFuncCache<TPropertyType>.GenerateSetCache(targetMemberExpression.Member);
            Action<object, TPropertyType> setHostFunc = SetMemberFuncCache<TPropertyType>.GenerateSetCache(fromMemberExpression.Member);

            System.Collections.Generic.List<Func<object, object>> getFetcherToPropertyChain = toProperty.Body.GetGetValueMemberChain();
            System.Collections.Generic.List<Func<object, object>> getFetchFromPropertyChain = fromProperty.Body.GetGetValueMemberChain();
            return hostObs.Merge(targetObs).ObserveOn(scheduler).Subscribe(x =>
            {
                if (x.IsHost)
                {
                    object parent = getFetcherToPropertyChain.GetParentForExpression(targetObject);
                    setTargetFunc(parent, x.Value);
                }
                else
                {
                    object parent = getFetchFromPropertyChain.GetParentForExpression(fromObject);
                    setHostFunc(parent, x.Value);
                }
            });
        }

        private static IDisposable OneWayBindImplementation<TTarget, TPropertyType>(
            TTarget targetObject,
            IObservable<TPropertyType> hostObs,
            LambdaExpression property,
            IScheduler scheduler)
        {
            if (hostObs == null)
            {
                throw new ArgumentNullException(nameof(hostObs));
            }

            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (!(property.Body is MemberExpression fromMemberExpression))
            {
                throw new ArgumentException("The expression does not bind to a valid member.");
            }

            scheduler ??= ImmediateScheduler.Instance;

            Action<object, TPropertyType> setHostFunc = SetMemberFuncCache<TPropertyType>.GenerateSetCache(fromMemberExpression.Member);
            System.Collections.Generic.List<Func<object, object>> getFetcherPropertyChain = property.Body.GetGetValueMemberChain();

            return hostObs.ObserveOn(scheduler).Subscribe(x =>
            {
                object parent = getFetcherPropertyChain.GetParentForExpression(targetObject);
                setHostFunc(parent, x);
            });
        }
    }
}
