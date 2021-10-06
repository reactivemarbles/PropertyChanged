// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;

namespace ReactiveMarbles.PropertyChanged;

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
        if (propertyExpression is null)
        {
            throw new ArgumentNullException(nameof(propertyExpression));
        }

        var currentObservable = Observable.Return((INotifyPropertyChanged)objectToMonitor);

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
                return Observable.Return(memberInfo)
                    .CombineLatest(currentObservable, (mi, parent) => (memberInfo: mi, value: parent))
                    .Where(x => x.value is not null)
                    .Select(x => GenerateObservable(x.value, x.memberInfo, GetMemberFuncCache<INotifyPropertyChanged, TReturn>.GetCache(x.memberInfo)))
                    .Switch();
            }

            currentObservable = Observable.Return(memberInfo)
                .CombineLatest(currentObservable, (mi, parent) => (memberInfo: mi, value: parent))
                .Where(x => x.value is not null)
                .Select(x => GenerateObservable(x.value, x.memberInfo, GetMemberFuncCache<INotifyPropertyChanged, INotifyPropertyChanged>.GetCache(x.memberInfo)))
                .Switch();

            i++;
        }

        throw new ArgumentException("Invalid expression", nameof(propertyExpression));
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
    public static IObservable<(object Sender, TReturn Value)> WhenChangedWithSender<TObj, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, TReturn>> propertyExpression)
        where TObj : class, INotifyPropertyChanged
    {
        if (propertyExpression is null)
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
                return Observable.Return(memberInfo)
                    .CombineLatest(currentObservable, (mi, parent) => (memberInfo: mi, sender: parent.Sender, value: parent.Value))
                    .Where(x => x.value is not null)
                    .Select(x => GenerateObservableWithSender(x.value, x.memberInfo, GetMemberFuncCache<INotifyPropertyChanged, TReturn>.GetCache(x.memberInfo)))
                    .Switch();
            }

            currentObservable = Observable.Return(memberInfo)
                .CombineLatest(currentObservable, (mi, parent) => (memberInfo: mi, sender: parent.Sender, value: parent.Value))
                .Where(x => x.value is not null)
                .Select(x => GenerateObservableWithSender(x.value, x.memberInfo, GetMemberFuncCache<INotifyPropertyChanged, INotifyPropertyChanged>.GetCache(x.memberInfo)))
                .Switch();

            i++;
        }

        throw new ArgumentException("Invalid expression", nameof(propertyExpression));
    }

    private static IObservable<T> GenerateObservable<T>(
        INotifyPropertyChanged parent,
        MemberInfo memberInfo,
        Func<INotifyPropertyChanged, T> getter)
    {
        var memberName = memberInfo.Name;
        return Observable.Create<T>(
                observer =>
                {
                    void Handler(object sender, PropertyChangedEventArgs e)
                    {
                        if (e.PropertyName == memberName)
                        {
                            observer.OnNext(getter(parent));
                        }
                    }

                    parent.PropertyChanged += Handler;

                    return Disposable.Create(parent, x => x.PropertyChanged -= Handler);
                })
            .StartWith(getter(parent));
    }

    private static IObservable<(object Sender, T Value)> GenerateObservableWithSender<T>(
        INotifyPropertyChanged parent,
        MemberInfo memberInfo,
        Func<INotifyPropertyChanged, T> getter)
    {
        var memberName = memberInfo.Name;
        return Observable.Create<(object Sender, T Value)>(
                observer =>
                {
                    void Handler(object sender, PropertyChangedEventArgs e)
                    {
                        if (e.PropertyName == memberName)
                        {
                            observer.OnNext((sender, getter(parent)));
                        }
                    }

                    parent.PropertyChanged += Handler;

                    return Disposable.Create(parent, x => x.PropertyChanged -= Handler);
                })
            .StartWith((parent, getter(parent)));
    }
}
