// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    /// <summary>
    /// Source generator tets.
    /// </summary>
    public class WhenChangedGeneratorTests
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
        /// Tests the basic generation works.
        /// </summary>
        [Fact]
        public void TestBasicGenerationWorks()
        {
            string source = @"
using System;
using System.ComponentModel;

namespace Foo
{
    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public C()
        {
            this.WhenChanged(x => x.MyProperty).Subscribe(Console.WriteLine);
        }

        public string MyProperty { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }    
}";

            Compilation compilation = CreateCompilation(source);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());
            var diagnostics = newCompilation.GetDiagnostics();

            Assert.Empty(diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(generatorDiagnostics);
        }

        /// <summary>
        /// Tests the basic generation explicit works.
        /// </summary>
        [Fact]
        public void TestBasicGenerationExplicitWorks()
        {
            string source = @"
using System;
using System.ComponentModel;

namespace Foo
{
    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public C()
        {
            NotifyPropertyChangedExtensions.WhenChanged(this, x => x.MyProperty).Subscribe(Console.WriteLine);
        }

        public string MyProperty { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }    
}";

            Compilation compilation = CreateCompilation(source);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());
            var diagnostics = newCompilation.GetDiagnostics();

            Assert.Empty(diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(generatorDiagnostics);
        }

        /// <summary>
        /// Tests the basic generation explicit works.
        /// </summary>
        [Fact]
        public void TestBasicGenerationWithMultipleWorks()
        {
            string source = @"
using System;
using System.ComponentModel;

namespace Foo
{
    public class A : INotifyPropertyChanged
    {
        public A()
        {
            this.WhenChanged(x => x.MyString1, x => x.MyString2, (x, y) => x + ' ' + y).Subscribe(Console.WriteLine);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string MyString1 { get; set; }
        public string MyString2 { get; set; }
    }
}";

            Compilation compilation = CreateCompilation(source);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());
            var diagnostics = newCompilation.GetDiagnostics();
            string output = string.Join(Environment.NewLine, newCompilation.SyntaxTrees.Select(x => x.ToString()));

            Assert.Empty(diagnostics.Where(x => x.Severity > DiagnosticSeverity.Warning));
            Assert.Empty(generatorDiagnostics);
            Assert.False(string.IsNullOrWhiteSpace(output));
        }

        /// <summary>
        /// Tests the basic generation explicit works.
        /// </summary>
        [Fact]
        public void TestBasicGenerationWithMultipleNestedWorks()
        {
            string source = @"
using System;
using System.ComponentModel;

namespace Foo
{
    public class A : INotifyPropertyChanged
    {
        public A()
        {
            this.WhenChanged(x => x.BValue.MyString1, x => x.BValue.Child.Child.MyString2, (x, y) => x + ' ' + y).Subscribe(Console.WriteLine);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public B BValue { get; set; }
    }
    
    public class B : INotifyPropertyChanged
    {
        public B()
        {
            this.WhenChanged(x => x.MyString1, x => x.MyString2, (x, y) => x + ' ' + y).Subscribe(Console.WriteLine);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string MyString1 { get; set; }
        public string MyString2 { get; set; }

        public B Child { get; set; }
    }
}";

            Compilation compilation = CreateCompilation(source);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());
            var diagnostics = newCompilation.GetDiagnostics();

            string output = string.Join(Environment.NewLine, newCompilation.SyntaxTrees.Select(x => x.ToString()));

            Assert.Empty(diagnostics.Where(x => x.Severity > DiagnosticSeverity.Warning));
            Assert.Empty(generatorDiagnostics);
            Assert.False(string.IsNullOrWhiteSpace(output));
        }

        /// <summary>
        /// Tests that there are no diagnostics are reported when invoked via this chain.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [MemberData(nameof(Data))]
        public void NoDiagnosticsAreReported_When_InvokedViaThis_Chain(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Inline, depth: 3)
                .GetTypeName(out var typeName)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(newCompilation.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var assembly = GetAssembly(newCompilation);
            var type = assembly.GetType(typeName);
            var parent = CreateInstance(type);
            var observable = GetWhenChangedObservable<string>(parent);

            string testValue = "ignore";
            observable.Subscribe(x => testValue = x);
            Assert.Equal("ignore", testValue);

            // parent.Child = new type();
            // parent.Child.Child = new type();
            // parent.Child.Child.Value = "A";
            var child1Instance1 = CreateInstance(type);
            var child2Instance1 = CreateInstance(type);
            SetProperty(parent, "Child", child1Instance1);
            Assert.Equal("ignore", testValue);
            SetProperty(child1Instance1, "Child", child2Instance1);
            Assert.Null(testValue);
            SetProperty(child2Instance1, "Value", "A");
            Assert.Equal("A", testValue);

            // parent.Child = new type() { Child: new type() { Value: "B" } };
            var child1Instance2 = CreateInstance(type);
            var child2Instance2 = CreateInstance(type);
            SetProperty(child1Instance2, "Child", child2Instance2);
            SetProperty(child2Instance2, "Value", "B");
            SetProperty(parent, "Child", child1Instance2);
            Assert.Equal("B", testValue);

            // parent.Child.Child.Value = null;
            SetProperty(child2Instance2, "Value", null);
            Assert.Null(testValue);
        }

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
            var observable = GetWhenChangedObservable<string>(target);

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
            var observable = GetWhenChangedObservable<string>(target);

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
            var observable = GetWhenChangedObservable<string>(target);

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
            var observable = GetWhenChangedObservable(target);

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
            var observable = GetWhenChangedObservable<string>(target);

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
            var observable = GetWhenChangedObservable<string>(target);

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
            var observable = GetWhenChangedObservable(target);

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
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Inline, depth: 1)
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
            var observable = GetWhenChangedObservable(target);

            var valueInstance = CreateInstance(outputType);
            SetProperty(target, "Value", valueInstance);

            object testValue = null;
            observable.Subscribe(x => testValue = x);
            Assert.Equal(valueInstance, testValue);

            SetProperty(target, "Value", null);
            Assert.Null(testValue);
        }

        /// <summary>
        /// Tests that no diagnostics are reported when two classes exist with the same.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [MemberData(nameof(Data))]
        public void NoDiagnosticsAreReported_When_TwoClassesExistWithTheSameName(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource1 = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Inline, depth: 1)
                .NamespaceName("Sample1")
                .GetTypeName(out var typeName1)
                .Build();
            string userSource2 = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Inline, depth: 1)
                .NamespaceName("Sample2")
                .GetTypeName(out var typeName2)
                .Build();

            Compilation compilation = CreateCompilation(userSource1, userSource2);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(newCompilation.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var assembly = GetAssembly(newCompilation);

            // Sample1.SampleClass
            var type1 = assembly.GetType(typeName1);
            var target1 = CreateInstance(type1);
            var observable1 = GetWhenChangedObservable<string>(target1);

            string testValue1 = "ignore";
            observable1.Subscribe(x => testValue1 = x);
            Assert.Null(testValue1);

            SetProperty(target1, "Value", "test");
            Assert.Equal("test", testValue1);

            SetProperty(target1, "Value", null);
            Assert.Null(testValue1);

            // Sample2.SampleClass
            var type2 = assembly.GetType(typeName2);
            var target2 = CreateInstance(type2);
            var observable2 = GetWhenChangedObservable<string>(target2);

            string testValue2 = "ignore";
            observable2.Subscribe(x => testValue2 = x);
            Assert.Null(testValue2);

            SetProperty(target2, "Value", "test");
            Assert.Equal("test", testValue2);

            SetProperty(target2, "Value", null);
            Assert.Null(testValue2);
        }

        /// <summary>
        /// Tests that no diagnostics are reported when two invocations with a unique expression and same output type exist.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [MemberData(nameof(Data))]
        public void NoDiagnosticsAreReported_When_TwoInvocationsWithAUniqueExpressionAndSameOutputTypeExist(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Inline, depth: 1)
                .ClassAccessModifier("public")
                .AndWhenChanged(invocationKind, receiverKind, ExpressionForm.Inline, depth: 2)
                .GetTypeName(out var typeName)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(newCompilation.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var assembly = GetAssembly(newCompilation);
            var type = assembly.GetType(typeName);
            var target = CreateInstance(type);
            var observable = GetWhenChangedObservable<string>(target);

            string testValue = "ignore";
            observable.Subscribe(x => testValue = x);
            Assert.Null(testValue);

            SetProperty(target, "Value", "test");
            Assert.Equal("test", testValue);

            SetProperty(target, "Value", null);
            Assert.Null(testValue);
        }

        /// <summary>
        /// Tests that no diagnostics are reported when multiple output types exist with same name and multiple unique invocations involving each exist.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [MemberData(nameof(Data))]
        public void NoDiagnosticsAreReported_When_MultipleOutputTypesExistWithSameName_And_MultipleUniqueInvocationsInvolvingEachExist(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Inline, depth: 1)
                .BuildMultiExpressionVersion();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(newCompilation.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));
        }

        /// <summary>
        /// Tests that no diagnostics are reported when multi expression with nested protected output.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [MemberData(nameof(Data))]
        public void NoDiagnosticsAreReported_When_MultiExpressionWithNestedProtectedOutputType(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Inline, depth: 1)
                .BuildMultiExpressionVersionNestedOutputType();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(newCompilation.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var assembly = GetAssembly(newCompilation);
            var type = assembly.GetType("SampleClass");
            var target = CreateInstance(type);
            var observable = GetWhenChangedObservable(target);
            observable.Subscribe();
        }

        /// <summary>
        /// Test that diagnostics are reported when property is used as an expression argument.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [MemberData(nameof(Data))]
        public void DiagnosticIsReported_When_PropertyIsUsedAsAnExpressionArgument(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            // this.WhenChanged(MyExpression)
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Property, depth: 1)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());
            var diagnostics = newCompilation.GetDiagnostics();

            Assert.Empty(diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Single(generatorDiagnostics);
            Assert.Equal(DiagnosticWarnings.ExpressionMustBeInline, generatorDiagnostics[0].Descriptor);
        }

        /// <summary>
        /// Tests that diagnostics is reported when method invocation is used as an expression argument.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [MemberData(nameof(Data))]
        public void DiagnosticIsReported_When_MethodInvocationIsUsedAsAnExpressionArgument(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            // this.WhenChanged(GetExpression())
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.Method, depth: 1)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());
            var diagnostics = newCompilation.GetDiagnostics();

            Assert.Empty(diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Single(generatorDiagnostics);
            Assert.Equal(DiagnosticWarnings.ExpressionMustBeInline, generatorDiagnostics[0].Descriptor);
        }

        /// <summary>
        /// Tests that Diagnostics are reported when expression excludes lambda parameter.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [MemberData(nameof(Data))]
        public void DiagnosticIsReported_When_ExpressionExcludesLambdaParameter(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            // this.WhenChanged(GetExpression())
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.BodyExcludesLambdaParam, depth: 1)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());
            var diagnostics = newCompilation.GetDiagnostics();

            Assert.Empty(diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Single(generatorDiagnostics);
            Assert.Equal(DiagnosticWarnings.LambdaParameterMustBeUsed, generatorDiagnostics[0].Descriptor);
        }

        /// <summary>
        /// Tests that Diagnostics are reported when expression includes array access.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [MemberData(nameof(Data))]
        public void DiagnosticIsReported_When_ExpressionIncludesArrayAccess(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            // this.WhenChanged(GetExpression())
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.BodyIncludesIndexer, depth: 1)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());
            var diagnostics = newCompilation.GetDiagnostics();

            Assert.Empty(diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Single(generatorDiagnostics);
            Assert.Equal(DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed, generatorDiagnostics[0].Descriptor);
        }

        /// <summary>
        /// Tests that diagnostics are reported when expression includes method invocation.
        /// </summary>
        /// <param name="invocationKind">Kind of the invocation.</param>
        /// <param name="receiverKind">Kind of the receiver.</param>
        [Theory]
        [MemberData(nameof(Data))]
        public void DiagnosticIsReported_When_ExpressionIncludesMethodInvocation(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            // this.WhenChanged(GetExpression())
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, ExpressionForm.BodyIncludesMethodInvocation, depth: 1)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new Generator());
            var diagnostics = newCompilation.GetDiagnostics();

            Assert.Empty(diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Single(generatorDiagnostics);
            Assert.Equal(DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed, generatorDiagnostics[0].Descriptor);
        }

        private static object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type, bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, null, null);
        }

        private static IObservable<T> GetWhenChangedObservable<T>(object target)
        {
            return target.GetType().InvokeMember(
                "GetWhenChangedObservable",
                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                target,
                Array.Empty<object>()) as IObservable<T>;
        }

        private static IObservable<dynamic> GetWhenChangedObservable(object target)
        {
            return target.GetType().InvokeMember(
                "GetWhenChangedObservable",
                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                target,
                Array.Empty<object>()) as IObservable<dynamic>;
        }

        private static void SetProperty(object target, string propertyName, object value)
        {
            target.GetType().InvokeMember(
                propertyName,
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                target,
                new object[] { value });
        }

        private static Assembly GetAssembly(Compilation compilation)
        {
            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(ms.ToArray());
        }

        private static Compilation CreateCompilation(params string[] sources)
        {
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            return CSharpCompilation.Create(
                assemblyName: "compilation",
                syntaxTrees: sources.Select(x => CSharpSyntaxTree.ParseText(x, new CSharpParseOptions(LanguageVersion.Latest))),
                references: new[]
                {
                    MetadataReference.CreateFromFile(typeof(Observable).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(WhenChangedGenerator).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Console.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Linq.Expressions.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.ObjectModel.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
                },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithSpecificDiagnosticOptions(new[] { new KeyValuePair<string, ReportDiagnostic>("1061", ReportDiagnostic.Suppress) }));
        }

        private static GeneratorDriver CreateDriver(Compilation compilation, params ISourceGenerator[] generators) =>
            CSharpGeneratorDriver.Create(
                generators: ImmutableArray.Create(generators),
                additionalTexts: ImmutableArray<AdditionalText>.Empty,
                parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
                optionsProvider: null);

        private static Compilation RunGenerators(Compilation compilation, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
        {
            CreateDriver(compilation, generators).RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out diagnostics);
            return outputCompilation;
        }
    }
}
