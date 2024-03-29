﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
// <auto-generated />

namespace ReactiveMarbles.PropertyChanged.SourceGenerator;

internal static partial class Constants
{
    internal const string BindExtensionClassSource = @"// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
// <auto-generated />

#pragma warning disable

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

/// <summary>
/// Set of extension methods that handle binding.
/// </summary>
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[global::System.Diagnostics.DebuggerNonUserCode]
[Preserve(AllMembers=true)]
[global::System.Reflection.Obfuscation(Exclude=true)]
[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
internal static partial class BindingExtensions
{
    /// <summary>
    /// Performs one way binding between a property on the host to a target property.
    /// </summary>
    /// <param name=""fromObject"">The object which contains the host property.</param>
    /// <param name=""targetObject"">The object which contains the target property.</param>
    /// <param name=""fromProperty"">A expression to the host property.</param>
    /// <param name=""toProperty"">A expression to the target property.</param>
    /// <param name=""scheduler"">A scheduler for performing the binding on. Defaults to ImmediateScheduler.</param>
    /// <param name=""callerMemberName"">The caller of the method.</param>
    /// <param name=""callerFilePath"">The caller file path.</param>
    /// <param name=""callerLineNumber"">The caller line number.</param>
    /// <typeparam name=""TFrom"">The type of property the host is.</typeparam>
    /// <typeparam name=""TPropertyType"">The property types.</typeparam>
    /// <typeparam name=""TTarget"">The target property.</typeparam>
    /// <returns>A disposable which when disposed the binding will stop.</returns>
    /// <exception cref=""ArgumentException"">If there is a invalid expression.</exception>
    public static IDisposable BindOneWay<TFrom, TPropertyType, TTarget>(
        this TFrom fromObject,
        TTarget targetObject,
        Expression<Func<TFrom, TPropertyType>> fromProperty,
        Expression<Func<TTarget, TPropertyType>> toProperty,
        IScheduler scheduler = null,
        [CallerMemberName]string callerMemberName = null,
        [CallerFilePath]string callerFilePath = null,
        [CallerLineNumber]int callerLineNumber = 0)
        where TFrom : class, INotifyPropertyChanged
        {
            throw new Exception(""The impementation should have been generated."");
        }

    /// <summary>
    /// Performs one way binding between a property on the host to a target property.
    /// </summary>
    /// <param name=""fromObject"">The object which contains the host property.</param>
    /// <param name=""targetObject"">The object which contains the target property.</param>
    /// <param name=""fromProperty"">A expression to the host property.</param>
    /// <param name=""toProperty"">A expression to the target property.</param>
    /// <param name=""conversionFunc"">A converter which will convert the property from the host to the target property.</param>
    /// <param name=""scheduler"">A scheduler for performing the binding on. Defaults to ImmediateScheduler.</param>
    /// <param name=""callerMemberName"">The caller of the method.</param>
    /// <param name=""callerFilePath"">The caller file path.</param>
    /// <param name=""callerLineNumber"">The caller line number.</param>
    /// <typeparam name=""TFrom"">The type of property the host is.</typeparam>
    /// <typeparam name=""TFromProperty"">The property from type.</typeparam>
    /// <typeparam name=""TTarget"">The target property.</typeparam>
    /// <typeparam name=""TTargetProperty"">The property to type.</typeparam>
    /// <returns>A disposable which when disposed the binding will stop.</returns>
    /// <exception cref=""ArgumentException"">If there is a invalid expression.</exception>
    public static IDisposable BindOneWay<TFrom, TFromProperty, TTarget, TTargetProperty>(
        this TFrom fromObject,
        TTarget targetObject,
        Expression<Func<TFrom, TFromProperty>> fromProperty,
        Expression<Func<TTarget, TTargetProperty>> toProperty,
        Func<TFromProperty, TTargetProperty> conversionFunc,
        IScheduler scheduler = null,
        [CallerMemberName]string callerMemberName = null,
        [CallerFilePath]string callerFilePath = null,
        [CallerLineNumber]int callerLineNumber = 0)
        where TFrom : class, INotifyPropertyChanged
    {
        throw new Exception(""The impementation should have been generated."");
    }

    /// <summary>
    /// Performs one way binding between a property on the host to a target property.
    /// </summary>
    /// <param name=""fromObject"">The object which contains the host property.</param>
    /// <param name=""targetObject"">The object which contains the target property.</param>
    /// <param name=""fromProperty"">A expression to the host property.</param>
    /// <param name=""toProperty"">A expression to the target property.</param>
    /// <param name=""hostToTargetConv"">A converter which will convert the property from the host to the target property.</param>
    /// <param name=""targetToHostConv"">A converter which will convert the property from the target to the host property.</param>
    /// <param name=""scheduler"">A scheduler for performing the binding on. Defaults to ImmediateScheduler.</param>
    /// <param name=""callerMemberName"">The caller of the method.</param>
    /// <param name=""callerFilePath"">The caller file path.</param>
    /// <param name=""callerLineNumber"">The caller line number.</param>
    /// <typeparam name=""TFrom"">The type of property the host is.</typeparam>
    /// <typeparam name=""TFromProperty"">The property from type.</typeparam>
    /// <typeparam name=""TTarget"">The target property.</typeparam>
    /// <typeparam name=""TTargetProperty"">The property to type.</typeparam>
    /// <returns>A disposable which when disposed the binding will stop.</returns>
    /// <exception cref=""ArgumentException"">If there is a invalid expression.</exception>
    public static IDisposable BindTwoWay<TFrom, TFromProperty, TTarget, TTargetProperty>(
        this TFrom fromObject,
        TTarget targetObject,
        Expression<Func<TFrom, TFromProperty>> fromProperty,
        Expression<Func<TTarget, TTargetProperty>> toProperty,
        Func<TFromProperty, TTargetProperty> hostToTargetConv,
        Func<TTargetProperty, TFromProperty> targetToHostConv,
        IScheduler scheduler = null,
        [CallerMemberName]string callerMemberName = null,
        [CallerFilePath]string callerFilePath = null,
        [CallerLineNumber]int callerLineNumber = 0)
        where TFrom : class, INotifyPropertyChanged
        where TTarget : class, INotifyPropertyChanged
    {
        throw new Exception(""The impementation should have been generated."");
    }

    /// <summary>
    /// Performs one way binding between a property on the host to a target property.
    /// </summary>
    /// <param name=""fromObject"">The object which contains the host property.</param>
    /// <param name=""targetObject"">The object which contains the target property.</param>
    /// <param name=""fromProperty"">A expression to the host property.</param>
    /// <param name=""toProperty"">A expression to the target property.</param>
    /// <param name=""scheduler"">A scheduler for performing the binding on. Defaults to ImmediateScheduler.</param>
    /// <param name=""callerMemberName"">The caller of the method.</param>
    /// <param name=""callerFilePath"">The caller file path.</param>
    /// <param name=""callerLineNumber"">The caller line number.</param>
    /// <typeparam name=""TFrom"">The type of property the host is.</typeparam>
    /// <typeparam name=""TProperty"">The property from type.</typeparam>
    /// <typeparam name=""TTarget"">The target property.</typeparam>
    /// <returns>A disposable which when disposed the binding will stop.</returns>
    /// <exception cref=""ArgumentException"">If there is a invalid expression.</exception>
    public static IDisposable BindTwoWay<TFrom, TProperty, TTarget>(
        this TFrom fromObject,
        TTarget targetObject,
        Expression<Func<TFrom, TProperty>> fromProperty,
        Expression<Func<TTarget, TProperty>> toProperty,
        IScheduler scheduler = null,
        [CallerMemberName]string callerMemberName = null,
        [CallerFilePath]string callerFilePath = null,
        [CallerLineNumber]int callerLineNumber = 0)
        where TFrom : class, INotifyPropertyChanged
        where TTarget : class, INotifyPropertyChanged
    {
        throw new Exception(""The impementation should have been generated."");
    }
}";
}
