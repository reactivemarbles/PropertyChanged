// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reflection;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    internal class HostProxy
    {
        private readonly object _source;
        private HostProxy _child;

        public HostProxy(object source)
        {
            _source = source;
        }

        public object Source => _source;

        public object Value
        {
            get => ReflectionUtil.GetProperty(_source, nameof(Value));
            set => ReflectionUtil.SetProperty(_source, nameof(Value), value);
        }

        public HostProxy Child
        {
            get => _child;

            set
            {
                _child = value;
                ReflectionUtil.SetProperty(_source, nameof(Child), value.Source);
            }
        }

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

        public IDisposable GetOneWayBindSubscription()
        {
            return GetMethod(_source, WhenChangedHostBuilder.MethodName.GetOneWayBindSubscription) as IDisposable;
        }

        public IDisposable GetTwoWayBindSubscription()
        {
            return GetMethod(_source, WhenChangedHostBuilder.MethodName.GetTwoWayBindSubscription) as IDisposable;
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
