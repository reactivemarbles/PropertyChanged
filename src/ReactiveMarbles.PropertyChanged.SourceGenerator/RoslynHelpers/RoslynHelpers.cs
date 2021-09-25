// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators;

using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.RoslynHelpers
{
    internal static class RoslynHelpers
    {
        /// <summary>
        /// Generates:
        /// [source].WhenChanged(expressionName).
        /// </summary>
        /// <param name="methodName">The method name.</param>
        /// <param name="expressionName">The expression.</param>
        /// <param name="source">The source variable.</param>
        /// <returns>The invocation.</returns>
        public static InvocationExpressionSyntax InvokeWhenChanged(string methodName, string expressionName, string source) =>
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(source),
                    methodName),
                new[]
                {
                        Argument(expressionName),
                        Argument("callerMemberName"),
                        Argument("callerFilePath"),
                        Argument("callerLineNumber"),
                });

        public static IReadOnlyCollection<UsingDirectiveSyntax> GetReactiveExtensionUsingDirectives() =>
            new[]
            {
                UsingDirective("System"),
                UsingDirective("System.Collections.Generic"),
                UsingDirective("System.ComponentModel"),
                UsingDirective("System.Linq.Expressions"),
                UsingDirective("System.Reactive.Concurrency"),
                UsingDirective("System.Reactive.Disposables"),
                UsingDirective("System.Reactive.Linq"),
                UsingDirective("System.Runtime.CompilerServices"),
            };

        public static ArgumentSyntax MethodArgument(string methodName) => Argument(InvocationExpression(IdentifierName(methodName)));

        public static EventFieldDeclarationSyntax PropertyChanged() =>
            EventFieldDeclaration(new[] { SyntaxKind.PublicKeyword }, "PropertyChangedEventHandler", "PropertyChanged", 1);

        public static PropertyDeclarationSyntax RaiseAndSetProperty(string typeName, string propertyName, Accessibility accessibility, string fieldName) =>
            PropertyDeclaration(
                typeName,
                propertyName,
                accessibility.GetAccessibilityTokens(),
                new[]
                {
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, Array.Empty<AttributeListSyntax>(), Array.Empty<SyntaxKind>(), ArrowExpressionClause(IdentifierName(fieldName))),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, Array.Empty<AttributeListSyntax>(), Array.Empty<SyntaxKind>(), ArrowExpressionClause(InvocationExpression("RaiseAndSetIfChanged", new[] { Argument(fieldName, Token(SyntaxKind.RefKeyword)), Argument("value") }))),
                },
                1);

        public static MethodDeclarationSyntax OnPropertyChanged() =>
            MethodDeclaration(
                new[] { SyntaxKind.PropertyKeyword, SyntaxKind.VirtualKeyword },
                "void",
                "OnPropertyChanged",
                new[] { Parameter("string", "propertyName") },
                0,
                Block(
                    new StatementSyntax[]
                    {
                        ExpressionStatement(
                            ConditionalAccessExpression(
                                IdentifierName("PropertyChanged"),
                                InvocationExpression(
                                    MemberBindingExpression("Invoke"),
                                    new[]
                                    {
                                        Argument(Constants.ThisObjectVariable),
                                        Argument(
                                            ObjectCreationExpression("PropertyChangedEventArgs", new[] { Argument("propertyName") })),
                                    }))),
                    },
                    1));

        ////MethodDeclaration(
        public static MethodDeclarationSyntax RaiseAndSetIfChanged() =>
            MethodDeclaration(
                new[] { SyntaxKind.ProtectedKeyword },
                "void",
                "RaiseAndSetIfChanged",
                new[]
                {
                    Parameter("T", "fieldValue", new[] { SyntaxKind.RefKeyword }),
                    Parameter("T", "value"),
                    Parameter(new[] { AttributeList(Attribute("CallerMemberName")) }, "string", "propertyName", EqualsValueClause(NullLiteral())),
                },
                0,
                Block(
                    new StatementSyntax[]
                    {
                        IfStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        GenericName("EqualityComparer", new[] { IdentifierName("T") }),
                                        "Default"),
                                    "Equals"),
                                new[]
                                {
                                    Argument("fieldValue"),
                                    Argument("value"),
                                }),
                            Block(
                                new StatementSyntax[]
                                {
                                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, "fieldValue", "value")),
                                    ExpressionStatement(InvocationExpression("OnPropertyChanged", new[] { Argument("propertyName") })),
                                },
                                1)),
                    },
                    0));

        public static MethodDeclarationSyntax WhenChangedConversion(string methodName, string inputType, string outputType, List<string> returnTypes, bool isExtension, Accessibility accessibility, BlockSyntax body)
        {
            var modifiers = accessibility.GetAccessibilityTokens().ToList();

            var parameterList = new List<ParameterSyntax>();

            if (isExtension)
            {
                modifiers.Add(SyntaxKind.StaticKeyword);
                parameterList.Add(Parameter(inputType, Constants.SourceParameterName, new[] { SyntaxKind.ThisKeyword }));
            }

            for (var i = 0; i < returnTypes.Count; ++i)
            {
                var returnType = returnTypes[i];
                var propertyName = "propertyExpression" + (i + 1);

                var parameter = Parameter(
                    GetExpressionFunc(inputType, returnType),
                    propertyName);
                parameterList.Add(parameter);
            }

            var conversionTypes = returnTypes.Select(IdentifierName).Concat(new[] { IdentifierName(outputType) }).ToList();

            parameterList.Add(Parameter(GenericName("Func", conversionTypes), "conversionFunc"));

            parameterList.AddRange(CallerMembersParameters());

            return MethodDeclaration(modifiers, $"IObservable<{outputType}>", methodName, parameterList, 0, body);
        }

        public static MethodDeclarationSyntax BindTwoWay(string hostInputType, string hostOutputType, string targetInputType, string targetOutputType, bool isExtension, bool hasConverters, Accessibility accessibility, BlockSyntax body)
        {
            var modifiers = accessibility.GetAccessibilityTokens().ToList();

            var parameterList = new List<ParameterSyntax>();

            if (isExtension)
            {
                modifiers.Add(SyntaxKind.StaticKeyword);
                parameterList.Add(Parameter(hostInputType, Constants.FromObjectVariable, new[] { SyntaxKind.ThisKeyword }));
            }

            parameterList.Add(Parameter(targetInputType, Constants.TargetParameter));

            parameterList.Add(Parameter(GetExpressionFunc(hostInputType, hostOutputType), Constants.FromPropertyParameter));
            parameterList.Add(Parameter(GetExpressionFunc(targetInputType, targetOutputType), Constants.ToPropertyParameter));

            if (hasConverters)
            {
                parameterList.Add(Parameter(GenericName("Func", new[] { IdentifierName(hostInputType), IdentifierName(targetOutputType) }), Constants.HostToTargetConverterFuncParameter));
                parameterList.Add(Parameter(GenericName("Func", new[] { IdentifierName(targetInputType), IdentifierName(hostOutputType) }), Constants.TargetToHostConverterFuncParameter));
            }

            parameterList.Add(Parameter("IScheduler", "scheduler", EqualsValueClause(NullLiteral())));

            parameterList.AddRange(CallerMembersParameters());

            return MethodDeclaration(modifiers, Constants.SystemDisposableTypeName, "Bind", parameterList, 0, body);
        }

        public static MethodDeclarationSyntax BindOneWay(string hostInputType, string hostOutputType, string targetInputType, string targetOutputType, bool isExtension, bool hasConverters, Accessibility accessibility, BlockSyntax body)
        {
            var modifiers = accessibility.GetAccessibilityTokens().ToList();

            var parameterList = new List<ParameterSyntax>();

            if (isExtension)
            {
                modifiers.Add(SyntaxKind.StaticKeyword);
                parameterList.Add(Parameter(hostInputType, Constants.FromObjectVariable, new[] { SyntaxKind.ThisKeyword }));
            }

            parameterList.Add(Parameter(targetInputType, Constants.TargetParameter));

            parameterList.Add(Parameter(GetExpressionFunc(hostInputType, hostOutputType), Constants.FromPropertyParameter));
            parameterList.Add(Parameter(GetExpressionFunc(targetInputType, targetOutputType), Constants.ToPropertyParameter));

            if (hasConverters)
            {
                parameterList.Add(Parameter(GenericName("Func", new[] { IdentifierName(hostInputType), IdentifierName(targetOutputType) }), Constants.HostToTargetConverterFuncParameter));
            }

            parameterList.Add(Parameter(Constants.ISchedulerTypeName, "scheduler", EqualsValueClause(NullLiteral())));

            parameterList.AddRange(CallerMembersParameters());

            return MethodDeclaration(modifiers, Constants.SystemDisposableTypeName, "BindOneWay", parameterList, 0, body);
        }

        public static GenericNameSyntax GetExpressionFunc(string inputType, string returnType) =>
            GenericName(
                "global::System.Linq.Expressions.Expression",
                new[]
                {
                    GenericName(
                        "global::System.Func",
                        new[]
                        {
                            IdentifierName(inputType),
                            IdentifierName(returnType),
                        }),
                });

        public static MethodDeclarationSyntax WhenChanged(string methodName, string inputType, string outputType, bool isExtension, Accessibility accessibility, BlockSyntax body)
        {
            GetWhenChangedValues(inputType, outputType, isExtension, accessibility, out var modifiers, out var parameterList);

            return MethodDeclaration(modifiers, $"IObservable<{outputType}>", methodName, parameterList, 0, body);
        }

        public static MethodDeclarationSyntax WhenChanged(string methodName, string inputType, string outputType, bool isExtension, Accessibility accessibility, ArrowExpressionClauseSyntax body)
        {
            GetWhenChangedValues(inputType, outputType, isExtension, accessibility, out var modifiers, out var parameterList);

            return MethodDeclaration(modifiers, $"IObservable<{outputType}>", methodName, parameterList, 0, body);
        }

        public static AssignmentExpressionSyntax MapEntry(string keyName, InvocationExpressionSyntax observableExpression) =>
            AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    ImplicitElementAccess(new[]
                        {
                            Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(keyName))),
                        }),
                    SimpleLambdaExpression(Parameter(Constants.SourceParameterName), observableExpression));

        public static FieldDeclarationSyntax MapDictionary(string inputTypeName, string outputTypeName, string mapName, IReadOnlyCollection<ExpressionSyntax> initializerMembers)
        {
            var dictionaryType = GenericName("Dictionary", new TypeSyntax[] { IdentifierName("string"), GenericName("IObservable", new[] { IdentifierName(outputTypeName) }) });
            return FieldDeclaration(
                GenericName(
                    "Dictionary",
                    new TypeSyntax[]
                    {
                        IdentifierName("string"),
                        GenericName(
                            "IObservable",
                            new TypeSyntax[]
                            {
                                IdentifierName(outputTypeName),
                            }),
                    }),
                mapName,
                EqualsValueClause(ObjectCreationExpression(dictionaryType, Array.Empty<ArgumentSyntax>(), InitializerExpression(SyntaxKind.ObjectInitializerExpression, initializerMembers))),
                new[]
                {
                    SyntaxKind.PrivateKeyword,
                    SyntaxKind.StaticKeyword,
                    SyntaxKind.ReadOnlyKeyword,
                },
                1);
        }

        public static InvocationExpressionSyntax MapInvokeExpression(string invokeName, string mapName, string expressionParameterName) =>
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ElementAccessExpression(
                        mapName,
                        new[]
                        {
                            Argument(InvocationExpression(MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    expressionParameterName,
                                    "Body"),
                                "ToString"))),
                        }),
                    "Invoke"),
                new[]
                {
                    Argument(invokeName),
                });

        public static LocalDeclarationStatementSyntax InvokeWhenChangedVariable(string methodName, string type, string obsName, string expressionName, string source) =>
            LocalDeclarationStatement(VariableDeclaration($"IObservable<{type}>", new[] { VariableDeclarator(obsName, EqualsValueClause(InvokeWhenChanged(methodName, expressionName, source))) }));

        public static LocalDeclarationStatementSyntax InvokeWhenChangedSkipVariable(string methodName, string type, string obsName, string expressionName, string source, int skipNumber) =>
            LocalDeclarationStatement(VariableDeclaration($"IObservable<{type}>", new[] { VariableDeclarator(obsName, EqualsValueClause(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, InvokeWhenChanged(methodName, expressionName, source), Constants.SkipMethodName), new[] { Argument(LiteralExpression(skipNumber)) }))) }));

        public static InvocationExpressionSyntax SelectObservableNotifyPropertyChangedSwitch(InvocationExpressionSyntax sourceInvoke, string returnType, string inputName, string memberName, string eventName, string handlerName) =>
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            sourceInvoke,
                            Constants.SelectMethod),
                        new[]
                        {
                            Argument(SimpleLambdaExpression(Parameter(inputName), ObservableNotifyPropertyChanged(returnType, inputName, memberName, eventName, handlerName))),
                        }),
                    Constants.SwitchMethodName));

        public static InvocationExpressionSyntax? GetObservableChain(string inputName, IReadOnlyList<ExpressionChain> members, string eventName, string handlerName)
        {
            InvocationExpressionSyntax? observable = null;
            for (var i = 0; i < members.Count; ++i)
            {
                var (name, _, outputType) = members[i];

                observable = i == 0 || observable is null ?
                    ObservableNotifyPropertyChanged(outputType.ToDisplayString(), inputName, name, eventName, handlerName) :
                    SelectObservableNotifyPropertyChangedSwitch(observable, outputType.ToDisplayString(), Constants.SourceParameterName, name, eventName, handlerName);
            }

            return observable;
        }

        public static InvocationExpressionSyntax ObservableNotifyPropertyChanged(string returnType, string inputName, string memberName, string eventName, string handlerName) =>
            InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("Observable"), GenericName(Constants.CreateMethodName, new TypeSyntax[] { IdentifierName(returnType) })),
                new[]
                {
                    Argument(
                        SimpleLambdaExpression(
                            Parameter(Constants.ObserverParameterName),
                            Block(
                                new StatementSyntax[]
                                {
                                    IfStatement(
                                        BinaryExpression(SyntaxKind.EqualsExpression, IdentifierName(inputName), NullLiteral()),
                                        ReturnStatement(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "Disposable", Constants.EmptyPropertyName))),
                                    ExpressionStatement(InvocationExpression(
                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.ObserverParameterName, Constants.OnNextMethodName),
                                        new[] { Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, inputName, memberName)) })),
                                    LocalDeclarationStatement(
                                        VariableDeclaration(
                                            handlerName,
                                            new[]
                                            {
                                                VariableDeclarator(
                                                    "handler",
                                                    EqualsValueClause(ParenthesizedLambdaExpression(
                                                        new[]
                                                        {
                                                            Parameter(Constants.SenderParameterName),
                                                            Parameter("e"),
                                                        },
                                                        Block(
                                                            new StatementSyntax[]
                                                            {
                                                                IfStatement(
                                                                    BinaryExpression(
                                                                        SyntaxKind.EqualsExpression,
                                                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "e", "PropertyName"),
                                                                        LiteralExpression(memberName)),
                                                                    Block(
                                                                        new StatementSyntax[]
                                                                        {
                                                                            ExpressionStatement(InvocationExpression(
                                                                                MemberAccessExpression(
                                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                                    Constants.ObserverParameterName,
                                                                                    Constants.OnNextMethodName),
                                                                                new[]
                                                                                {
                                                                                    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, inputName, memberName)),
                                                                                })),
                                                                        },
                                                                        3)),
                                                            },
                                                            2)))),
                                            })),
                                    ExpressionStatement(AssignmentExpression(SyntaxKind.AddAssignmentExpression, MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, inputName, eventName), IdentifierName("handler"))),
                                    ReturnStatement(InvocationExpression(
                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "Disposable", Constants.CreateMethodName),
                                        new[]
                                        {
                                            Argument(TupleExpression(
                                                new[]
                                                {
                                                    Argument(inputName, "Parent"),
                                                    Argument("handler", Constants.HandlerMethodName),
                                                })),
                                            Argument(SimpleLambdaExpression(
                                                Parameter(Constants.LambdaSingleParameterName),
                                                AssignmentExpression(
                                                    SyntaxKind.SubtractAssignmentExpression,
                                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, inputName, eventName),
                                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.LambdaSingleParameterName, Constants.HandlerMethodName)))),
                                        })),
                                },
                                1))),
                });

        public static IEnumerable<ParameterSyntax> CallerMembersParameters() =>
            new[]
            {
                Parameter(new[] { AttributeList(Attribute("global::System.Runtime.CompilerServices.CallerMemberName")) }, "string", "callerMemberName", EqualsValueClause(NullLiteral())),
                Parameter(new[] { AttributeList(Attribute("global::System.Runtime.CompilerServices.CallerFilePath")) }, "string", "callerFilePath", EqualsValueClause(NullLiteral())),
                Parameter(new[] { AttributeList(Attribute("global::System.Runtime.CompilerServices.CallerLineNumber")) }, "int", "callerLineNumber", EqualsValueClause(LiteralExpression(0))),
            };

        public static MethodDeclarationSyntax GetMethodToProperty(string propertyType, string propertyName, string methodName, Accessibility accessibility) =>
            MethodDeclaration(accessibility.GetAccessibilityTokens(), propertyType, propertyName, 1, ArrowExpressionClause(IdentifierName(propertyName)));

        public static MethodDeclarationSyntax GetMethodExpressionToProperty(string className, string propertyType, string propertyName, string methodName, Accessibility accessibility) =>
            MethodDeclaration(accessibility.GetAccessibilityTokens(), propertyType, propertyName, new[] { TypeParameter(className), TypeParameter(propertyType) }, 1, ArrowExpressionClause(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.LambdaSingleParameterName, propertyName)));

        public static PropertyDeclarationSyntax GetPropertyExpressionToProperty(string inputType, string outputType, string propertyName, string valuePropertyName, Accessibility accessibility) =>
            PropertyDeclaration(GetExpressionFunc(inputType, outputType), propertyName, accessibility.GetAccessibilityTokens(), new[] { AccessorDeclaration(SyntaxKind.GetAccessorDeclaration) }, EqualsValueClause(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.LambdaSingleParameterName, valuePropertyName)), 0);

        public static SimpleLambdaExpressionSyntax LambdaIndexer(string variableName, string arrayName, int index) =>
            SimpleLambdaExpression(
                Parameter(variableName),
                ElementAccessExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(variableName),
                        IdentifierName(arrayName)))
                            .WithArgumentList(
                                BracketedArgumentList(SingletonSeparatedList(
                                        Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(index)))))));

        public static ArgumentSyntax LambdaIndexerArgument(string variableName, string arrayName, int index) =>
            Argument(LambdaIndexer(variableName, arrayName, index));

        public static ArgumentSyntax LambdaIndexerArgument(string variableName, string arrayName, int index, IEnumerable<string> members) =>
            Argument(MemberAccess(LambdaIndexer(variableName, arrayName, index), members));

        public static ArgumentSyntax LambdaInvokeMethodArgument(string variableName, string methodName) =>
            Argument(SimpleLambdaExpression(Parameter(variableName), InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(variableName), IdentifierName(methodName)))));

        public static ArgumentSyntax LambdaInvokeMethodArgument(string variableName, string methodName, IEnumerable<string> members) =>
            Argument(SimpleLambdaExpression(Parameter(variableName), MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, MemberAccess(variableName, members), IdentifierName(methodName))));

        public static ArgumentSyntax LambdaNoVariableUseArgument(string variableName, string propertyName) =>
            Argument(SimpleLambdaExpression(Parameter(variableName), IdentifierName(propertyName)));

        public static ArgumentSyntax LambdaNoVariableUseArgument(string variableName, IReadOnlyCollection<string> properties) =>
            Argument(SimpleLambdaExpression(
                Parameter(variableName),
                properties.Skip(1).Aggregate<string, ExpressionSyntax>(
                    IdentifierName(properties.First()),
                    (expression, name) =>
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, IdentifierName(name)))));

        private static void GetWhenChangedValues(string inputType, string outputType, bool isExtension, Accessibility accessibility, out List<SyntaxKind> modifiers, out List<ParameterSyntax> parameterList)
        {
            modifiers = accessibility.GetAccessibilityTokens().ToList();
            parameterList = new();
            if (isExtension)
            {
                modifiers.Add(SyntaxKind.StaticKeyword);
                parameterList.Add(Parameter(inputType, Constants.SourceParameterName, new[] { SyntaxKind.ThisKeyword }));
            }

            parameterList.Add(Parameter(
                    GetExpressionFunc(inputType, outputType),
                    "propertyExpression"));

            parameterList.AddRange(CallerMembersParameters());
        }
    }
}
