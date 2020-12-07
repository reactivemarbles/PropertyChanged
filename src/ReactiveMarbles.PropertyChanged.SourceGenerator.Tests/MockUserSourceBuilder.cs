// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    internal class MockUserSourceBuilder
    {
        private InvocationKind _invocationKind;
        private ReceiverKind _receiverKind;
        private int _depth;
        private string _classAccessModifier;
        private string _namespaceName;
        private string _className;
        private string _propertyAccessModifier;
        private string _valuePropertyTypeName;
        private ExpressionArgumentForm _expressionArgumentForm;
        private string _invocation;

        public MockUserSourceBuilder(InvocationKind invocationKind, ReceiverKind receiverKind, int depth)
        {
            _invocationKind = invocationKind;
            _receiverKind = receiverKind;
            _depth = depth;
            _classAccessModifier = "public";
            _namespaceName = "Sample";
            _className = "SampleClass";
            _propertyAccessModifier = "public";
            _valuePropertyTypeName = "string";
            _expressionArgumentForm = ExpressionArgumentForm.Inline;
        }

        public int Depth { get; }

        public MockUserSourceBuilder ValuePropertyTypeName(string value)
        {
            _valuePropertyTypeName = value;
            return this;
        }

        public MockUserSourceBuilder ClassAccessModifier(string value)
        {
            _classAccessModifier = value;
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

        public MockUserSourceBuilder WithExpressionArgumentForm(ExpressionArgumentForm expressionArgumentForm)
        {
            _expressionArgumentForm = expressionArgumentForm;
            return this;
        }

        public MockUserSourceBuilder GetTypeName(out string typeName)
        {
            typeName = $"{_namespaceName}.{_className}";
            return this;
        }

        public string Build()
        {
            var receiver = _receiverKind == ReceiverKind.This ? "this" : "instance";
            var expression = _expressionArgumentForm switch
            {
                ExpressionArgumentForm.Inline => string.Join(".", Enumerable.Range(1, _depth - 1).Select(_ => "Child").Prepend("x => x").Append("Value")),
                ExpressionArgumentForm.Property => "MyExpression",
                ExpressionArgumentForm.Method => "GetExpression()",
                ExpressionArgumentForm.BodyIncludesArrayAccess => _depth < 2 ? "x => x.Values[0]" : string.Join(".", Enumerable.Range(1, _depth - 2).Select(_ => "Child").Prepend("x => x.Children[0]").Append("Value")),
                ExpressionArgumentForm.BodyIncludesMethodInvocation => _depth < 2 ? "x => x.GetValue()" : string.Join(".", Enumerable.Range(1, _depth - 2).Select(_ => "Child").Prepend("x => x.GetChild()").Append("Value")),
                ExpressionArgumentForm.BodyExcludesLambdaParam => _depth < 2 ? "x => Value" : "x => " + string.Join(".", Enumerable.Range(1, _depth - 2).Select(_ => "Child").Append("Value")),
                _ => throw new InvalidOperationException($"{nameof(ExpressionArgumentForm)} [{_expressionArgumentForm}] is not supported."),
            };

            if (_invocationKind == InvocationKind.MemberAccess)
            {
                _invocation = $"{receiver}.WhenChanged({expression})";
            }
            else
            {
                _invocation = $"NotifyPropertyChangedExtensions.WhenChanged({receiver}, {expression})";
            }

            return $@"
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace {_namespaceName}
{{
    {_classAccessModifier} class {_className} : INotifyPropertyChanged
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

        public {_valuePropertyTypeName}[] Values {{ get; }}

        public Expression<Func<{_className}, {_valuePropertyTypeName}>> MyExpression = x => x.Value;

        public Expression<Func<{_className}, {_valuePropertyTypeName}>> GetExpression() => x => x.Value;

        public {_valuePropertyTypeName} GetValue() => Value;

        public IObservable<{_valuePropertyTypeName}> GetWhenChangedObservable()
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
    }}
}}
";
        }
    }
}
