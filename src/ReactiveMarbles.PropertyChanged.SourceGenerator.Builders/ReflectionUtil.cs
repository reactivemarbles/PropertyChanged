// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reflection;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

/// <summary>
/// Utility for common reflection operations.
/// </summary>
public static class ReflectionUtil
{
    /// <summary>
    /// Sets a property on a target object.
    /// </summary>
    /// <param name="target">The object to invoke the setter on.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value to assign.</param>
    public static void SetProperty(object target, string propertyName, object? value)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException($"'{nameof(propertyName)}' cannot be null or whitespace.", nameof(propertyName));
        }

        target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(target, value);
    }

    /// <summary>
    /// Gets a property on a target object.
    /// </summary>
    /// <param name="target">The object to invoke the getter on.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The value of the property.</returns>
    public static object? GetProperty(object target, string propertyName)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException($"'{nameof(propertyName)}' cannot be null or whitespace.", nameof(propertyName));
        }

        return target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(target);
    }
}