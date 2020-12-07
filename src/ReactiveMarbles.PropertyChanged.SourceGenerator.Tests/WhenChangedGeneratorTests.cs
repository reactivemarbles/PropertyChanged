// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
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
    public class WhenChangedGeneratorTests
    {
        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { InvocationKind.MemberAccess, ReceiverKind.This },
                new object[] { InvocationKind.MemberAccess, ReceiverKind.Instance },
                new object[] { InvocationKind.Explicit, ReceiverKind.This },
                new object[] { InvocationKind.Explicit, ReceiverKind.Instance },
            };

        [Theory]
        [MemberData(nameof(Data))]
        public void NoDiagnosticsAreReported_When_InvokedViaThis_Chain(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            // x => x.Child.Child.MyString
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, depth: 3)
                .ValuePropertyTypeName("string")
                .GetTypeName(out var typeName)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new WhenChangedGenerator());

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

        [Theory]
        [MemberData(nameof(Data))]
        public void NoDiagnosticsAreReported_When_ClassAccessModifierIsPublic(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, depth: 1)
                .ClassAccessModifier("public")
                .ValuePropertyTypeName("string")
                .GetTypeName(out var typeName)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new WhenChangedGenerator());

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

        [Theory]
        [MemberData(nameof(Data))]
        public void NoDiagnosticsAreReported_When_ClassAccessModifierIsInternal(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, depth: 1)
                .ClassAccessModifier("internal")
                .ValuePropertyTypeName("string")
                .GetTypeName(out var typeName)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new WhenChangedGenerator());

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

        [Theory]
        [MemberData(nameof(Data))]
        public void NoDiagnosticsAreReported_When_TwoClassesExistWithTheSameName(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            string userSource1 = new MockUserSourceBuilder(invocationKind, receiverKind, depth: 1)
                .NamespaceName("Sample1")
                .GetTypeName(out var typeName1)
                .Build();
            string userSource2 = new MockUserSourceBuilder(invocationKind, receiverKind, depth: 1)
                .NamespaceName("Sample2")
                .GetTypeName(out var typeName2)
                .Build();

            Compilation compilation = CreateCompilation(userSource1, userSource2);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new WhenChangedGenerator());

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

        [Theory]
        [MemberData(nameof(Data))]
        public void DiagnosticIsReported_When_PropertyIsUsedAsAnExpressionArgument(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            // this.WhenChanged(MyExpression)
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, depth: 1)
                .WithExpressionArgumentForm(ExpressionArgumentForm.Property)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new WhenChangedGenerator());
            var diagnostics = newCompilation.GetDiagnostics();

            Assert.Empty(diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Single(generatorDiagnostics);
            Assert.Equal(WhenChangedGenerator.ExpressionMustBeInline, generatorDiagnostics[0].Descriptor);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void DiagnosticIsReported_When_MethodInvocationIsUsedAsAnExpressionArgument(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            // this.WhenChanged(GetExpression())
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, depth: 1)
                .WithExpressionArgumentForm(ExpressionArgumentForm.Method)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new WhenChangedGenerator());
            var diagnostics = newCompilation.GetDiagnostics();

            Assert.Empty(diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Single(generatorDiagnostics);
            Assert.Equal(WhenChangedGenerator.ExpressionMustBeInline, generatorDiagnostics[0].Descriptor);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void DiagnosticIsReported_When_ExpressionLambdaParameterIsNotUsed(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            // this.WhenChanged(GetExpression())
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, depth: 1)
                .WithExpressionArgumentForm(ExpressionArgumentForm.BodyExcludesLambdaParam)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new WhenChangedGenerator());
            var diagnostics = newCompilation.GetDiagnostics();

            Assert.Empty(diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Single(generatorDiagnostics);
            Assert.Equal(WhenChangedGenerator.LambdaParameterMustBeUsed, generatorDiagnostics[0].Descriptor);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void DiagnosticIsReported_When_ExpressionIncludesArrayAccess(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            // this.WhenChanged(GetExpression())
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, depth: 1)
                .WithExpressionArgumentForm(ExpressionArgumentForm.BodyIncludesArrayAccess)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new WhenChangedGenerator());
            var diagnostics = newCompilation.GetDiagnostics();

            Assert.Empty(diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Single(generatorDiagnostics);
            Assert.Equal(WhenChangedGenerator.OnlyPropertyAndFieldAccessAllowed, generatorDiagnostics[0].Descriptor);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void DiagnosticIsReported_When_ExpressionIncludesMethodInvocation(InvocationKind invocationKind, ReceiverKind receiverKind)
        {
            // this.WhenChanged(GetExpression())
            string userSource = new MockUserSourceBuilder(invocationKind, receiverKind, depth: 1)
                .WithExpressionArgumentForm(ExpressionArgumentForm.BodyIncludesMethodInvocation)
                .Build();

            Compilation compilation = CreateCompilation(userSource);
            var newCompilation = RunGenerators(compilation, out var generatorDiagnostics, new WhenChangedGenerator());
            var diagnostics = newCompilation.GetDiagnostics();

            Assert.Empty(diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Single(generatorDiagnostics);
            Assert.Equal(WhenChangedGenerator.OnlyPropertyAndFieldAccessAllowed, generatorDiagnostics[0].Descriptor);
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
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Linq.Expressions.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.ObjectModel.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
                },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
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