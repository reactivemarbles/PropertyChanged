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
    public class WhenChangedHostProxy : HostProxyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WhenChangedHostProxy"/> class.
        /// </summary>
        /// <param name="source">The source object to proxy.</param>
        public WhenChangedHostProxy(object source)
            : base(source)
        {
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public object Value
        {
            get => ReflectionUtil.GetProperty(Source, nameof(Value));
            set => ReflectionUtil.SetProperty(Source, nameof(Value), value);
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
                return GetMethod(Source, MethodNames.GetWhenChangedObservable) as IObservable<object>;
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                throw;
            }
        }
    }
}
