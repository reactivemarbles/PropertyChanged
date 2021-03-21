// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    internal class WhenChangedMockUserSourceBuilder
    {
        private Accessibility _classAccessModifier;
        private string _namespaceName;
        private string _className;
        private Accessibility _propertyAccessModifier;
        private string _valuePropertyTypeName;
        private CustomTypeInfoForValueProperty _customTypeInfoForValueProperty;
        private Accessibility? _outerClassAccessModifier;
        private List<InvocationExpressionSyntax> _whenAnyInvocations;

        public WhenChangedMockUserSourceBuilder(InvocationKind invocationKind, ReceiverKind receiverKind, ExpressionForm expressionForm, int depth)
        {
            _classAccessModifier = Accessibility.Public;
            _namespaceName = "Sample";
            _className = "SampleClass";
            _propertyAccessModifier = Accessibility.Public;
            _valuePropertyTypeName = "string";
            _whenAnyInvocations = new List<InvocationExpressionSyntax>();

            AndWhenChanged(invocationKind, receiverKind, expressionForm, depth);
        }

        public WhenChangedMockUserSourceBuilder ClassAccessModifier(Accessibility value)
        {
            _classAccessModifier = value;
            return this;
        }

        public WhenChangedMockUserSourceBuilder PropertyAccessModifier(Accessibility value)
        {
            _propertyAccessModifier = value;
            return this;
        }

        public WhenChangedMockUserSourceBuilder NamespaceName(string value)
        {
            _namespaceName = value;
            return this;
        }

        public WhenChangedMockUserSourceBuilder ClassName(string value)
        {
            _className = value;
            return this;
        }

        public WhenChangedMockUserSourceBuilder OuterClassAccessModifier(Accessibility value)
        {
            _outerClassAccessModifier = value;
            return this;
        }

        public WhenChangedMockUserSourceBuilder GetTypeName(out string typeName)
        {
            if (_outerClassAccessModifier == null)
            {
                typeName = string.IsNullOrEmpty(_namespaceName) ? _className : $"{_namespaceName}.{_className}";
            }
            else
            {
                typeName = string.IsNullOrEmpty(_namespaceName) ? $"OuterClass+{_className}" : $"{_namespaceName}.OuterClass+{_className}";
            }

            return this;
        }

        public WhenChangedMockUserSourceBuilder AddCustomTypeForValueProperty(string className, Accessibility accessModifier, out string typeName)
        {
            _customTypeInfoForValueProperty = new CustomTypeInfoForValueProperty(className, accessModifier, false);
            typeName = string.IsNullOrEmpty(_namespaceName) ? $"{className}" : $"{_namespaceName}.{className}";
            return this;
        }

        public WhenChangedMockUserSourceBuilder AddCustomNestedTypeForValueProperty(string className, Accessibility accessModifier, out string typeName)
        {
            _customTypeInfoForValueProperty = new CustomTypeInfoForValueProperty(className, accessModifier, true);
            typeName = string.IsNullOrEmpty(_namespaceName) ? $"{_className}+{className}" : $"{_namespaceName}.{_className}+{className}";
            return this;
        }

        public WhenChangedMockUserSourceBuilder AndWhenChanged(InvocationKind invocationKind, ReceiverKind receiverKind, ExpressionForm expressionForm, int depth)
        {
            ExpressionSyntax receiver = receiverKind == ReceiverKind.This ? ThisExpression() : IdentifierName("instance");

            ArgumentSyntax argument = expressionForm switch
            {
                ExpressionForm.Inline =>
                    RoslynHelpers.LambdaAccessArgument(
                        "x",
                        RoslynHelpers.MemberAccess("x", Enumerable.Range(1, depth - 1).Select(_ => "Child").Concat(new[] { "Value" }))),
                ExpressionForm.Property =>
                    RoslynHelpers.PropertyArgument("MyExpression"),
                ExpressionForm.Method =>
                    RoslynHelpers.MethodArgument("GetExpression"),
                ExpressionForm.BodyIncludesIndexer =>
                    depth < 2 ? RoslynHelpers.LambdaIndexerArgument("x", "Values", 0) : RoslynHelpers.LambdaIndexerArgument("x", "Children", 0, Enumerable.Range(1, depth - 1).Select(_ => "Child").Concat(new[] { "Value" })),
                ExpressionForm.BodyIncludesMethodInvocation =>
                    depth < 2 ? RoslynHelpers.LambdaInvokeMethodArgument("x", "GetValue") : RoslynHelpers.LambdaInvokeMethodArgument("x", "GetValue", Enumerable.Range(1, depth - 1).Select(_ => "Child").Concat(new[] { "Value" })),
                ExpressionForm.BodyExcludesLambdaParam =>
                     depth < 2 ? RoslynHelpers.LambdaNoVariableUseArgument("x", "Value") : RoslynHelpers.LambdaNoVariableUseArgument("x", Enumerable.Range(1, depth - 1).Select(_ => "Child").Concat(new[] { "Value" })),
                _ => throw new InvalidOperationException($"{nameof(ExpressionForm)} [{expressionForm}] is not supported."),
            };

            var invocation = invocationKind switch
            {
                InvocationKind.Explicit => RoslynHelpers.InvokeExplicitMethod("NotifyPropertyChangedExtensions", "WhenChanged", Argument(receiver), argument),
                InvocationKind.MemberAccess => RoslynHelpers.InvokeMethod("WhenChanged", receiver, argument),
                _ => throw new InvalidOperationException("Unknown type of invocation."),
            };

            _whenAnyInvocations.Add(invocation);

            return this;
        }

        public string Build()
        {
            var classes = new List<ClassDeclarationSyntax>();

            ClassDeclarationSyntax nestedCustomType = null;

            if (_customTypeInfoForValueProperty != null)
            {
                var (className, accessModifier, isNested) = _customTypeInfoForValueProperty;
                _valuePropertyTypeName = className;

                var customTypeClass = ClassDeclaration(className)
                    .WithModifiers(accessModifier.GetToken());

                if (isNested)
                {
                    _valuePropertyTypeName = _className + "." + _valuePropertyTypeName;
                    nestedCustomType = customTypeClass;
                }
                else
                {
                    classes.Add(customTypeClass);
                }

                if (!string.IsNullOrEmpty(_namespaceName))
                {
                    _valuePropertyTypeName = _namespaceName + "." + _valuePropertyTypeName;
                }

                if (accessModifier == Accessibility.Internal && _propertyAccessModifier == Accessibility.Public)
                {
                    _propertyAccessModifier = Accessibility.Internal;
                }
                else if (accessModifier == Accessibility.Protected && (_propertyAccessModifier == Accessibility.Public || _propertyAccessModifier == Accessibility.Internal))
                {
                    _propertyAccessModifier = Accessibility.Protected;
                }
                else if (accessModifier == Accessibility.Private && (_propertyAccessModifier == Accessibility.Public || _propertyAccessModifier == Accessibility.Internal || _propertyAccessModifier == Accessibility.Protected))
                {
                    _propertyAccessModifier = Accessibility.Private;
                }
            }

            var mainClass = GetClass(_className, _classAccessModifier, GetMembers().Concat(new[] { nestedCustomType }), true);

            if (_outerClassAccessModifier != null)
            {
                mainClass = GetClass("OuterClass", _outerClassAccessModifier.Value, new MemberDeclarationSyntax[] { mainClass }, false);
            }

            classes.Add(mainClass);

            var compilationUnit = GetCompilationUnitForClass(classes, _namespaceName);

            return compilationUnit.NormalizeWhitespace().ToFullString();
        }

        public string BuildMultiExpressionVersion()
        {
            return @"
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

public partial class SampleClass : INotifyPropertyChanged
{
    private MyClass _value;
    private string _stringValue;
    private SampleClass _child;

    public event PropertyChangedEventHandler PropertyChanged;

    protected MyClass Value
    {
        get => _value;
        set => RaiseAndSetIfChanged(ref _value, value);
    }

    public string StringValue
    {
        get => _stringValue;
        set => RaiseAndSetIfChanged(ref _stringValue, value);
    }

    public SampleClass Child
    {
        get => _child;
        set => RaiseAndSetIfChanged(ref _child, value);
    }

    public IObservable<MyClass> GetWhenChangedObservable()
    {
        return this.WhenChanged(x => x.Value, x => x.StringValue, (a, b) => a);
    }

    protected void RaiseAndSetIfChanged<T>(ref T fieldValue, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(fieldValue, value))
        {
            return;
        }

        fieldValue = value;
        OnPropertyChanged(propertyName);
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class MyClass
{
}
";
        }

        public string BuildMultiExpressionVersionNestedOutputType()
        {
            return @"
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

public partial class SampleClass : INotifyPropertyChanged
{
    private MyClass _value;
    private string _stringValue;
    private SampleClass _child;

    public event PropertyChangedEventHandler PropertyChanged;

    protected MyClass Value
    {
        get => _value;
        set => RaiseAndSetIfChanged(ref _value, value);
    }

    public string StringValue
    {
        get => _stringValue;
        set => RaiseAndSetIfChanged(ref _stringValue, value);
    }

    public SampleClass Child
    {
        get => _child;
        set => RaiseAndSetIfChanged(ref _child, value);
    }

    protected IObservable<MyClass> GetWhenChangedObservable()
    {
        return this.WhenChanged(x => x.Value, x => x.StringValue, (a, b) => a);
    }

    protected void RaiseAndSetIfChanged<T>(ref T fieldValue, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(fieldValue, value))
        {
            return;
        }

        fieldValue = value;
        OnPropertyChanged(propertyName);
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected class MyClass
    {
    }
}
";
        }

        private static ClassDeclarationSyntax GetClass(string className, Accessibility accessibility, IEnumerable<MemberDeclarationSyntax> members, bool isNotifyable)
        {
            var outputClass = ClassDeclaration(className)
                .WithModifiers(accessibility.GetToken().Add(Token(SyntaxKind.PartialKeyword)))
                .WithMembers(List(members));

            if (isNotifyable)
            {
                outputClass = outputClass.WithBaseList(
                    BaseList(
                    SingletonSeparatedList<BaseTypeSyntax>(
                        SimpleBaseType(
                            IdentifierName("INotifyPropertyChanged")))));
            }

            return outputClass;
        }

        private static CompilationUnitSyntax GetCompilationUnitForClass(IEnumerable<ClassDeclarationSyntax> classDeclarations, string namespaceName)
        {
            var compilationUnitMembers = string.IsNullOrWhiteSpace(namespaceName) ?
                classDeclarations.ToArray() :
                new MemberDeclarationSyntax[]
                {
                    NamespaceDeclaration(IdentifierName(namespaceName))
                        .WithMembers(List<MemberDeclarationSyntax>(classDeclarations))
                };

            return CompilationUnit()
                .WithUsings(List(
                    new[]
                    {
                        UsingDirective(IdentifierName("System")),
                        UsingDirective(IdentifierName("System.Collections.Generic")),
                        UsingDirective(IdentifierName("System.ComponentModel")),
                        UsingDirective(IdentifierName("System.Linq.Expressions")),
                        UsingDirective(IdentifierName("System.Runtime.CompilerServices"))
                    }))
                .WithMembers(List(compilationUnitMembers));
        }

        private IEnumerable<MemberDeclarationSyntax> GetMembers()
        {
            yield return RoslynHelpers.Field(_valuePropertyTypeName, "_value");
            yield return RoslynHelpers.Field(_className, "_child");
            yield return RoslynHelpers.Field(_valuePropertyTypeName + "[]", "_values");

            yield return RoslynHelpers.PropertyChanged();

            yield return RoslynHelpers.RaiseAndSetProperty(_valuePropertyTypeName, "Value", _propertyAccessModifier, "_value");
            yield return RoslynHelpers.RaiseAndSetProperty(_className, "Child", _propertyAccessModifier, "_child");
            yield return RoslynHelpers.RaiseAndSetProperty(_valuePropertyTypeName + "[]", "Values", _propertyAccessModifier, "_values");

            yield return GetWhenChangedObservables();
            yield return GetWhenChangedObservable();

            yield return RoslynHelpers.GetMethodToProperty(_valuePropertyTypeName, "Value", "GetValue", _propertyAccessModifier);

            yield return RoslynHelpers.GetMethodExpressionToProperty(_className, _valuePropertyTypeName, "Value", "GetExpression", _propertyAccessModifier);
            yield return RoslynHelpers.GetPropertyExpressionToProperty(_className, _valuePropertyTypeName, "MyExpression", "Value", _propertyAccessModifier);

            yield return RoslynHelpers.RaiseAndSetIfChanged();
            yield return RoslynHelpers.OnPropertyChanged();
        }

        private MethodDeclarationSyntax GetWhenChangedObservables() =>
            MethodDeclaration(
                ArrayType(
                    GenericName(Identifier("IObservable"))
                        .WithTypeArgumentList(
                            TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(_valuePropertyTypeName)))))
                    .WithRankSpecifiers(
                                SingletonList(
                                    ArrayRankSpecifier(
                                        SingletonSeparatedList<ExpressionSyntax>(
                                            OmittedArraySizeExpression())))),
                Identifier("GetWhenChangedObservables"))
                        .WithBody(
                            Block(
                                List(
                                    new StatementSyntax[]
                                    {
                                        LocalDeclarationStatement(
                                            VariableDeclaration(IdentifierName("var"))
                                                .WithVariables(
                                                    SingletonSeparatedList(
                                                        VariableDeclarator(Identifier("instance"))
                                                            .WithInitializer(EqualsValueClause(ThisExpression()))))),

                                        ReturnStatement(
                                            ImplicitArrayCreationExpression(
                                                InitializerExpression(
                                                    SyntaxKind.ArrayInitializerExpression,
                                                    SeparatedList<ExpressionSyntax>(_whenAnyInvocations)))),
                                    })))
                .WithModifiers(_propertyAccessModifier.GetToken());

        private MethodDeclarationSyntax GetWhenChangedObservable() =>
            MethodDeclaration(
                        GenericName(
                            Identifier("IObservable"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(_valuePropertyTypeName)))),
                        Identifier("GetWhenChangedObservable"))
                    .WithModifiers(_propertyAccessModifier.GetToken())
                    .WithExpressionBody(
                        ArrowExpressionClause(
                            ElementAccessExpression(
                                InvocationExpression(
                                    IdentifierName("GetWhenChangedObservables")))
                            .WithArgumentList(
                                BracketedArgumentList(
                                    SingletonSeparatedList(
                                        Argument(
                                            LiteralExpression(
                                                SyntaxKind.NumericLiteralExpression,
                                                Literal(0))))))))
                    .WithSemicolonToken(
                        Token(SyntaxKind.SemicolonToken));

        private class CustomTypeInfoForValueProperty
        {
            public CustomTypeInfoForValueProperty(string className, Accessibility accessModifier, bool isNested)
            {
                ClassName = className;
                AccessModifier = accessModifier;
                IsNested = isNested;
            }

            public string ClassName { get; }

            public Accessibility AccessModifier { get; }

            public bool IsNested { get; }

            internal void Deconstruct(out string className, out Accessibility accessModifier, out bool isNested)
            {
                className = ClassName;
                accessModifier = AccessModifier;
                isNested = IsNested;
            }
        }
    }
}
