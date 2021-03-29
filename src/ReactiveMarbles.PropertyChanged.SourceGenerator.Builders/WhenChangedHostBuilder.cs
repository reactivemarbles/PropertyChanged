// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using ReactiveMarbles.PropertyChanged.SourceGenerator;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Builders
{
    /// <summary>
    /// Simplifies the source code creation of the 'host' class.
    /// </summary>
    /// <remarks>'Host' refers to the class that contains a WhenChanged invocation.</remarks>
    public class WhenChangedHostBuilder : BaseUserSourceBuilder<WhenChangedHostBuilder>
    {
        private BaseUserSourceBuilder _propertyType;
        private Accessibility _propertyAccess;
        private string _invocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="WhenChangedHostBuilder"/> class.
        /// </summary>
        public WhenChangedHostBuilder()
        {
            _propertyType = null;
            _propertyAccess = Accessibility.Public;
            WithInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Value);
        }

        /// <summary>
        /// Gets the type name of the <b>Value</b> property.
        /// </summary>
        public string ValuePropertyTypeName => _propertyType.GetTypeName();

        /// <summary>
        /// Sets the type of the <b>Value</b> property.
        /// </summary>
        /// <param name="value">A builder that represents a type.</param>
        /// <returns>A reference to this builder.</returns>
        public WhenChangedHostBuilder WithPropertyType(BaseUserSourceBuilder value)
        {
            _propertyType = value;
            return this;
        }

        /// <summary>
        /// Sets the access modifier of the <b>Value</b> property.
        /// </summary>
        /// <param name="value">An access modifier.</param>
        /// <returns>A reference to this builder.</returns>
        public WhenChangedHostBuilder WithPropertyAccess(Accessibility value)
        {
            _propertyAccess = value;
            return this;
        }

        /// <summary>
        /// Sets the WhenChanged invocation.
        /// </summary>
        /// <param name="invocationKind">The invocation kind.</param>
        /// <param name="receiverKind">The receiver kind.</param>
        /// <param name="expression">The expression.</param>
        /// <returns>A reference to this builder.</returns>
        public WhenChangedHostBuilder WithInvocation(
            InvocationKind invocationKind,
            ReceiverKind receiverKind,
            Expression<Func<HostProxy, object>> expression)
        {
            _invocation = GetWhenChangedInvocation(invocationKind, receiverKind, expression.ToString());
            return this;
        }

        /// <summary>
        /// Sets the WhenChanged invocation.
        /// </summary>
        /// <param name="invocationKind">The invocation kind.</param>
        /// <param name="receiverKind">The receiver kind.</param>
        /// <param name="expression1">The first expression.</param>
        /// <param name="expression2">The second expression.</param>
        /// <param name="conversionFunc">The conversion function.</param>
        /// <returns>A reference to this builder.</returns>
        public WhenChangedHostBuilder WithInvocation(
            InvocationKind invocationKind,
            ReceiverKind receiverKind,
            Expression<Func<HostProxy, object>> expression1,
            Expression<Func<HostProxy, object>> expression2,
            Expression<Func<object, object, object>> conversionFunc)
        {
            _invocation = GetWhenChangedInvocation(invocationKind, receiverKind, $"{expression1}, {expression2}, {conversionFunc}");
            return this;
        }

        /// <summary>
        /// Sets the WhenChanged invocation.
        /// </summary>
        /// <param name="depth">The depth of the expression chain.</param>
        /// <param name="invocationKind">The invocation kind.</param>
        /// <param name="receiverKind">The receiver kind.</param>
        /// <returns>A reference to this builder.</returns>
        public WhenChangedHostBuilder WithInvocation(
            int depth,
            InvocationKind invocationKind,
            ReceiverKind receiverKind)
        {
            var expression = string.Join(".", Enumerable.Range(1, depth - 1).Select(_ => "Child").Prepend("x => x").Append("Value"));
            _invocation = GetWhenChangedInvocation(invocationKind, receiverKind, expression);
            return this;
        }

        /// <inheritdoc/>
        public override IEnumerable<string> GetNamespaces()
        {
            return new[]
            {
                "System",
                "System.Collections.Generic",
                "System.ComponentModel",
                "System.Linq.Expressions",
                "System.Runtime.CompilerServices",
            };
        }

        /// <inheritdoc/>
        protected override string CreateClass(string nestedClasses)
        {
            var propertyAccess = _propertyAccess.ToFriendlyString();
            var propertyTypeName = _propertyType.GetTypeName().Replace('+', '.');

            var source = $@"
    {ClassAccess.ToFriendlyString()} partial class {ClassName} : INotifyPropertyChanged
    {{
        private {propertyTypeName} _value;
        private {ClassName} _child;

        public event PropertyChangedEventHandler PropertyChanged;

        {propertyAccess} {propertyTypeName} Value
        {{
            get => _value;
            set => RaiseAndSetIfChanged(ref _value, value);
        }}

        {propertyAccess} {ClassName} Child
        {{
            get => _child;
            set => RaiseAndSetIfChanged(ref _child, value);
        }}
        
        public IObservable<object> {MethodName.GetWhenChangedObservable}()
        {{
            var instance = this;
            return {_invocation};
        }}

        protected void RaiseAndSetIfChanged<T>(ref T fieldValue, T value, [CallerMemberName] string propertyName = null)
        {{
            if (EqualityComparer<T>.Default.Equals(fieldValue, value))
            {{
                return;
            }}

            fieldValue = value;
            OnPropertyChanged(propertyName);
        }}

        protected virtual void OnPropertyChanged(string propertyName)
        {{
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }}

        {nestedClasses}
    }}
";
            return source;
        }

        private static string GetWhenChangedInvocation(
            InvocationKind invocationKind,
            ReceiverKind receiverKind,
            string args)
        {
            var receiver = receiverKind == ReceiverKind.This ? "this" : "instance";

            string invocation;
            if (invocationKind == InvocationKind.MemberAccess)
            {
                invocation = $"{receiver}.WhenChanged({args})";
            }
            else
            {
                invocation = $"NotifyPropertyChangedExtensions.WhenChanged({receiver}, {args})";
            }

            return invocation;
        }

        internal static class MethodName
        {
            public const string GetWhenChangedObservable = "GetWhenChangedObservable";
            public const string GetOneWayBindSubscription = "GetOneWayBindSubscription";
            public const string GetTwoWayBindSubscription = "GetTwoWayBindSubscription";
        }
    }
}
