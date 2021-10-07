// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace ReactiveMarbles.PropertyChanged;

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
    public static IDisposable BindOneWay<TFrom, TPropertyType, TTarget>(
        this TFrom fromObject,
        TTarget targetObject,
        Expression<Func<TFrom, TPropertyType>> fromProperty,
        Expression<Func<TTarget, TPropertyType>> toProperty,
        IScheduler scheduler = null)
        where TFrom : class, INotifyPropertyChanged
    {
        if (fromObject is null)
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
    public static IDisposable BindOneWay<TFrom, TFromProperty, TTarget, TTargetProperty>(
        this TFrom fromObject,
        TTarget targetObject,
        Expression<Func<TFrom, TFromProperty>> fromProperty,
        Expression<Func<TTarget, TTargetProperty>> toProperty,
        Func<TFromProperty, TTargetProperty> conversionFunc,
        IScheduler scheduler = null)
        where TFrom : class, INotifyPropertyChanged
    {
        if (fromObject is null)
        {
            throw new ArgumentNullException(nameof(fromObject));
        }

        var hostObs = fromObject.WhenChanged(fromProperty)
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
    public static IDisposable BindTwoWay<TFrom, TFromProperty, TTarget, TTargetProperty>(
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
        var hostObs = fromObject.WhenChanged(fromProperty)
            .Select(hostToTargetConv);
        var targetObs = targetObject.WhenChanged(toProperty)
            .Skip(1) // We have the host to win first off.
            .Select(targetToHostConv);

        return BindTwoWayImplementation(fromObject, targetObject, hostObs, targetObs, fromProperty, toProperty, scheduler);
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
    public static IDisposable BindTwoWay<TFrom, TProperty, TTarget>(
        this TFrom fromObject,
        TTarget targetObject,
        Expression<Func<TFrom, TProperty>> fromProperty,
        Expression<Func<TTarget, TProperty>> toProperty,
        IScheduler scheduler = null)
        where TFrom : class, INotifyPropertyChanged
        where TTarget : class, INotifyPropertyChanged
    {
        var hostObs = fromObject.WhenChanged(fromProperty);
        var targetObs = targetObject.WhenChanged(toProperty)
            .Skip(1); // We have the host to win first off.

        return BindTwoWayImplementation(fromObject, targetObject, hostObs, targetObs, fromProperty, toProperty, scheduler);
    }

    private static IDisposable BindTwoWayImplementation<TFrom, TFromProperty, TTarget, TTargetProperty>(
        TFrom fromObject,
        TTarget targetObject,
        IObservable<TTargetProperty> hostObs,
        IObservable<TFromProperty> targetObs,
        Expression<Func<TFrom, TFromProperty>> fromProperty,
        Expression<Func<TTarget, TTargetProperty>> toProperty,
        IScheduler scheduler)
    {
        if (hostObs is null)
        {
            throw new ArgumentNullException(nameof(hostObs));
        }

        if (toProperty is null)
        {
            throw new ArgumentNullException(nameof(toProperty));
        }

        if (fromProperty is null)
        {
            throw new ArgumentNullException(nameof(fromProperty));
        }

        if (toProperty.Body is not MemberExpression)
        {
            throw new ArgumentException("The expression does not bind to a valid member.");
        }

        if (fromProperty.Body is not MemberExpression)
        {
            throw new ArgumentException("The expression does not bind to a valid member.");
        }

        if ((scheduler ?? ImmediateScheduler.Instance) != ImmediateScheduler.Instance)
        {
            hostObs = hostObs.ObserveOn(scheduler);
            targetObs = targetObs.ObserveOn(scheduler);
        }

        var setterTo = toProperty.GetSetter();
        var setterFrom = fromProperty.GetSetter();

        return new CompositeDisposable(
            hostObs.Subscribe(x => setterTo(targetObject, x)),
            targetObs.Subscribe(x => setterFrom(fromObject, x)));
    }

    private static IDisposable OneWayBindImplementation<TTarget, TPropertyType>(
        TTarget targetObject,
        IObservable<TPropertyType> hostObs,
        Expression<Func<TTarget, TPropertyType>> property,
        IScheduler scheduler)
    {
        if (hostObs is null)
        {
            throw new ArgumentNullException(nameof(hostObs));
        }

        if (property is null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        if (property.Body is not MemberExpression)
        {
            throw new ArgumentException("The expression does not bind to a valid member.");
        }

        if ((scheduler ?? ImmediateScheduler.Instance) != ImmediateScheduler.Instance)
        {
            hostObs = hostObs.ObserveOn(scheduler);
        }

        var setter = property.GetSetter();
        return hostObs.Subscribe(x => setter(targetObject, x));
    }
}
