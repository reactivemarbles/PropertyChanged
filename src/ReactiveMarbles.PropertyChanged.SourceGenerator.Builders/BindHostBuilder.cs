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
        private BaseUserSourceBuilder _viewModelPropertyType;
        private Accessibility _propertyAccess;
        private Accessibility _viewModelPropertyAccess;
        private string _twoWayBindInvocation;
        private string _oneWayBindInvocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindHostBuilder"/> class.
        /// </summary>
        public BindHostBuilder()
        {
            _viewModelPropertyType = null;
            _viewModelPropertyAccess = Accessibility.Public;
            _propertyAccess = Accessibility.Public;
        }

        /// <summary>
        /// Gets the type name of the <b>Value</b> property.
        /// </summary>
        public string ViewModelPropertyTypeName => _viewModelPropertyType.GetTypeName();

        /// <summary>
        /// Gets the type name of the <b>Value</b> property.
        /// </summary>
        public string PropertyTypeName { get; private set; }

        /// <summary>
        /// Sets the type of the <b>Value</b> property.
        /// </summary>
        /// <param name="value">A builder that represents a type.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithPropertyType(BaseUserSourceBuilder value)
        {
            PropertyTypeName = value.GetTypeName();
            return this;
        }

        /// <summary>
        /// Sets the type of the <b>Value</b> property.
        /// </summary>
        /// <param name="value">A builder that represents a type.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithPropertyType(string value)
        {
            PropertyTypeName = value;
            return this;
        }

        /// <summary>
        /// Sets the access modifier of the <b>Value</b> property.
        /// </summary>
        /// <param name="value">An access modifier.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithPropertyAccess(Accessibility value)
        {
            _propertyAccess = value;
            return this;
        }

        /// <summary>
        /// Sets the type of the <b>Value</b> property.
        /// </summary>
        /// <param name="value">A builder that represents a type.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithViewModelPropertyType(BaseUserSourceBuilder value)
        {
            _viewModelPropertyType = value;
            return this;
        }

        /// <summary>
        /// Sets the access modifier of the <b>Value</b> property.
        /// </summary>
        /// <param name="value">An access modifier.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithViewModelPropertyAccess(Accessibility value)
        {
            _viewModelPropertyAccess = value;
            return this;
        }

        /// <summary>
        /// Sets the Bind invocation.
        /// </summary>
        /// <param name="invocationKind">The invocation kind.</param>
        /// <param name="receiverKind">The receiver kind.</param>
        /// <param name="viewModelExpression">The view model expression.</param>
        /// <param name="viewExpression">The view expression.</param>
        /// <param name="target">The target parameter.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithTwoWayInvocation(
            InvocationKind invocationKind,
            ReceiverKind receiverKind,
            Expression<Func<BindHostProxy, object>> viewModelExpression,
            Expression<Func<WhenChangedHostProxy, object>> viewExpression,
            string target = "ViewModel")
        {
            _twoWayBindInvocation = GetTwoWayBindInvocation(invocationKind, receiverKind, viewModelExpression.ToString(), viewExpression.ToString(), target);
            return this;
        }

        /// <summary>
        /// Sets the Bind invocation.
        /// </summary>
        /// <param name="invocationKind">The invocation kind.</param>
        /// <param name="receiverKind">The receiver kind.</param>
        /// <param name="viewModelExpression">The view model expression.</param>
        /// <param name="viewExpression">The view expression.</param>
        /// <param name="viewModelConvert">The view model conversion function.</param>
        /// <param name="viewConvert">The view conversion function.</param>
        /// <param name="target">The target parameter.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithTwoWayInvocation(
            InvocationKind invocationKind,
            ReceiverKind receiverKind,
            Expression<Func<BindHostProxy, object>> viewModelExpression,
            Expression<Func<WhenChangedHostProxy, object>> viewExpression,
            Expression<Func<object, object, object>> viewModelConvert,
            Expression<Func<object, object, object>> viewConvert,
            string target = "ViewModel")
        {
            _twoWayBindInvocation = GetTwoWayBindInvocation(invocationKind, receiverKind, viewModelExpression.ToString(), viewExpression.ToString(), target, viewModelConvert.ToString(), viewConvert.ToString());
            return this;
        }

        /// <summary>
        /// Sets the OneWayBind invocation.
        /// </summary>
        /// <param name="invocationKind">The invocation kind.</param>
        /// <param name="receiverKind">The receiver kind.</param>
        /// <param name="viewModelExpression">The view model expression.</param>
        /// <param name="viewExpression">The view expression.</param>
        /// <param name="target">The target parameter.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithOneWayInvocation(
            InvocationKind invocationKind,
            ReceiverKind receiverKind,
            Expression<Func<BindHostProxy, object>> viewModelExpression,
            Expression<Func<WhenChangedHostProxy, object>> viewExpression,
            string target = "ViewModel")
        {
            _oneWayBindInvocation = GetOneWayBindInvocation(invocationKind, receiverKind, viewModelExpression.ToString(), viewExpression.ToString(), target);
            return this;
        }

        /// <summary>
        /// Sets the OneWayBind invocation.
        /// </summary>
        /// <param name="invocationKind">The invocation kind.</param>
        /// <param name="receiverKind">The receiver kind.</param>
        /// <param name="viewModelExpression">The view model expression.</param>
        /// <param name="viewExpression">The view expression.</param>
        /// <param name="viewModelConvert">The view model conversion function.</param>
        /// <param name="target">The target parameter.</param>
        /// <returns>A reference to this builder.</returns>
        public BindHostBuilder WithOneWayInvocation(
            InvocationKind invocationKind,
            ReceiverKind receiverKind,
            Expression<Func<BindHostProxy, object>> viewModelExpression,
            Expression<Func<WhenChangedHostProxy, object>> viewExpression,
            Expression<Func<object, object, object>> viewModelConvert,
            string target = "ViewModel")
        {
            _oneWayBindInvocation = GetOneWayBindInvocation(invocationKind, receiverKind, viewModelExpression.ToString(), viewExpression.ToString(), target, viewModelConvert.ToString());
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
            var viewModelPropertyAccess = _viewModelPropertyAccess.ToFriendlyString();
            var viewModelPropertyTypeName = _viewModelPropertyType.GetTypeName().Replace('+', '.');
            var propertyAccess = _propertyAccess.ToFriendlyString();
            var propertyTypeName = PropertyTypeName.Replace('+', '.');
            var oneWayBindString = string.Empty;
            var twoWayBindString = string.Empty;

            if (_oneWayBindInvocation != null)
            {
                oneWayBindString = @$"public IDisposable {MethodNames.GetOneWayBindSubscription}()
        {{
            var instance = this;
            return {_oneWayBindInvocation};
        }}";
            }

            if (_twoWayBindInvocation != null)
            {
                twoWayBindString = @$"public IDisposable {MethodNames.GetTwoWayBindSubscription}()
        {{
            var instance = this;
            return {_twoWayBindInvocation};
        }}";
            }

            return $@"
    {ClassAccess.ToFriendlyString()} partial class {ClassName} : INotifyPropertyChanged
    {{
        private {viewModelPropertyTypeName} _viewModel;
        private {propertyTypeName} _value;

        public event PropertyChangedEventHandler PropertyChanged;

        {propertyAccess} {propertyTypeName} Value
        {{
            get => _value;
            set => RaiseAndSetIfChanged(ref _value, value);
        }}

        {viewModelPropertyAccess} {viewModelPropertyTypeName} ViewModel
        {{
            get => _viewModel;
            set => RaiseAndSetIfChanged(ref _viewModel, value);
        }}
              
        {oneWayBindString}

        {twoWayBindString}

        public IObservable<object> {MethodNames.GetWhenChangedViewModelObservable}()
        {{
            return this.WhenChanged(x => x.ViewModel);
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
            ReceiverKind viewModelKind,
            string viewModelArgs,
            string viewArgs,
            string targetName,
            string viewModelConvertFunc = null,
            string viewConvertFunc = null)
        {
            var receiver = viewModelKind == ReceiverKind.This ? "this" : "instance";

            if (viewModelConvertFunc != null)
            {
                viewArgs = viewArgs + ", " + viewModelConvertFunc + ", " + viewConvertFunc;
            }

            return invocationKind == InvocationKind.MemberAccess ?
                $"{receiver}.Bind({targetName}, {viewModelArgs}, {viewArgs})" :
                $"BindExtensions.Bind({receiver}, {targetName}, {viewModelArgs}, {viewArgs})";
        }

        private static string GetOneWayBindInvocation(
            InvocationKind invocationKind,
            ReceiverKind viewModelKind,
            string viewModelArgs,
            string viewArgs,
            string targetName,
            string viewModelConvertFunc = null)
        {
            var receiver = viewModelKind == ReceiverKind.This ? "this" : "instance";

            if (viewModelConvertFunc != null)
            {
                viewArgs = viewArgs + ", " + viewModelConvertFunc;
            }

            return invocationKind == InvocationKind.MemberAccess ?
                $"{receiver}.OneWayBind({targetName}, {viewModelArgs}, {viewArgs})" :
                $"BindExtensions.OneWayBind({receiver}, {targetName}, {viewModelArgs}, {viewArgs})";
        }
    }
}
