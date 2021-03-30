// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reflection;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Builders
{
    /// <summary>
    /// Acts as a user-friendly interface to interact with the 'host' type in the generated compilation.
    /// </summary>
    public class HostProxy
    {
        private readonly object _source;
        private HostProxy _child;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostProxy"/> class.
        /// </summary>
        /// <param name="source">An instance of the <i>actual</i> host class.</param>
        public HostProxy(object source)
        {
            _source = source;
        }

        /// <summary>
        /// Gets the <i>actual</i> host object.
        /// </summary>
        public object Source => _source;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public object Value
        {
            get => ReflectionUtil.GetProperty(_source, nameof(Value));
            set => ReflectionUtil.SetProperty(_source, nameof(Value), value);
        }

        /// <summary>
        /// Gets or sets the child.
        /// </summary>
        public HostProxy Child
        {
            get => _child;

            set
            {
                _child = value;
                ReflectionUtil.SetProperty(_source, nameof(Child), value.Source);
            }
        }

        /// <summary>
        /// Gets the observable resulting from the WhenChanged invocation.
        /// </summary>
        /// <param name="onError">An action to invoke when the WhenChanged implementation doesn't generate correctly.</param>
        /// <returns>An observable.</returns>
        public IObservable<object> GetWhenChangedObservable(Action<Exception> onError)
        {
            try
            {
                return GetMethod(_source, WhenChangedHostBuilder.MethodName.GetWhenChangedObservable) as IObservable<object>;
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
                return GetMethod(_source, WhenChangedHostBuilder.MethodName.GetOneWayBindSubscription) as IDisposable;
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
                return GetMethod(_source, WhenChangedHostBuilder.MethodName.GetTwoWayBindSubscription) as IDisposable;
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                throw;
            }
        }

        private static object GetMethod(object target, string methodName)
        {
            return target.GetType().InvokeMember(
                methodName,
                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                target,
                Array.Empty<object>());
        }
    }
}
