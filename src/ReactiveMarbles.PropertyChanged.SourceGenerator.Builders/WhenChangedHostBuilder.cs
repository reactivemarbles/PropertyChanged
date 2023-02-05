// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

/// <summary>
/// Simplifies the source code creation of the 'host' class.
/// </summary>
/// <remarks>'Host' refers to the class that contains a WhenChanged invocation.</remarks>
public class WhenChangedHostBuilder : BaseUserSourceBuilder<WhenChangedHostBuilder>
{
    private readonly string _methodName;
    private readonly string _extensionClassName;
    private WhenChangedHostBuilder? _externalReceiverTypeInfo;
    private Accessibility _propertyAccess;
    private Func<string> _propertyTypeNameFunc;
    private string? _invocation;

    /// <summary>
    /// Initializes a new instance of the <see cref="WhenChangedHostBuilder"/> class.
    /// </summary>
    /// <param name="methodName">The method name.</param>
    /// <param name="extensionClassName">The extension class name.</param>
    public WhenChangedHostBuilder(string methodName = "WhenChanged", string extensionClassName = "NotifyPropertyExtensions")
    {
        _methodName = methodName;
        _extensionClassName = extensionClassName;
        _propertyAccess = Accessibility.Public;
        _propertyTypeNameFunc = () => "string";
        WithInvocation(InvocationKind.MemberAccess, x => x.Value);
    }

    /// <summary>
    /// Gets the type name of the <b>Value</b> property.
    /// </summary>
    public string ValuePropertyTypeName => _propertyTypeNameFunc.Invoke();

    /// <summary>
    /// Initializes a new instance of the <see cref="WhenChangedHostBuilder"/> class for WhenChanging.
    /// </summary>
    /// <returns>An instance of the builder.</returns>
    public static WhenChangedHostBuilder Changing() => new("WhenChanging", "NotifyPropertyExtensions");

    /// <summary>
    /// Sets the type of the <b>Value</b> property.
    /// </summary>
    /// <param name="value">A builder that represents a type.</param>
    /// <returns>A reference to this builder.</returns>
    public WhenChangedHostBuilder WithPropertyType(BaseUserSourceBuilder value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        _propertyTypeNameFunc = value.GetTypeName;
        return this;
    }

    /// <summary>
    /// Sets the type of the <b>Value</b> property.
    /// </summary>
    /// <param name="value">A builder that represents a type.</param>
    /// <returns>A reference to this builder.</returns>
    public WhenChangedHostBuilder WithPropertyType(string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        _propertyTypeNameFunc = () => value;
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
    /// <param name="expression">The expression.</param>
    /// <param name="externalReceiverTypeInfo">The type info of an object that will invoke WhenChanged via instance rather than 'this'.</param>
    /// <returns>A reference to this builder.</returns>
    public WhenChangedHostBuilder WithInvocation(
        InvocationKind invocationKind,
        Expression<Func<WhenChangedHostProxy, object?>> expression,
        WhenChangedHostBuilder? externalReceiverTypeInfo = null)
    {
        if (expression is null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        _invocation = GetWhenChangedInvocation(invocationKind, externalReceiverTypeInfo, expression.ToString());
        return this;
    }

    /// <summary>
    /// Sets the WhenChanged invocation.
    /// </summary>
    /// <param name="invocation">The invocation.</param>
    /// <returns>A reference to this builder.</returns>
    public WhenChangedHostBuilder WithInvocation(string invocation)
    {
        _invocation = invocation;
        return this;
    }

    /// <summary>
    /// Sets the WhenChanged invocation.
    /// </summary>
    /// <param name="invocationKind">The invocation kind.</param>
    /// <param name="expression1">The first expression.</param>
    /// <param name="expression2">The second expression.</param>
    /// <param name="conversionFunc">The conversion function.</param>
    /// <param name="externalReceiverTypeInfo">The type info of an object that will invoke WhenChanged via instance rather than 'this'.</param>
    /// <returns>A reference to this builder.</returns>
    public WhenChangedHostBuilder WithInvocation(
        InvocationKind invocationKind,
        Expression<Func<WhenChangedHostProxy, object>> expression1,
        Expression<Func<WhenChangedHostProxy, object>> expression2,
        Expression<Func<object, object, object>> conversionFunc,
        WhenChangedHostBuilder? externalReceiverTypeInfo = null)
    {
        _invocation = GetWhenChangedInvocation(invocationKind, externalReceiverTypeInfo, $"{expression1}, {expression2}, {conversionFunc}");
        return this;
    }

    /// <summary>
    /// Sets the WhenChanged invocation.
    /// </summary>
    /// <param name="invocationKind">The invocation kind.</param>
    /// <param name="expression1">The first expression.</param>
    /// <param name="expression2">The second expression.</param>
    /// <param name="conversionFunc">The conversion function.</param>
    /// <param name="externalReceiverTypeInfo">The type info of an object that will invoke WhenChanged via instance rather than 'this'.</param>
    /// <returns>A reference to this builder.</returns>
    public WhenChangedHostBuilder WithInvocation(
        InvocationKind invocationKind,
        Expression<Func<WhenChangedHostProxy, object>> expression1,
        Expression<Func<WhenChangedHostProxy, object>> expression2,
        string conversionFunc,
        WhenChangedHostBuilder? externalReceiverTypeInfo = null)
    {
        _invocation = GetWhenChangedInvocation(invocationKind, externalReceiverTypeInfo, $"{expression1}, {expression2}, {conversionFunc}");
        return this;
    }

    /// <summary>
    /// Sets the WhenChanged invocation.
    /// </summary>
    /// <param name="depth">The depth of the expression chain.</param>
    /// <param name="invocationKind">The invocation kind.</param>
    /// <param name="externalReceiverTypeInfo">The type info of an object that will invoke WhenChanged via instance rather than 'this'.</param>
    /// <returns>A reference to this builder.</returns>
    public WhenChangedHostBuilder WithInvocation(
        int depth,
        InvocationKind invocationKind,
        WhenChangedHostBuilder? externalReceiverTypeInfo = null)
    {
        var expression = string.Join(".", Enumerable.Range(1, depth - 1).Select(_ => "Child").Prepend("x => x").Append("Value"));
        _invocation = GetWhenChangedInvocation(invocationKind, externalReceiverTypeInfo, expression);
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
        var propertyAccess = _propertyAccess.ToFriendlyString();
        var propertyTypeName = _propertyTypeNameFunc.Invoke();
        propertyTypeName = propertyTypeName.Replace('+', '.');

        var receiverProperty = string.Empty;
        if (_externalReceiverTypeInfo is not null)
        {
            var receiverAccess = _externalReceiverTypeInfo._propertyAccess.ToFriendlyString();
            receiverProperty = $"{receiverAccess} {_externalReceiverTypeInfo.GetTypeName()} Receiver {{ get; set; }}";
        }

        return $@"
    {ClassAccess.ToFriendlyString()} partial class {ClassName} : INotifyPropertyChanged, INotifyPropertyChanging
    {{
        private {propertyTypeName} _value;
        private {ClassName} _child;

        public event PropertyChangingEventHandler PropertyChanging;

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

        {receiverProperty}
        
        public IObservable<object> {MethodNames.GetWhenChangedObservable}()
        {{
            return {_invocation};
        }}

        protected void RaiseAndSetIfChanged<T>(ref T fieldValue, T value, [CallerMemberName] string propertyName = null)
        {{
            if (EqualityComparer<T>.Default.Equals(fieldValue, value))
            {{
                return;
            }}

            OnPropertyChanging(propertyName);
            fieldValue = value;
            OnPropertyChanged(propertyName);
        }}

        protected virtual void OnPropertyChanging(string propertyName)
        {{
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }}

        protected virtual void OnPropertyChanged(string propertyName)
        {{
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }}

        {nestedClasses}
    }}
";
    }

    private string GetWhenChangedInvocation(
        InvocationKind invocationKind,
        WhenChangedHostBuilder? externalReceiverTypeInfo,
        string args)
    {
        _externalReceiverTypeInfo = externalReceiverTypeInfo;
        var receiver = externalReceiverTypeInfo is null ? "this" : "Receiver";

        return invocationKind == InvocationKind.MemberAccess ?
            $"{receiver}.{_methodName}({args})" :
            $"{_extensionClassName}.{_methodName}({receiver}, {args})";
    }
}
