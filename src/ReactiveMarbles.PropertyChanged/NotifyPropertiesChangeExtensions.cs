// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace ReactiveMarbles.PropertyChanged;

/// <summary>
/// Provides extension methods for the notify property changed extensions.
/// </summary>
public static class NotifyPropertiesChangeExtensions
{
    /// <summary>
    /// Notifies when the specified property changes.
    /// </summary>
    /// <param name="objectToMonitor">The object to monitor.</param>
    /// <param name="propertyExpression1">A expression to the value1.</param>
    /// <param name="propertyExpression2">A expression to the value2.</param>
    /// <param name="conversionFunc">Parameter which converts into the end value.</param>
    /// <typeparam name="TObj">The type of initial object.</typeparam>
    /// <typeparam name="TTempReturn1">The return type of the value1.</typeparam>
    /// <typeparam name="TTempReturn2">The return type of the value2.</typeparam>
    /// <typeparam name="TReturn">The return value of the observable. Generated from the conversion func.</typeparam>
    /// <returns>An observable that signals when the properties specified in the expressions have changed.</returns>
    /// <exception cref="ArgumentNullException">Either the property expression or the object to monitor is null.</exception>
    /// <exception cref="ArgumentException">If there is an issue with the property expression.</exception>
    public static IObservable<TReturn> WhenChanged<TObj, TTempReturn1, TTempReturn2, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, TTempReturn1>> propertyExpression1,
        Expression<Func<TObj, TTempReturn2>> propertyExpression2,
        Func<TTempReturn1, TTempReturn2, TReturn> conversionFunc)
        where TObj : class, INotifyPropertyChanged
    {
        var obs1 = objectToMonitor.WhenChanged(propertyExpression1);

        var obs2 = objectToMonitor.WhenChanged(propertyExpression2);

        return obs1.CombineLatest(obs2, conversionFunc);
    }

    /// <summary>
    /// Notifies when the specified property changes.
    /// </summary>
    /// <param name="objectToMonitor">The object to monitor.</param>
    /// <param name="propertyExpression1">A expression to the value1.</param>
    /// <param name="propertyExpression2">A expression to the value2.</param>
    /// <param name="propertyExpression3">A expression to the value3.</param>
    /// <param name="conversionFunc">Parameter which converts into the end value.</param>
    /// <typeparam name="TObj">The type of initial object.</typeparam>
    /// <typeparam name="TTempReturn1">The return type of the value1.</typeparam>
    /// <typeparam name="TTempReturn2">The return type of the value2.</typeparam>
    /// <typeparam name="TTempReturn3">The return type of the value3.</typeparam>
    /// <typeparam name="TReturn">The return value of the observable. Generated from the conversion func.</typeparam>
    /// <returns>An observable that signals when the properties specified in the expressions have changed.</returns>
    /// <exception cref="ArgumentNullException">Either the property expression or the object to monitor is null.</exception>
    /// <exception cref="ArgumentException">If there is an issue with the property expression.</exception>
    public static IObservable<TReturn> WhenChanged<TObj, TTempReturn1, TTempReturn2, TTempReturn3, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, TTempReturn1>> propertyExpression1,
        Expression<Func<TObj, TTempReturn2>> propertyExpression2,
        Expression<Func<TObj, TTempReturn3>> propertyExpression3,
        Func<TTempReturn1, TTempReturn2, TTempReturn3, TReturn> conversionFunc)
        where TObj : class, INotifyPropertyChanged
    {
        var obs1 = objectToMonitor.WhenChanged(propertyExpression1);

        var obs2 = objectToMonitor.WhenChanged(propertyExpression2);

        var obs3 = objectToMonitor.WhenChanged(propertyExpression3);

        return obs1.CombineLatest(obs2, obs3, conversionFunc);
    }

    /// <summary>
    /// Notifies when the specified property changes.
    /// </summary>
    /// <param name="objectToMonitor">The object to monitor.</param>
    /// <param name="propertyExpression1">A expression to the value1.</param>
    /// <param name="propertyExpression2">A expression to the value2.</param>
    /// <param name="propertyExpression3">A expression to the value3.</param>
    /// <param name="propertyExpression4">A expression to the value4.</param>
    /// <param name="conversionFunc">Parameter which converts into the end value.</param>
    /// <typeparam name="TObj">The type of initial object.</typeparam>
    /// <typeparam name="TTempReturn1">The return type of the value1.</typeparam>
    /// <typeparam name="TTempReturn2">The return type of the value2.</typeparam>
    /// <typeparam name="TTempReturn3">The return type of the value3.</typeparam>
    /// <typeparam name="TTempReturn4">The return type of the value4.</typeparam>
    /// <typeparam name="TReturn">The return value of the observable. Generated from the conversion func.</typeparam>
    /// <returns>An observable that signals when the properties specified in the expressions have changed.</returns>
    /// <exception cref="ArgumentNullException">Either the property expression or the object to monitor is null.</exception>
    /// <exception cref="ArgumentException">If there is an issue with the property expression.</exception>
    public static IObservable<TReturn> WhenChanged<TObj, TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, TTempReturn1>> propertyExpression1,
        Expression<Func<TObj, TTempReturn2>> propertyExpression2,
        Expression<Func<TObj, TTempReturn3>> propertyExpression3,
        Expression<Func<TObj, TTempReturn4>> propertyExpression4,
        Func<TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TReturn> conversionFunc)
        where TObj : class, INotifyPropertyChanged
    {
        var obs1 = objectToMonitor.WhenChanged(propertyExpression1);

        var obs2 = objectToMonitor.WhenChanged(propertyExpression2);

        var obs3 = objectToMonitor.WhenChanged(propertyExpression3);

        var obs4 = objectToMonitor.WhenChanged(propertyExpression4);

        return obs1.CombineLatest(obs2, obs3, obs4, conversionFunc);
    }

    /// <summary>
    /// Notifies when the specified property changes.
    /// </summary>
    /// <param name="objectToMonitor">The object to monitor.</param>
    /// <param name="propertyExpression1">A expression to the value1.</param>
    /// <param name="propertyExpression2">A expression to the value2.</param>
    /// <param name="propertyExpression3">A expression to the value3.</param>
    /// <param name="propertyExpression4">A expression to the value4.</param>
    /// <param name="propertyExpression5">A expression to the value5.</param>
    /// <param name="conversionFunc">Parameter which converts into the end value.</param>
    /// <typeparam name="TObj">The type of initial object.</typeparam>
    /// <typeparam name="TTempReturn1">The return type of the value1.</typeparam>
    /// <typeparam name="TTempReturn2">The return type of the value2.</typeparam>
    /// <typeparam name="TTempReturn3">The return type of the value3.</typeparam>
    /// <typeparam name="TTempReturn4">The return type of the value4.</typeparam>
    /// <typeparam name="TTempReturn5">The return type of the value5.</typeparam>
    /// <typeparam name="TReturn">The return value of the observable. Generated from the conversion func.</typeparam>
    /// <returns>An observable that signals when the properties specified in the expressions have changed.</returns>
    /// <exception cref="ArgumentNullException">Either the property expression or the object to monitor is null.</exception>
    /// <exception cref="ArgumentException">If there is an issue with the property expression.</exception>
    public static IObservable<TReturn> WhenChanged<TObj, TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, TTempReturn1>> propertyExpression1,
        Expression<Func<TObj, TTempReturn2>> propertyExpression2,
        Expression<Func<TObj, TTempReturn3>> propertyExpression3,
        Expression<Func<TObj, TTempReturn4>> propertyExpression4,
        Expression<Func<TObj, TTempReturn5>> propertyExpression5,
        Func<TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TReturn> conversionFunc)
        where TObj : class, INotifyPropertyChanged
    {
        var obs1 = objectToMonitor.WhenChanged(propertyExpression1);

        var obs2 = objectToMonitor.WhenChanged(propertyExpression2);

        var obs3 = objectToMonitor.WhenChanged(propertyExpression3);

        var obs4 = objectToMonitor.WhenChanged(propertyExpression4);

        var obs5 = objectToMonitor.WhenChanged(propertyExpression5);

        return obs1.CombineLatest(obs2, obs3, obs4, obs5, conversionFunc);
    }

    /// <summary>
    /// Notifies when the specified property changes.
    /// </summary>
    /// <param name="objectToMonitor">The object to monitor.</param>
    /// <param name="propertyExpression1">A expression to the value1.</param>
    /// <param name="propertyExpression2">A expression to the value2.</param>
    /// <param name="propertyExpression3">A expression to the value3.</param>
    /// <param name="propertyExpression4">A expression to the value4.</param>
    /// <param name="propertyExpression5">A expression to the value5.</param>
    /// <param name="propertyExpression6">A expression to the value6.</param>
    /// <param name="conversionFunc">Parameter which converts into the end value.</param>
    /// <typeparam name="TObj">The type of initial object.</typeparam>
    /// <typeparam name="TTempReturn1">The return type of the value1.</typeparam>
    /// <typeparam name="TTempReturn2">The return type of the value2.</typeparam>
    /// <typeparam name="TTempReturn3">The return type of the value3.</typeparam>
    /// <typeparam name="TTempReturn4">The return type of the value4.</typeparam>
    /// <typeparam name="TTempReturn5">The return type of the value5.</typeparam>
    /// <typeparam name="TTempReturn6">The return type of the value6.</typeparam>
    /// <typeparam name="TReturn">The return value of the observable. Generated from the conversion func.</typeparam>
    /// <returns>An observable that signals when the properties specified in the expressions have changed.</returns>
    /// <exception cref="ArgumentNullException">Either the property expression or the object to monitor is null.</exception>
    /// <exception cref="ArgumentException">If there is an issue with the property expression.</exception>
    public static IObservable<TReturn> WhenChanged<TObj, TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TTempReturn6, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, TTempReturn1>> propertyExpression1,
        Expression<Func<TObj, TTempReturn2>> propertyExpression2,
        Expression<Func<TObj, TTempReturn3>> propertyExpression3,
        Expression<Func<TObj, TTempReturn4>> propertyExpression4,
        Expression<Func<TObj, TTempReturn5>> propertyExpression5,
        Expression<Func<TObj, TTempReturn6>> propertyExpression6,
        Func<TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TTempReturn6, TReturn> conversionFunc)
        where TObj : class, INotifyPropertyChanged
    {
        var obs1 = objectToMonitor.WhenChanged(propertyExpression1);

        var obs2 = objectToMonitor.WhenChanged(propertyExpression2);

        var obs3 = objectToMonitor.WhenChanged(propertyExpression3);

        var obs4 = objectToMonitor.WhenChanged(propertyExpression4);

        var obs5 = objectToMonitor.WhenChanged(propertyExpression5);

        var obs6 = objectToMonitor.WhenChanged(propertyExpression6);

        return obs1.CombineLatest(obs2, obs3, obs4, obs5, obs6, conversionFunc);
    }

    /// <summary>
    /// Notifies when the specified property changes.
    /// </summary>
    /// <param name="objectToMonitor">The object to monitor.</param>
    /// <param name="propertyExpression1">A expression to the value1.</param>
    /// <param name="propertyExpression2">A expression to the value2.</param>
    /// <param name="propertyExpression3">A expression to the value3.</param>
    /// <param name="propertyExpression4">A expression to the value4.</param>
    /// <param name="propertyExpression5">A expression to the value5.</param>
    /// <param name="propertyExpression6">A expression to the value6.</param>
    /// <param name="propertyExpression7">A expression to the value7.</param>
    /// <param name="conversionFunc">Parameter which converts into the end value.</param>
    /// <typeparam name="TObj">The type of initial object.</typeparam>
    /// <typeparam name="TTempReturn1">The return type of the value1.</typeparam>
    /// <typeparam name="TTempReturn2">The return type of the value2.</typeparam>
    /// <typeparam name="TTempReturn3">The return type of the value3.</typeparam>
    /// <typeparam name="TTempReturn4">The return type of the value4.</typeparam>
    /// <typeparam name="TTempReturn5">The return type of the value5.</typeparam>
    /// <typeparam name="TTempReturn6">The return type of the value6.</typeparam>
    /// <typeparam name="TTempReturn7">The return type of the value7.</typeparam>
    /// <typeparam name="TReturn">The return value of the observable. Generated from the conversion func.</typeparam>
    /// <returns>An observable that signals when the properties specified in the expressions have changed.</returns>
    /// <exception cref="ArgumentNullException">Either the property expression or the object to monitor is null.</exception>
    /// <exception cref="ArgumentException">If there is an issue with the property expression.</exception>
    public static IObservable<TReturn> WhenChanged<TObj, TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TTempReturn6, TTempReturn7, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, TTempReturn1>> propertyExpression1,
        Expression<Func<TObj, TTempReturn2>> propertyExpression2,
        Expression<Func<TObj, TTempReturn3>> propertyExpression3,
        Expression<Func<TObj, TTempReturn4>> propertyExpression4,
        Expression<Func<TObj, TTempReturn5>> propertyExpression5,
        Expression<Func<TObj, TTempReturn6>> propertyExpression6,
        Expression<Func<TObj, TTempReturn7>> propertyExpression7,
        Func<TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TTempReturn6, TTempReturn7, TReturn> conversionFunc)
        where TObj : class, INotifyPropertyChanged
    {
        var obs1 = objectToMonitor.WhenChanged(propertyExpression1);

        var obs2 = objectToMonitor.WhenChanged(propertyExpression2);

        var obs3 = objectToMonitor.WhenChanged(propertyExpression3);

        var obs4 = objectToMonitor.WhenChanged(propertyExpression4);

        var obs5 = objectToMonitor.WhenChanged(propertyExpression5);

        var obs6 = objectToMonitor.WhenChanged(propertyExpression6);

        var obs7 = objectToMonitor.WhenChanged(propertyExpression7);

        return obs1.CombineLatest(obs2, obs3, obs4, obs5, obs6, obs7, conversionFunc);
    }

    /// <summary>
    /// Notifies when the specified property changes.
    /// </summary>
    /// <param name="objectToMonitor">The object to monitor.</param>
    /// <param name="propertyExpression1">A expression to the value1.</param>
    /// <param name="propertyExpression2">A expression to the value2.</param>
    /// <param name="propertyExpression3">A expression to the value3.</param>
    /// <param name="propertyExpression4">A expression to the value4.</param>
    /// <param name="propertyExpression5">A expression to the value5.</param>
    /// <param name="propertyExpression6">A expression to the value6.</param>
    /// <param name="propertyExpression7">A expression to the value7.</param>
    /// <param name="propertyExpression8">A expression to the value8.</param>
    /// <param name="conversionFunc">Parameter which converts into the end value.</param>
    /// <typeparam name="TObj">The type of initial object.</typeparam>
    /// <typeparam name="TTempReturn1">The return type of the value1.</typeparam>
    /// <typeparam name="TTempReturn2">The return type of the value2.</typeparam>
    /// <typeparam name="TTempReturn3">The return type of the value3.</typeparam>
    /// <typeparam name="TTempReturn4">The return type of the value4.</typeparam>
    /// <typeparam name="TTempReturn5">The return type of the value5.</typeparam>
    /// <typeparam name="TTempReturn6">The return type of the value6.</typeparam>
    /// <typeparam name="TTempReturn7">The return type of the value7.</typeparam>
    /// <typeparam name="TTempReturn8">The return type of the value8.</typeparam>
    /// <typeparam name="TReturn">The return value of the observable. Generated from the conversion func.</typeparam>
    /// <returns>An observable that signals when the properties specified in the expressions have changed.</returns>
    /// <exception cref="ArgumentNullException">Either the property expression or the object to monitor is null.</exception>
    /// <exception cref="ArgumentException">If there is an issue with the property expression.</exception>
    public static IObservable<TReturn> WhenChanged<TObj, TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TTempReturn6, TTempReturn7, TTempReturn8, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, TTempReturn1>> propertyExpression1,
        Expression<Func<TObj, TTempReturn2>> propertyExpression2,
        Expression<Func<TObj, TTempReturn3>> propertyExpression3,
        Expression<Func<TObj, TTempReturn4>> propertyExpression4,
        Expression<Func<TObj, TTempReturn5>> propertyExpression5,
        Expression<Func<TObj, TTempReturn6>> propertyExpression6,
        Expression<Func<TObj, TTempReturn7>> propertyExpression7,
        Expression<Func<TObj, TTempReturn8>> propertyExpression8,
        Func<TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TTempReturn6, TTempReturn7, TTempReturn8, TReturn> conversionFunc)
        where TObj : class, INotifyPropertyChanged
    {
        var obs1 = objectToMonitor.WhenChanged(propertyExpression1);

        var obs2 = objectToMonitor.WhenChanged(propertyExpression2);

        var obs3 = objectToMonitor.WhenChanged(propertyExpression3);

        var obs4 = objectToMonitor.WhenChanged(propertyExpression4);

        var obs5 = objectToMonitor.WhenChanged(propertyExpression5);

        var obs6 = objectToMonitor.WhenChanged(propertyExpression6);

        var obs7 = objectToMonitor.WhenChanged(propertyExpression7);

        var obs8 = objectToMonitor.WhenChanged(propertyExpression8);

        return obs1.CombineLatest(obs2, obs3, obs4, obs5, obs6, obs7, obs8, conversionFunc);
    }

    /// <summary>
    /// Notifies when the specified property changes.
    /// </summary>
    /// <param name="objectToMonitor">The object to monitor.</param>
    /// <param name="propertyExpression1">A expression to the value1.</param>
    /// <param name="propertyExpression2">A expression to the value2.</param>
    /// <param name="propertyExpression3">A expression to the value3.</param>
    /// <param name="propertyExpression4">A expression to the value4.</param>
    /// <param name="propertyExpression5">A expression to the value5.</param>
    /// <param name="propertyExpression6">A expression to the value6.</param>
    /// <param name="propertyExpression7">A expression to the value7.</param>
    /// <param name="propertyExpression8">A expression to the value8.</param>
    /// <param name="propertyExpression9">A expression to the value9.</param>
    /// <param name="conversionFunc">Parameter which converts into the end value.</param>
    /// <typeparam name="TObj">The type of initial object.</typeparam>
    /// <typeparam name="TTempReturn1">The return type of the value1.</typeparam>
    /// <typeparam name="TTempReturn2">The return type of the value2.</typeparam>
    /// <typeparam name="TTempReturn3">The return type of the value3.</typeparam>
    /// <typeparam name="TTempReturn4">The return type of the value4.</typeparam>
    /// <typeparam name="TTempReturn5">The return type of the value5.</typeparam>
    /// <typeparam name="TTempReturn6">The return type of the value6.</typeparam>
    /// <typeparam name="TTempReturn7">The return type of the value7.</typeparam>
    /// <typeparam name="TTempReturn8">The return type of the value8.</typeparam>
    /// <typeparam name="TTempReturn9">The return type of the value9.</typeparam>
    /// <typeparam name="TReturn">The return value of the observable. Generated from the conversion func.</typeparam>
    /// <returns>An observable that signals when the properties specified in the expressions have changed.</returns>
    /// <exception cref="ArgumentNullException">Either the property expression or the object to monitor is null.</exception>
    /// <exception cref="ArgumentException">If there is an issue with the property expression.</exception>
    public static IObservable<TReturn> WhenChanged<TObj, TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TTempReturn6, TTempReturn7, TTempReturn8, TTempReturn9, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, TTempReturn1>> propertyExpression1,
        Expression<Func<TObj, TTempReturn2>> propertyExpression2,
        Expression<Func<TObj, TTempReturn3>> propertyExpression3,
        Expression<Func<TObj, TTempReturn4>> propertyExpression4,
        Expression<Func<TObj, TTempReturn5>> propertyExpression5,
        Expression<Func<TObj, TTempReturn6>> propertyExpression6,
        Expression<Func<TObj, TTempReturn7>> propertyExpression7,
        Expression<Func<TObj, TTempReturn8>> propertyExpression8,
        Expression<Func<TObj, TTempReturn9>> propertyExpression9,
        Func<TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TTempReturn6, TTempReturn7, TTempReturn8, TTempReturn9, TReturn> conversionFunc)
        where TObj : class, INotifyPropertyChanged
    {
        var obs1 = objectToMonitor.WhenChanged(propertyExpression1);

        var obs2 = objectToMonitor.WhenChanged(propertyExpression2);

        var obs3 = objectToMonitor.WhenChanged(propertyExpression3);

        var obs4 = objectToMonitor.WhenChanged(propertyExpression4);

        var obs5 = objectToMonitor.WhenChanged(propertyExpression5);

        var obs6 = objectToMonitor.WhenChanged(propertyExpression6);

        var obs7 = objectToMonitor.WhenChanged(propertyExpression7);

        var obs8 = objectToMonitor.WhenChanged(propertyExpression8);

        var obs9 = objectToMonitor.WhenChanged(propertyExpression9);

        return obs1.CombineLatest(obs2, obs3, obs4, obs5, obs6, obs7, obs8, obs9, conversionFunc);
    }

    /// <summary>
    /// Notifies when the specified property changes.
    /// </summary>
    /// <param name="objectToMonitor">The object to monitor.</param>
    /// <param name="propertyExpression1">A expression to the value1.</param>
    /// <param name="propertyExpression2">A expression to the value2.</param>
    /// <param name="propertyExpression3">A expression to the value3.</param>
    /// <param name="propertyExpression4">A expression to the value4.</param>
    /// <param name="propertyExpression5">A expression to the value5.</param>
    /// <param name="propertyExpression6">A expression to the value6.</param>
    /// <param name="propertyExpression7">A expression to the value7.</param>
    /// <param name="propertyExpression8">A expression to the value8.</param>
    /// <param name="propertyExpression9">A expression to the value9.</param>
    /// <param name="propertyExpression10">A expression to the value10.</param>
    /// <param name="conversionFunc">Parameter which converts into the end value.</param>
    /// <typeparam name="TObj">The type of initial object.</typeparam>
    /// <typeparam name="TTempReturn1">The return type of the value1.</typeparam>
    /// <typeparam name="TTempReturn2">The return type of the value2.</typeparam>
    /// <typeparam name="TTempReturn3">The return type of the value3.</typeparam>
    /// <typeparam name="TTempReturn4">The return type of the value4.</typeparam>
    /// <typeparam name="TTempReturn5">The return type of the value5.</typeparam>
    /// <typeparam name="TTempReturn6">The return type of the value6.</typeparam>
    /// <typeparam name="TTempReturn7">The return type of the value7.</typeparam>
    /// <typeparam name="TTempReturn8">The return type of the value8.</typeparam>
    /// <typeparam name="TTempReturn9">The return type of the value9.</typeparam>
    /// <typeparam name="TTempReturn10">The return type of the value10.</typeparam>
    /// <typeparam name="TReturn">The return value of the observable. Generated from the conversion func.</typeparam>
    /// <returns>An observable that signals when the properties specified in the expressions have changed.</returns>
    /// <exception cref="ArgumentNullException">Either the property expression or the object to monitor is null.</exception>
    /// <exception cref="ArgumentException">If there is an issue with the property expression.</exception>
    public static IObservable<TReturn> WhenChanged<TObj, TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TTempReturn6, TTempReturn7, TTempReturn8, TTempReturn9, TTempReturn10, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, TTempReturn1>> propertyExpression1,
        Expression<Func<TObj, TTempReturn2>> propertyExpression2,
        Expression<Func<TObj, TTempReturn3>> propertyExpression3,
        Expression<Func<TObj, TTempReturn4>> propertyExpression4,
        Expression<Func<TObj, TTempReturn5>> propertyExpression5,
        Expression<Func<TObj, TTempReturn6>> propertyExpression6,
        Expression<Func<TObj, TTempReturn7>> propertyExpression7,
        Expression<Func<TObj, TTempReturn8>> propertyExpression8,
        Expression<Func<TObj, TTempReturn9>> propertyExpression9,
        Expression<Func<TObj, TTempReturn10>> propertyExpression10,
        Func<TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TTempReturn6, TTempReturn7, TTempReturn8, TTempReturn9, TTempReturn10, TReturn> conversionFunc)
        where TObj : class, INotifyPropertyChanged
    {
        var obs1 = objectToMonitor.WhenChanged(propertyExpression1);

        var obs2 = objectToMonitor.WhenChanged(propertyExpression2);

        var obs3 = objectToMonitor.WhenChanged(propertyExpression3);

        var obs4 = objectToMonitor.WhenChanged(propertyExpression4);

        var obs5 = objectToMonitor.WhenChanged(propertyExpression5);

        var obs6 = objectToMonitor.WhenChanged(propertyExpression6);

        var obs7 = objectToMonitor.WhenChanged(propertyExpression7);

        var obs8 = objectToMonitor.WhenChanged(propertyExpression8);

        var obs9 = objectToMonitor.WhenChanged(propertyExpression9);

        var obs10 = objectToMonitor.WhenChanged(propertyExpression10);

        return obs1.CombineLatest(obs2, obs3, obs4, obs5, obs6, obs7, obs8, obs9, obs10, conversionFunc);
    }

    /// <summary>
    /// Notifies when the specified property changes.
    /// </summary>
    /// <param name="objectToMonitor">The object to monitor.</param>
    /// <param name="propertyExpression1">A expression to the value1.</param>
    /// <param name="propertyExpression2">A expression to the value2.</param>
    /// <param name="propertyExpression3">A expression to the value3.</param>
    /// <param name="propertyExpression4">A expression to the value4.</param>
    /// <param name="propertyExpression5">A expression to the value5.</param>
    /// <param name="propertyExpression6">A expression to the value6.</param>
    /// <param name="propertyExpression7">A expression to the value7.</param>
    /// <param name="propertyExpression8">A expression to the value8.</param>
    /// <param name="propertyExpression9">A expression to the value9.</param>
    /// <param name="propertyExpression10">A expression to the value10.</param>
    /// <param name="propertyExpression11">A expression to the value11.</param>
    /// <param name="conversionFunc">Parameter which converts into the end value.</param>
    /// <typeparam name="TObj">The type of initial object.</typeparam>
    /// <typeparam name="TTempReturn1">The return type of the value1.</typeparam>
    /// <typeparam name="TTempReturn2">The return type of the value2.</typeparam>
    /// <typeparam name="TTempReturn3">The return type of the value3.</typeparam>
    /// <typeparam name="TTempReturn4">The return type of the value4.</typeparam>
    /// <typeparam name="TTempReturn5">The return type of the value5.</typeparam>
    /// <typeparam name="TTempReturn6">The return type of the value6.</typeparam>
    /// <typeparam name="TTempReturn7">The return type of the value7.</typeparam>
    /// <typeparam name="TTempReturn8">The return type of the value8.</typeparam>
    /// <typeparam name="TTempReturn9">The return type of the value9.</typeparam>
    /// <typeparam name="TTempReturn10">The return type of the value10.</typeparam>
    /// <typeparam name="TTempReturn11">The return type of the value11.</typeparam>
    /// <typeparam name="TReturn">The return value of the observable. Generated from the conversion func.</typeparam>
    /// <returns>An observable that signals when the properties specified in the expressions have changed.</returns>
    /// <exception cref="ArgumentNullException">Either the property expression or the object to monitor is null.</exception>
    /// <exception cref="ArgumentException">If there is an issue with the property expression.</exception>
    public static IObservable<TReturn> WhenChanged<TObj, TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TTempReturn6, TTempReturn7, TTempReturn8, TTempReturn9, TTempReturn10, TTempReturn11, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, TTempReturn1>> propertyExpression1,
        Expression<Func<TObj, TTempReturn2>> propertyExpression2,
        Expression<Func<TObj, TTempReturn3>> propertyExpression3,
        Expression<Func<TObj, TTempReturn4>> propertyExpression4,
        Expression<Func<TObj, TTempReturn5>> propertyExpression5,
        Expression<Func<TObj, TTempReturn6>> propertyExpression6,
        Expression<Func<TObj, TTempReturn7>> propertyExpression7,
        Expression<Func<TObj, TTempReturn8>> propertyExpression8,
        Expression<Func<TObj, TTempReturn9>> propertyExpression9,
        Expression<Func<TObj, TTempReturn10>> propertyExpression10,
        Expression<Func<TObj, TTempReturn11>> propertyExpression11,
        Func<TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TTempReturn6, TTempReturn7, TTempReturn8, TTempReturn9, TTempReturn10, TTempReturn11, TReturn> conversionFunc)
        where TObj : class, INotifyPropertyChanged
    {
        var obs1 = objectToMonitor.WhenChanged(propertyExpression1);

        var obs2 = objectToMonitor.WhenChanged(propertyExpression2);

        var obs3 = objectToMonitor.WhenChanged(propertyExpression3);

        var obs4 = objectToMonitor.WhenChanged(propertyExpression4);

        var obs5 = objectToMonitor.WhenChanged(propertyExpression5);

        var obs6 = objectToMonitor.WhenChanged(propertyExpression6);

        var obs7 = objectToMonitor.WhenChanged(propertyExpression7);

        var obs8 = objectToMonitor.WhenChanged(propertyExpression8);

        var obs9 = objectToMonitor.WhenChanged(propertyExpression9);

        var obs10 = objectToMonitor.WhenChanged(propertyExpression10);

        var obs11 = objectToMonitor.WhenChanged(propertyExpression11);

        return obs1.CombineLatest(obs2, obs3, obs4, obs5, obs6, obs7, obs8, obs9, obs10, obs11, conversionFunc);
    }

    /// <summary>
    /// Notifies when the specified property changes.
    /// </summary>
    /// <param name="objectToMonitor">The object to monitor.</param>
    /// <param name="propertyExpression1">A expression to the value1.</param>
    /// <param name="propertyExpression2">A expression to the value2.</param>
    /// <param name="propertyExpression3">A expression to the value3.</param>
    /// <param name="propertyExpression4">A expression to the value4.</param>
    /// <param name="propertyExpression5">A expression to the value5.</param>
    /// <param name="propertyExpression6">A expression to the value6.</param>
    /// <param name="propertyExpression7">A expression to the value7.</param>
    /// <param name="propertyExpression8">A expression to the value8.</param>
    /// <param name="propertyExpression9">A expression to the value9.</param>
    /// <param name="propertyExpression10">A expression to the value10.</param>
    /// <param name="propertyExpression11">A expression to the value11.</param>
    /// <param name="propertyExpression12">A expression to the value12.</param>
    /// <param name="conversionFunc">Parameter which converts into the end value.</param>
    /// <typeparam name="TObj">The type of initial object.</typeparam>
    /// <typeparam name="TTempReturn1">The return type of the value1.</typeparam>
    /// <typeparam name="TTempReturn2">The return type of the value2.</typeparam>
    /// <typeparam name="TTempReturn3">The return type of the value3.</typeparam>
    /// <typeparam name="TTempReturn4">The return type of the value4.</typeparam>
    /// <typeparam name="TTempReturn5">The return type of the value5.</typeparam>
    /// <typeparam name="TTempReturn6">The return type of the value6.</typeparam>
    /// <typeparam name="TTempReturn7">The return type of the value7.</typeparam>
    /// <typeparam name="TTempReturn8">The return type of the value8.</typeparam>
    /// <typeparam name="TTempReturn9">The return type of the value9.</typeparam>
    /// <typeparam name="TTempReturn10">The return type of the value10.</typeparam>
    /// <typeparam name="TTempReturn11">The return type of the value11.</typeparam>
    /// <typeparam name="TTempReturn12">The return type of the value12.</typeparam>
    /// <typeparam name="TReturn">The return value of the observable. Generated from the conversion func.</typeparam>
    /// <returns>An observable that signals when the properties specified in the expressions have changed.</returns>
    /// <exception cref="ArgumentNullException">Either the property expression or the object to monitor is null.</exception>
    /// <exception cref="ArgumentException">If there is an issue with the property expression.</exception>
    public static IObservable<TReturn> WhenChanged<TObj, TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TTempReturn6, TTempReturn7, TTempReturn8, TTempReturn9, TTempReturn10, TTempReturn11, TTempReturn12, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, TTempReturn1>> propertyExpression1,
        Expression<Func<TObj, TTempReturn2>> propertyExpression2,
        Expression<Func<TObj, TTempReturn3>> propertyExpression3,
        Expression<Func<TObj, TTempReturn4>> propertyExpression4,
        Expression<Func<TObj, TTempReturn5>> propertyExpression5,
        Expression<Func<TObj, TTempReturn6>> propertyExpression6,
        Expression<Func<TObj, TTempReturn7>> propertyExpression7,
        Expression<Func<TObj, TTempReturn8>> propertyExpression8,
        Expression<Func<TObj, TTempReturn9>> propertyExpression9,
        Expression<Func<TObj, TTempReturn10>> propertyExpression10,
        Expression<Func<TObj, TTempReturn11>> propertyExpression11,
        Expression<Func<TObj, TTempReturn12>> propertyExpression12,
        Func<TTempReturn1, TTempReturn2, TTempReturn3, TTempReturn4, TTempReturn5, TTempReturn6, TTempReturn7, TTempReturn8, TTempReturn9, TTempReturn10, TTempReturn11, TTempReturn12, TReturn> conversionFunc)
        where TObj : class, INotifyPropertyChanged
    {
        var obs1 = objectToMonitor.WhenChanged(propertyExpression1);

        var obs2 = objectToMonitor.WhenChanged(propertyExpression2);

        var obs3 = objectToMonitor.WhenChanged(propertyExpression3);

        var obs4 = objectToMonitor.WhenChanged(propertyExpression4);

        var obs5 = objectToMonitor.WhenChanged(propertyExpression5);

        var obs6 = objectToMonitor.WhenChanged(propertyExpression6);

        var obs7 = objectToMonitor.WhenChanged(propertyExpression7);

        var obs8 = objectToMonitor.WhenChanged(propertyExpression8);

        var obs9 = objectToMonitor.WhenChanged(propertyExpression9);

        var obs10 = objectToMonitor.WhenChanged(propertyExpression10);

        var obs11 = objectToMonitor.WhenChanged(propertyExpression11);

        var obs12 = objectToMonitor.WhenChanged(propertyExpression12);

        return obs1.CombineLatest(obs2, obs3, obs4, obs5, obs6, obs7, obs8, obs9, obs10, obs11, obs12, conversionFunc);
    }
}
