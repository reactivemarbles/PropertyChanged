// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

/// <summary>
/// Proxies a Bind host.
/// </summary>
public class BindHostProxy : HostProxyBase
{
    private WhenChangedHostProxy? _viewModelProxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="BindHostProxy"/> class.
    /// </summary>
    /// <param name="source">The source to proxy.</param>
    public BindHostProxy(object source)
        : base(source)
    {
    }

    /// <summary>
    /// Gets or sets the view model value.
    /// </summary>
    public WhenChangedHostProxy? ViewModel
    {
        get => _viewModelProxy;
        set
        {
            _viewModelProxy = value;
            ReflectionUtil.SetProperty(Source, nameof(ViewModel), value?.Source);
        }
    }

    /// <summary>
    /// Gets or sets the view value.
    /// </summary>
    public object? Value
    {
        get => ReflectionUtil.GetProperty(Source, nameof(Value));
        set => ReflectionUtil.SetProperty(Source, nameof(Value), value);
    }

    /// <summary>
    /// Gets the observable resulting from the WhenChanged view model invocation.
    /// </summary>
    /// <param name="onError">An action to invoke when the WhenChanged implementation doesn't generate correctly.</param>
    /// <returns>An observable.</returns>
    public IObservable<object> GetViewModelWhenChangedObservable(Action<Exception> onError)
    {
        try
        {
            var methodInstance = GetMethod(Source, MethodNames.GetWhenChangedObservable) ?? throw new InvalidOperationException("Must have valid method");
            return (IObservable<object>)methodInstance;
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            throw;
        }
    }

    /// <summary>
    /// Gets the observable resulting from the WhenChanged view invocation.
    /// </summary>
    /// <param name="onError">An action to invoke when the WhenChanged implementation doesn't generate correctly.</param>
    /// <returns>An observable.</returns>
    public IObservable<object> GetWhenChangedObservable(Action<Exception> onError)
    {
        try
        {
            var methodInstance = GetMethod(Source, MethodNames.GetWhenChangedObservable) ?? throw new InvalidOperationException("Must have valid method");

            return (IObservable<object>)methodInstance;
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            throw;
        }
    }

    /// <summary>
    /// Gets the observable resulting from the OneWayBind invocation.
    /// </summary>
    /// <param name="onError">An action to invoke when the OneWayBind implementation doesn't generate correctly.</param>
    /// <returns>An observable.</returns>
    public IDisposable GetOneWayBindSubscription(Action<Exception> onError)
    {
        try
        {
            var methodInstance = GetMethod(Source, MethodNames.GetBindOneWaySubscription) ?? throw new InvalidOperationException("Must have valid method");

            return (IDisposable)methodInstance;
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            throw;
        }
    }

    /// <summary>
    /// Gets the observable resulting from the Bind invocation.
    /// </summary>
    /// <param name="onError">An action to invoke when the Bind implementation doesn't generate correctly.</param>
    /// <returns>An observable.</returns>
    public IDisposable GetTwoWayBindSubscription(Action<Exception> onError)
    {
        try
        {
            var methodInstance = GetMethod(Source, MethodNames.GetBindTwoWaySubscription) ?? throw new InvalidOperationException("Must have valid method");

            return (IDisposable)methodInstance;
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            throw;
        }
    }
}
