// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Builders
{
    /// <summary>
    /// A builder for generating Bind() source code.
    /// </summary>
    public class BindHostBuilder : BaseUserSourceBuilder<BindHostBuilder>
    {
        private BaseUserSourceBuilder? _hostPropertyType;
        private Accessibility _targetPropertyAccess;
        private Accessibility _hostPropertyAccess;
        private string? _twoWayBindInvocation;
        private string? _oneWayBindInvocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindHostBuilder"/> class.
        /// </summary>
        public BindHostBuilder()
        {
            _hostPropertyType = null;
            _hostPropertyAccess = Accessibility.Public;
            _targetPropertyAccess = Accessibility.Public;
        }

        /// <summary>
        /// Gets the type name of the <b>Value</b> property.
        /// </summary>
        public string? HostPropertyTypeName => _hostPropertyType?.GetTypeName();

        /// <summary>
        /// Gets the type name of the <b>Value</b> property.
        /// </summary>
        public string? TargetPropertyTypeName { get; private set; }

        /// <summary>
        /// Sets the type of the <b>Value</b> property.
        /// </summary>
        /// <param name="value">A builder that represents a type.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithTargetPropertyType(BaseUserSourceBuilder value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            TargetPropertyTypeName = value.GetTypeName();
            return this;
        }

        /// <summary>
        /// Sets the type of the <b>Value</b> property.
        /// </summary>
        /// <param name="value">A builder that represents a type.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithTargetPropertyType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"'{nameof(value)}' cannot be null or whitespace.", nameof(value));
            }

            TargetPropertyTypeName = value;
            return this;
        }

        /// <summary>
        /// Sets the access modifier of the <b>Value</b> property.
        /// </summary>
        /// <param name="value">An access modifier.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithTargetPropertyAccess(Accessibility value)
        {
            _targetPropertyAccess = value;
            return this;
        }

        /// <summary>
        /// Sets the type of the <b>Value</b> property.
        /// </summary>
        /// <param name="value">A builder that represents a type.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithHostPropertyType(BaseUserSourceBuilder value)
        {
            _hostPropertyType = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        /// <summary>
        /// Sets the access modifier of the <b>Value</b> property.
        /// </summary>
        /// <param name="value">An access modifier.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithHostPropertyAccess(Accessibility value)
        {
            _hostPropertyAccess = value;
            return this;
        }

        /// <summary>
        /// Sets the Bind invocation.
        /// </summary>
        /// <param name="invocationKind">The invocation kind.</param>
        /// <param name="receiverKind">The receiver kind.</param>
        /// <param name="hostExpression">The host expression.</param>
        /// <param name="targetExpression">The target expression.</param>
        /// <param name="target">The target parameter.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithTwoWayInvocation(
            InvocationKind invocationKind,
            ReceiverKind receiverKind,
            Expression<Func<BindHostProxy, object>> hostExpression,
            Expression<Func<WhenChangedHostProxy, object>> targetExpression,
            string target = "targetModel")
        {
            if (hostExpression is null)
            {
                throw new ArgumentNullException(nameof(hostExpression));
            }

            if (targetExpression is null)
            {
                throw new ArgumentNullException(nameof(targetExpression));
            }

            _twoWayBindInvocation = GetTwoWayBindInvocation(invocationKind, receiverKind, hostExpression.ToString(), targetExpression.ToString(), target);
            return this;
        }

        /// <summary>
        /// Sets the Bind invocation.
        /// </summary>
        /// <param name="invocationKind">The invocation kind.</param>
        /// <param name="receiverKind">The receiver kind.</param>
        /// <param name="hostExpression">The host expression.</param>
        /// <param name="targetExpression">The target expression.</param>
        /// <param name="hostConvert">The host conversion function.</param>
        /// <param name="targetConvert">The target conversion function.</param>
        /// <param name="target">The target parameter.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithTwoWayInvocation(
            InvocationKind invocationKind,
            ReceiverKind receiverKind,
            Expression<Func<BindHostProxy, object>> hostExpression,
            Expression<Func<WhenChangedHostProxy, object>> targetExpression,
            Expression<Func<object, object, object>> hostConvert,
            Expression<Func<object, object, object>> targetConvert,
            string target = "targetModel")
        {
            if (hostExpression is null)
            {
                throw new ArgumentNullException(nameof(hostExpression));
            }

            if (targetExpression is null)
            {
                throw new ArgumentNullException(nameof(targetExpression));
            }

            if (hostConvert is null)
            {
                throw new ArgumentNullException(nameof(hostConvert));
            }

            if (targetConvert is null)
            {
                throw new ArgumentNullException(nameof(targetConvert));
            }

            _twoWayBindInvocation = GetTwoWayBindInvocation(invocationKind, receiverKind, hostExpression.ToString(), targetExpression.ToString(), target, hostConvert.ToString(), targetConvert.ToString());
            return this;
        }

        /// <summary>
        /// Sets the OneWayBind invocation.
        /// </summary>
        /// <param name="invocationKind">The invocation kind.</param>
        /// <param name="receiverKind">The receiver kind.</param>
        /// <param name="hostExpression">The host expression.</param>
        /// <param name="targetExpression">The target expression.</param>
        /// <param name="target">The target parameter.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithOneWayInvocation(
            InvocationKind invocationKind,
            ReceiverKind receiverKind,
            Expression<Func<BindHostProxy, object>> hostExpression,
            Expression<Func<WhenChangedHostProxy, object>> targetExpression,
            string target = "targetModel")
        {
            if (hostExpression is null)
            {
                throw new ArgumentNullException(nameof(hostExpression));
            }

            if (targetExpression is null)
            {
                throw new ArgumentNullException(nameof(targetExpression));
            }

            _oneWayBindInvocation = GetOneWayBindInvocation(invocationKind, receiverKind, hostExpression.ToString(), targetExpression.ToString(), target);
            return this;
        }

        /// <summary>
        /// Sets the OneWayBind invocation.
        /// </summary>
        /// <param name="invocationKind">The invocation kind.</param>
        /// <param name="receiverKind">The receiver kind.</param>
        /// <param name="hostExpression">The host expression.</param>
        /// <param name="targetExpression">The target expression.</param>
        /// <param name="hostConvert">The host conversion function.</param>
        /// <param name="target">The target parameter.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithOneWayInvocation(
            InvocationKind invocationKind,
            ReceiverKind receiverKind,
            Expression<Func<BindHostProxy, object>> hostExpression,
            Expression<Func<WhenChangedHostProxy, object>> targetExpression,
            Expression<Func<object, object, object>> hostConvert,
            string target = "targetModel")
        {
            if (hostExpression is null)
            {
                throw new ArgumentNullException(nameof(hostExpression));
            }

            if (targetExpression is null)
            {
                throw new ArgumentNullException(nameof(targetExpression));
            }

            if (hostConvert is null)
            {
                throw new ArgumentNullException(nameof(hostConvert));
            }

            if (string.IsNullOrEmpty(target))
            {
                throw new ArgumentException($"'{nameof(target)}' cannot be null or empty.", nameof(target));
            }

            _oneWayBindInvocation = GetOneWayBindInvocation(invocationKind, receiverKind, hostExpression.ToString(), targetExpression.ToString(), target, hostConvert.ToString());
            return this;
        }

        /// <inheritdoc/>
        public override IEnumerable<string> GetNamespaces() =>
            new[]
            {
                "System",
                "System.Collections.Generic",
                "System.ComponentModel",
                "System.Linq.Expressions",
                "System.Runtime.CompilerServices",
            };

        /// <inheritdoc/>
        protected override string CreateClass(string nestedClasses)
        {
            if (_hostPropertyType is null)
            {
                throw new InvalidOperationException("The Hostl property type is null.");
            }

            if (TargetPropertyTypeName is null || string.IsNullOrWhiteSpace(TargetPropertyTypeName))
            {
                throw new InvalidOperationException("The TargetPropertyTypeName is null");
            }

            var hostPropertyAccess = _hostPropertyAccess.ToFriendlyString();
            var hostPropertyTypeName = _hostPropertyType.GetTypeName().Replace('+', '.');
            var targetPropertyAccess = _targetPropertyAccess.ToFriendlyString();
            var targetPropertyTypeName = TargetPropertyTypeName.Replace('+', '.');
            var oneWayBindString = string.Empty;
            var twoWayBindString = string.Empty;

            if (_oneWayBindInvocation is not null)
            {
                oneWayBindString = @$"public IDisposable {MethodNames.GetBindOneWaySubscription}()
        {{
            var instance = this;
            return {_oneWayBindInvocation};
        }}";
            }

            if (_twoWayBindInvocation is not null)
            {
                twoWayBindString = @$"public IDisposable {MethodNames.GetBindTwoWaySubscription}()
        {{
            var instance = this;
            return {_twoWayBindInvocation};
        }}";
            }

            return $@"
    {ClassAccess.ToFriendlyString()} partial class {ClassName} : INotifyPropertyChanged
    {{
        private {HostPropertyTypeName} _host;
        private {targetPropertyTypeName} _target;

        public event PropertyChangedEventHandler PropertyChanged;

        {targetPropertyAccess} {targetPropertyTypeName} Target
        {{
            get => _target;
            set => RaiseAndSetIfChanged(ref _target, value);
        }}

        {hostPropertyAccess} {hostPropertyTypeName} Host
        {{
            get => _host;
            set => RaiseAndSetIfChanged(ref _host, value);
        }}
              
        {oneWayBindString}

        {twoWayBindString}

        public IObservable<object> {MethodNames.GetWhenChangedTargetObservable}()
        {{
            return this.WhenChanged(x => x.targetModel);
        }}

        public IObservable<object> {MethodNames.GetWhenChangedObservable}()
        {{
            return this.WhenChanged(x => x.Value);
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
        }

        private static string GetTwoWayBindInvocation(
            InvocationKind invocationKind,
            ReceiverKind targetModelKind,
            string hostArgs,
            string targetArgs,
            string hostName,
            string? hostConvertFunc = null,
            string? targetConvertFunc = null)
        {
            var receiver = targetModelKind == ReceiverKind.This ? "this" : "instance";

            if (hostConvertFunc is not null)
            {
                targetArgs = targetArgs + ", " + hostConvertFunc + ", " + targetConvertFunc;
            }

            return invocationKind == InvocationKind.MemberAccess ?
                $"{receiver}.Bind({hostName}, {hostArgs}, {targetArgs})" :
                $"BindExtensions.Bind({receiver}, {hostName}, {hostArgs}, {targetArgs})";
        }

        private static string GetOneWayBindInvocation(
            InvocationKind invocationKind,
            ReceiverKind targetModelKind,
            string hostArgs,
            string targetArgs,
            string hostName,
            string? hostConvertFunc = null)
        {
            var receiver = targetModelKind == ReceiverKind.This ? "this" : "instance";

            if (hostConvertFunc is not null)
            {
                targetArgs = targetArgs + ", " + hostConvertFunc;
            }

            return invocationKind == InvocationKind.MemberAccess ?
                $"{receiver}.OneWayBind({hostName}, {hostArgs}, {targetArgs})" :
                $"BindExtensions.OneWayBind({receiver}, {hostName}, {hostArgs}, {targetArgs})";
        }
    }
}
