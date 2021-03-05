// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace ReactiveMarbles.PropertyChanged
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
            throw new Exception("The impementation should have been generated.");
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
            throw new Exception("The impementation should have been generated.");
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
            throw new Exception("The impementation should have been generated.");
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
            throw new Exception("The impementation should have been generated.");
        }
    }
}
