// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Builders
{
    /// <summary>
    /// A base class for the host proxy which allows interaction with a roslyn compiled class.
    /// </summary>
    public abstract class HostProxyBase
    {
        private WhenChangedHostProxy _child;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostProxyBase"/> class.
        /// </summary>
        /// <param name="source">An instance of the <i>actual</i> host class.</param>
        public HostProxyBase(object source)
        {
            Source = source;
        }

        /// <summary>
        /// Gets the <i>actual</i> host object.
        /// </summary>
        public object Source { get; }

        /// <summary>
        /// Gets or sets the child.
        /// </summary>
        public WhenChangedHostProxy Child
        {
            get => _child;

            set
            {
                _child = value;
                ReflectionUtil.SetProperty(Source, nameof(Child), value?.Source);
            }
        }

        internal static object GetMethod(object target, string methodName)
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
