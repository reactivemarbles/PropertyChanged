// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    internal class MockUserSourceBuilder
    {
        private string _classAccessModifier;
        private string _namespaceName;
        private string _className;
        private string _propertyAccessModifier;
        private string _valuePropertyTypeName;
        private CustomTypeInfoForValueProperty _customTypeInfoForValueProperty;
        private string _outerClassAccessModifier;
        private List<string> _invocations;

        public MockUserSourceBuilder(InvocationKind invocationKind, ReceiverKind receiverKind, ExpressionForm expressionForm, int depth)
        {
            _classAccessModifier = "public";
            _namespaceName = "Sample";
            _className = "SampleClass";
            _propertyAccessModifier = "public";
            _valuePropertyTypeName = "string";
            _invocations = new List<string>();

            AndWhenChanged(invocationKind, receiverKind, expressionForm, depth);
        }

        public MockUserSourceBuilder ClassAccessModifier(string value)
        {
            _classAccessModifier = value;
            return this;
        }

        public MockUserSourceBuilder PropertyAccessModifier(string value)
        {
            _propertyAccessModifier = value;
            return this;
        }

        public MockUserSourceBuilder NamespaceName(string value)
        {
            _namespaceName = value;
            return this;
        }

        public MockUserSourceBuilder ClassName(string value)
        {
            _className = value;
            return this;
        }

        public MockUserSourceBuilder OuterClassAccessModifier(string value)
        {
            _outerClassAccessModifier = value;
            return this;
        }

        public MockUserSourceBuilder GetTypeName(out string typeName)
        {
            if (string.IsNullOrEmpty(_outerClassAccessModifier))
            {
                typeName = string.IsNullOrEmpty(_namespaceName) ? _className : $"{_namespaceName}.{_className}";
            }
            else
            {
                typeName = string.IsNullOrEmpty(_namespaceName) ? $"OuterClass+{_className}" : $"{_namespaceName}.OuterClass+{_className}";
            }

            return this;
        }

        public MockUserSourceBuilder AddCustomTypeForValueProperty(string className, string accessModifier, out string typeName)
        {
            _customTypeInfoForValueProperty = new CustomTypeInfoForValueProperty(className, accessModifier, false);
            typeName = string.IsNullOrEmpty(_namespaceName) ? $"{className}" : $"{_namespaceName}.{className}";
            return this;
        }

        public MockUserSourceBuilder AddCustomNestedTypeForValueProperty(string className, string accessModifier, out string typeName)
        {
            _customTypeInfoForValueProperty = new CustomTypeInfoForValueProperty(className, accessModifier, true);
            typeName = string.IsNullOrEmpty(_namespaceName) ? $"{_className}+{className}" : $"{_namespaceName}.{_className}+{className}";
            return this;
        }

        public MockUserSourceBuilder AndWhenChanged(InvocationKind invocationKind, ReceiverKind receiverKind, ExpressionForm expressionForm, int depth)
        {
            var receiver = receiverKind == ReceiverKind.This ? "this" : "instance";
            var expression = expressionForm switch
            {
                ExpressionForm.Inline => string.Join(".", Enumerable.Range(1, depth - 1).Select(_ => "Child").Prepend("x => x").Append("Value")),
                ExpressionForm.Property => "MyExpression",
                ExpressionForm.Method => "GetExpression()",
                ExpressionForm.BodyIncludesIndexer => depth < 2 ? "x => x.Values[0]" : string.Join(".", Enumerable.Range(1, depth - 2).Select(_ => "Child").Prepend("x => x.Children[0]").Append("Value")),
                ExpressionForm.BodyIncludesMethodInvocation => depth < 2 ? "x => x.GetValue()" : string.Join(".", Enumerable.Range(1, depth - 2).Select(_ => "Child").Prepend("x => x.GetChild()").Append("Value")),
                ExpressionForm.BodyExcludesLambdaParam => depth < 2 ? "x => Value" : "x => " + string.Join(".", Enumerable.Range(1, depth - 2).Select(_ => "Child").Append("Value")),
                _ => throw new InvalidOperationException($"{nameof(ExpressionForm)} [{expressionForm}] is not supported."),
            };

            string invocation;
            if (invocationKind == InvocationKind.MemberAccess)
            {
                invocation = $"{receiver}.WhenChanged({expression})";
            }
            else
            {
                invocation = $"NotifyPropertyChangedExtensions.WhenChanged({receiver}, {expression})";
            }

            _invocations.Add(invocation);

            return this;
        }

        public string Build()
        {
            if (!string.IsNullOrEmpty(_outerClassAccessModifier))
            {
                return BuildNested();
            }

            var customValuePropertyTypeSource = string.Empty;
            var customValuePropertyNestedTypeSource = string.Empty;

            if (_customTypeInfoForValueProperty != null)
            {
                var (className, accessModifier, isNested) = _customTypeInfoForValueProperty;

                _valuePropertyTypeName = className;
                if (isNested)
                {
                    _valuePropertyTypeName = _className + "." + _valuePropertyTypeName;
                    customValuePropertyNestedTypeSource = $@"
{accessModifier} class {className}
{{
}}
";
                }
                else
                {
                    customValuePropertyTypeSource = $@"
{accessModifier} class {className}
{{
}}
";
                }

                if (!string.IsNullOrEmpty(_namespaceName))
                {
                    _valuePropertyTypeName = _namespaceName + "." + _valuePropertyTypeName;
                }

                if (accessModifier == "internal" && _propertyAccessModifier == "public")
                {
                    _propertyAccessModifier = "internal";
                }
                else if (accessModifier == "protected" && (_propertyAccessModifier == "public" || _propertyAccessModifier == "internal"))
                {
                    _propertyAccessModifier = "protected";
                }
                else if (accessModifier == "private" && (_propertyAccessModifier == "public" || _propertyAccessModifier == "internal" || _propertyAccessModifier == "protected"))
                {
                    _propertyAccessModifier = "private";
                }
            }

            var source = $@"
    {_classAccessModifier} partial class {_className} : INotifyPropertyChanged
    {{
        private {_valuePropertyTypeName} _value;
        private {_className} _child;

        public event PropertyChangedEventHandler PropertyChanged;

        {_propertyAccessModifier} {_valuePropertyTypeName} Value
        {{
            get => _value;
            set => RaiseAndSetIfChanged(ref _value, value);
        }}

        {_propertyAccessModifier} {_className} Child
        {{
            get => _child;
            set => RaiseAndSetIfChanged(ref _child, value);
        }}

        {_propertyAccessModifier} {_valuePropertyTypeName}[] Values {{ get; }}

        {_propertyAccessModifier} Expression<Func<{_className}, {_valuePropertyTypeName}>> MyExpression = x => x.Value;

        {_propertyAccessModifier} Expression<Func<{_className}, {_valuePropertyTypeName}>> GetExpression() => x => x.Value;

        {_propertyAccessModifier} {_valuePropertyTypeName} GetValue() => Value;

        {_propertyAccessModifier} IObservable<{_valuePropertyTypeName}> GetWhenChangedObservable()
        {{
            var instance = this;
            return GetWhenChangedObservables()[0];
        }}

        {_propertyAccessModifier} IObservable<{_valuePropertyTypeName}>[] GetWhenChangedObservables()
        {{
            var instance = this;
            return new IObservable<{_valuePropertyTypeName}>[]
            {{
                {string.Join(",\n", _invocations)},
            }};
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
{customValuePropertyNestedTypeSource}
    }}
";

            if (!string.IsNullOrEmpty(_namespaceName))
            {
                source = $@"
namespace {_namespaceName}
{{
{source}
{customValuePropertyTypeSource}
}}
";
            }
            else
            {
                source += '\n' + customValuePropertyTypeSource;
            }

            var usingClauses = @"
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
";

            return source.Insert(0, usingClauses);
        }

        public string BuildTest()
        {
            return $@"
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

public class MyType : INotifyPropertyChanged
{{
    private Sample1.Output _output1;
    private Sample2.Output _output2;
    private MyType _child;

    public event PropertyChangedEventHandler PropertyChanged;

    public Sample1.Output Output1
    {{
        get => _output1;
        set => RaiseAndSetIfChanged(ref _output1, value);
    }}

    public Sample2.Output Output2
    {{
        get => _output2;
        set => RaiseAndSetIfChanged(ref _output2, value);
    }}

    public MyType Child
    {{
        get => _child;
        set => RaiseAndSetIfChanged(ref _child, value);
    }}

    public IObservable<Sample1.Output>[] GetWhenChangedObservables1()
    {{
        var instance = this;
        return new IObservable<Sample1.Output>[]
        {{
            this.WhenChanged(x => x.Output1),
            this.WhenChanged(x => x.Child.Output1),
        }};
    }}

    public IObservable<Sample2.Output>[] GetWhenChangedObservables2()
    {{
        var instance = this;
        return new IObservable<Sample2.Output>[]
        {{
            this.WhenChanged(x => x.Output2),
            this.WhenChanged(x => x.Child.Output2),
        }};
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
}}

namespace Sample1
{{
    public class Output
    {{
    }}
}}

namespace Sample2
{{
    public class Output
    {{
    }}
}}
";
        }

        public string BuildMultiExpressionVersion()
        {
            return $@"
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

public partial class SampleClass : INotifyPropertyChanged
{{
    private MyClass _value;
    private string _stringValue;
    private SampleClass _child;

    public event PropertyChangedEventHandler PropertyChanged;

    protected MyClass Value
    {{
        get => _value;
        set => RaiseAndSetIfChanged(ref _value, value);
    }}

    public string StringValue
    {{
        get => _stringValue;
        set => RaiseAndSetIfChanged(ref _stringValue, value);
    }}

    public SampleClass Child
    {{
        get => _child;
        set => RaiseAndSetIfChanged(ref _child, value);
    }}

    public IObservable<MyClass> GetWhenChangedObservable()
    {{
        return this.WhenChanged(x => x.Value, x => x.StringValue, (a, b) => a);
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
}}

public class MyClass
{{
}}
";
        }

        public string BuildMultiExpressionVersionNestedOutputType()
        {
            return $@"
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

public partial class SampleClass : INotifyPropertyChanged
{{
    private MyClass _value;
    private string _stringValue;
    private SampleClass _child;

    public event PropertyChangedEventHandler PropertyChanged;

    protected MyClass Value
    {{
        get => _value;
        set => RaiseAndSetIfChanged(ref _value, value);
    }}

    public string StringValue
    {{
        get => _stringValue;
        set => RaiseAndSetIfChanged(ref _stringValue, value);
    }}

    public SampleClass Child
    {{
        get => _child;
        set => RaiseAndSetIfChanged(ref _child, value);
    }}

    protected IObservable<MyClass> GetWhenChangedObservable()
    {{
        return this.WhenChanged(x => x.Value, x => x.StringValue, (a, b) => a);
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

    protected class MyClass
    {{
    }}
}}
";
        }

        private string BuildNested()
        {
            var source = $@"
{_classAccessModifier} partial class {_className} : INotifyPropertyChanged
{{
    private {_valuePropertyTypeName} _value;
    private {_className} _child;

    public event PropertyChangedEventHandler PropertyChanged;

    {_propertyAccessModifier} {_valuePropertyTypeName} Value
    {{
        get => _value;
        set => RaiseAndSetIfChanged(ref _value, value);
    }}

    {_propertyAccessModifier} {_className} Child
    {{
        get => _child;
        set => RaiseAndSetIfChanged(ref _child, value);
    }}

    {_propertyAccessModifier} IObservable<{_valuePropertyTypeName}> GetWhenChangedObservable()
    {{
        var instance = this;
        return GetWhenChangedObservables()[0];
    }}

    {_propertyAccessModifier} IObservable<{_valuePropertyTypeName}>[] GetWhenChangedObservables()
    {{
        var instance = this;
        return new IObservable<{_valuePropertyTypeName}>[]
        {{
            {string.Join(",\n", _invocations)},
        }};
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
}}
";

            source = $@"
    {_outerClassAccessModifier} partial class OuterClass
    {{
{source}
    }}
";

            if (!string.IsNullOrEmpty(_namespaceName))
            {
                source = $@"
namespace {_namespaceName}
{{
{source}
}}
";
            }

            var usingClauses = @"
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
";

            return source.Insert(0, usingClauses);
        }

        private class CustomTypeInfoForValueProperty
        {
            public CustomTypeInfoForValueProperty(string className, string accessModifier, bool isNested)
            {
                ClassName = className;
                AccessModifier = accessModifier;
                IsNested = isNested;
            }

            public string ClassName { get; }

            public string AccessModifier { get; }

            public bool IsNested { get; }

            internal void Deconstruct(out string className, out string accessModifier, out bool isNested)
            {
                className = ClassName;
                accessModifier = AccessModifier;
                isNested = IsNested;
            }
        }
    }
}
