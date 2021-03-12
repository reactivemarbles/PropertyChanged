// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis;

using Xunit;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    public class BindGeneratorTests : GeneratorTestBase
    {
        /// <summary>
        /// Gets the testing data.
        /// </summary>
        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { InvocationKind.MemberAccess, ReceiverKind.This },
                new object[] { InvocationKind.MemberAccess, ReceiverKind.Instance },
                new object[] { InvocationKind.Explicit, ReceiverKind.This },
                new object[] { InvocationKind.Explicit, ReceiverKind.Instance },
            };

        /// <summary>
        /// Tests that no diagnostics are reported when property access modifier is protected.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [InlineData(InvocationKind.MemberAccess, ReceiverKind.This)]
        [InlineData(InvocationKind.MemberAccess, ReceiverKind.Instance)]
        public void NoDiagnosticsAreReported_When_PropertyAccessModifierIsProtected(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Inline, depth: 1)
                .ClassAccessModifier("public")
                .PropertyAccessModifier("protected")
                .GetTypeName(out var typeName)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(newCompilation.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var assembly = GetAssembly(newCompilation);
            var type = assembly.GetType(typeName);
            var target = CreateInstance(type);
            var observable = GetBinding<string>(target);

            string testValue = "ignore";
            observable.Subscribe(x => testValue = x);
            Assert.Null(testValue);

            SetProperty(target, "Value", "test");
            Assert.Equal("test", testValue);

            SetProperty(target, "Value", null);
            Assert.Null(testValue);
        }

        /// <summary>
        /// Tests that no diagnostics are reported when property access modifier is private.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [InlineData(InvocationKind.MemberAccess, ReceiverKind.This)]
        [InlineData(InvocationKind.MemberAccess, ReceiverKind.Instance)]
        public void NoDiagnosticsAreReported_When_PropertyAccessModifierIsPrivate(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Inline, depth: 1)
                .ClassAccessModifier("public")
                .PropertyAccessModifier("private")
                .GetTypeName(out var typeName)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(newCompilation.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var assembly = GetAssembly(newCompilation);
            var type = assembly.GetType(typeName);
            var target = CreateInstance(type);
            var observable = GetBinding<string>(target);

            string testValue = "ignore";
            observable.Subscribe(x => testValue = x);
            Assert.Null(testValue);

            SetProperty(target, "Value", "test");
            Assert.Equal("test", testValue);

            SetProperty(target, "Value", null);
            Assert.Null(testValue);
        }

        /// <summary>
        /// Tests that no diagnostics are reported when class is nested and protected and outer class is internal.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [InlineData(InvocationKind.MemberAccess, ReceiverKind.This)]
        [InlineData(InvocationKind.MemberAccess, ReceiverKind.Instance)]
        public void NoDiagnosticsAreReported_When_ClassIsNestedAndProtectedAndOuterClassIsInternal(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Inline, depth: 1)
                .ClassAccessModifier("protected")
                .OuterClassAccessModifier("internal")
                .GetTypeName(out var typeName)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(newCompilation.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var assembly = GetAssembly(newCompilation);
            var type = assembly.GetType(typeName);
            var target = CreateInstance(type);
            var observable = GetBinding<string>(target);

            string testValue = "ignore";
            observable.Subscribe(x => testValue = x);
            Assert.Null(testValue);

            SetProperty(target, "Value", "test");
            Assert.Equal("test", testValue);

            SetProperty(target, "Value", null);
            Assert.Null(testValue);
        }

        /// <summary>
        /// Tests that no diagnostics are reported when class access modifier is public and no namespace and custom.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [InlineData(InvocationKind.MemberAccess, ReceiverKind.This)]
        [InlineData(InvocationKind.MemberAccess, ReceiverKind.Instance)]
        public void NoDiagnosticsAreReported_When_ClassAccessModifierIsPublicAndNoNamespaceAndCustomType(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Inline, depth: 1)
                .ClassAccessModifier("public")
                .NamespaceName(string.Empty) // TODO: Fix this temporal coupling. It breaks if this line is swapped with the next.
                .AddCustomTypeForValueProperty("CustomClass", "public", out var outputTypeName)
                .PropertyAccessModifier("protected")
                .GetTypeName(out var typeName)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(newCompilation.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var assembly = GetAssembly(newCompilation);
            var type = assembly.GetType(typeName);
            var outputType = assembly.GetType(outputTypeName);
            var target = CreateInstance(type);
            var observable = GetBinding(target);

            var valueInstance = CreateInstance(outputType);

            object testValue = null;
            observable.Subscribe(x => testValue = x);

            Assert.Null(testValue);

            SetProperty(target, "Value", valueInstance);

            Assert.Equal(valueInstance, testValue);

            SetProperty(target, "Value", null);
            Assert.Null(testValue);
        }

        /// <summary>
        /// Tests that no diagnostics are reported when class access modifier is public.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [MemberData(nameof(Data))]
        public void NoDiagnosticsAreReported_When_ClassAccessModifierIsPublic(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Inline, depth: 1)
                .ClassAccessModifier("public")
                .GetTypeName(out var typeName)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(newCompilation.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var assembly = GetAssembly(newCompilation);
            var type = assembly.GetType(typeName);
            var target = CreateInstance(type);
            var observable = GetBinding<string>(target);

            string testValue = "ignore";
            observable.Subscribe(x => testValue = x);
            Assert.Null(testValue);

            SetProperty(target, "Value", "test");
            Assert.Equal("test", testValue);

            SetProperty(target, "Value", null);
            Assert.Null(testValue);
        }

        /// <summary>
        /// Tests that no diagnostics are reported when class access modifier is internal.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [MemberData(nameof(Data))]
        public void NoDiagnosticsAreReported_When_ClassAccessModifierIsInternal(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Inline, depth: 1)
                .ClassAccessModifier("internal")
                .GetTypeName(out var typeName)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(newCompilation.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var assembly = GetAssembly(newCompilation);
            var type = assembly.GetType(typeName);
            var target = CreateInstance(type);
            var observable = GetBinding<string>(target);

            string testValue = "ignore";
            observable.Subscribe(x => testValue = x);
            Assert.Null(testValue);

            SetProperty(target, "Value", "test");
            Assert.Equal("test", testValue);

            SetProperty(target, "Value", null);
            Assert.Null(testValue);
        }

        /// <summary>
        /// Tests that no diagnostics are reported when output type is internal.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [MemberData(nameof(Data))]
        public void NoDiagnosticsAreReported_When_OutputTypeIsInternal(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Inline, depth: 1)
                .ClassAccessModifier("public")
                .AddCustomTypeForValueProperty("OutputTypeClass", "internal", out var outputTypeName)
                .GetTypeName(out var typeName)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(newCompilation.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var assembly = GetAssembly(newCompilation);
            var type = assembly.GetType(typeName);
            var outputType = assembly.GetType(outputTypeName);
            var target = CreateInstance(type);
            var observable = GetBinding(target);

            var valueInstance = CreateInstance(outputType);
            SetProperty(target, "Value", valueInstance);

            object testValue = null;
            observable.Subscribe(x => testValue = x);
            Assert.Equal(valueInstance, testValue);

            SetProperty(target, "Value", null);
            Assert.Null(testValue);
        }

        /// <summary>
        /// Tests that no diagnostics are reported when output type is nested and private.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [InlineData(InvocationKind.MemberAccess, ReceiverKind.This)]
        [InlineData(InvocationKind.MemberAccess, ReceiverKind.Instance)]
        public void NoDiagnosticsAreReported_When_OutputTypeIsNestedAndPrivate(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Inline, ExpressionForm.Inline, 1, 1)
                .ClassAccessModifier("public")
                .AddCustomNestedTypeForValueProperty("OutputTypeClass", "private", out var outputTypeName)
                .GetTypeName(out var typeName)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(newCompilation.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var assembly = GetAssembly(newCompilation);
            var type = assembly.GetType(typeName);
            var outputType = assembly.GetType(outputTypeName);
            var target = CreateInstance(type);
            var observable = GetBinding(target);

            var valueInstance = CreateInstance(outputType);
            SetProperty(target, "Value", valueInstance);

            object testValue = null;
            observable.Subscribe(x => testValue = x);
            Assert.Equal(valueInstance, testValue);

            SetProperty(target, "Value", null);
            Assert.Null(testValue);
        }

        private static IObservable<T> GetBinding<T>(object target)
        {
            return target.GetType().InvokeMember(
                "GetBinds",
                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                target,
                Array.Empty<object>()) as IObservable<T>;
        }

        private static IObservable<dynamic> GetBinding(object target)
        {
            return target.GetType().InvokeMember(
                "GetBinds",
                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                target,
                Array.Empty<object>()) as IObservable<dynamic>;
        }
    }
}
