// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveMarbles.PropertyChanged.SourceGenerator.Comparers;
using ReactiveMarbles.PropertyChanged.SourceGenerator.Helpers;
using ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;
using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators;

internal static partial class MethodCreator
{
    private static void GenerateWhenMetadata(IReadOnlyList<InvocationExpressionSyntax> invocations, Compilation compilation, string methodName, ICollection<MultiWhenStatementsDatum> whenChangedMetadata, in GeneratorExecutionContext context)
    {
        foreach (var invocationExpression in invocations)
        {
            var model = compilation.GetSemanticModel(invocationExpression.SyntaxTree);
            var symbol = model.GetSymbolInfo(invocationExpression).Symbol;

            if (symbol is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            if (!methodSymbol.ContainingType.ToDisplayString().Equals(Constants.WhenExtensionClass))
            {
                continue;
            }

            var invocationArguments = invocationExpression.ArgumentList.Arguments
                .Where(
                    argument =>
                        model.GetTypeInfo(argument.Expression)
                            .ConvertedType?
                            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                            .StartsWith(Constants.ExpressionTypeName, StringComparison.InvariantCulture) == true);

            var isExtension = true;

            var classVisibility =
                isExtension ? Accessibility.Internal : methodSymbol.ContainingType.DeclaredAccessibility;

            var expressions = new List<WhenStatementsDatum>();

            var inputType = methodSymbol.TypeArguments[0];
            var outputType = methodSymbol.TypeArguments[methodSymbol.TypeArguments.Length - 1];

            foreach (var argument in invocationArguments)
            {
                if (argument.Expression is LambdaExpressionSyntax lambdaExpression)
                {
                    var isValid = GeneratorHelpers.GetExpression(
                        context,
                        lambdaExpression,
                        compilation,
                        model,
                        out var expressionArgument);

                    if (!isValid)
                    {
                        continue;
                    }

                    if (isExtension)
                    {
                        isExtension = !expressionArgument.ContainsPrivateOrProtectedMember;
                    }

                    // this.WhenChanged(...) and instance.WhenChanged(...) MethodKind = MethodKind.ReducedExtension
                    // NotifyPropertyChangedExtensions.WhenChanged(...) MethodKind = MethodKind.Ordinary
                    // An alternative way is checking if methodSymbol.ReceiverType.Name == NotifyPropertyChangedExtensions.
                    var isExplicitInvocation = methodSymbol.MethodKind == MethodKind.Ordinary;

                    if (isExplicitInvocation && expressionArgument.ContainsPrivateOrProtectedMember)
                    {
                        context.ReportDiagnostic(
                            DiagnosticWarnings.UnableToGenerateExtension,
                            invocationExpression.GetLocation());
                        break;
                    }

                    expressions.Add(new(expressionArgument));
                }
                else
                {
                    // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                    context.ReportDiagnostic(DiagnosticWarnings.ExpressionMustBeInline, argument.GetLocation());
                }
            }

            if (!isExtension)
            {
                classVisibility = inputType.DeclaredAccessibility;
            }

            whenChangedMetadata.Add(new(
                methodName,
                expressions,
                isExtension,
                classVisibility,
                inputType,
                outputType,
                methodSymbol.TypeArguments.ToArray()));
        }
    }

    private static void GenerateWhenMethods(List<MultiWhenStatementsDatum> methods, CompilationDatum compilationData)
    {
        var extensions = new Dictionary<MultiWhenStatementsDatum, SortedSet<IfStatementSyntax>>();
        var partials = new Dictionary<MultiWhenStatementsDatum, SortedSet<IfStatementSyntax>>();

        var i = 0;
        foreach (var multiMethodStatement in methods)
        {
            var isExtension = multiMethodStatement.IsExtensionMethod;

            BinaryExpressionSyntax? binaryStatement = null;
            InvocationExpressionSyntax? previousInvocation = null;
            foreach (var methodMetadata in multiMethodStatement.Arguments)
            {
                var expressionArgument = methodMetadata.Argument;

                var propertyExpressionName = multiMethodStatement.Arguments.Count > 1
                    ? Constants.PropertyExpressionParameterName + (i + 1)
                    : Constants.PropertyExpressionParameterName;
                var currentInvocation = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        Constants.StringTypeName,
                        Constants.EqualsMethod),
                    new[]
                    {
                        Argument(InvocationExpression(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            propertyExpressionName,
                            Constants.ToStringMethod))),
                        Argument(LiteralExpression(expressionArgument.LambdaBodyString))
                    });

                if (previousInvocation is not null)
                {
                    binaryStatement = BinaryExpression(
                        SyntaxKind.LogicalAndExpression,
                        previousInvocation,
                        currentInvocation);
                }

                previousInvocation = currentInvocation;
                i++;
            }

            var dictionary = isExtension ? extensions : partials;
            if (!dictionary.TryGetValue(multiMethodStatement, out var ifStatements))
            {
                ifStatements = new(SyntaxNodeComparer.Default);
                dictionary.Add(multiMethodStatement, ifStatements);
            }

            var statementExpressions = CreateWhenStatements(multiMethodStatement);

            if (previousInvocation is null)
            {
                continue;
            }

            ifStatements.Add(
                IfStatement(
                    binaryStatement ?? (ExpressionSyntax)previousInvocation,
                    Block(statementExpressions, isExtension ? 2 : 3)));
        }

        GenerateWhenMethods(extensions, true, compilationData);
        GenerateWhenMethods(partials, false, compilationData);
    }

    private static void GenerateWhenMethods(
        Dictionary<MultiWhenStatementsDatum, SortedSet<IfStatementSyntax>> input,
        bool isExtension,
        CompilationDatum compilationData)
    {
        foreach (var kvp in input)
        {
            var multiMethodData = kvp.Key;
            var hostInputType = multiMethodData.InputType;
            var classAccessibility = multiMethodData.ClassAccessibility;

            var methodAccessibility = multiMethodData.TypeArguments.GetMinVisibility();

            var statements = kvp.Value;

            var classDatum = compilationData.GetClass(
                hostInputType,
                classAccessibility,
                isExtension,
                Constants.WhenExtensionClass);

            var method = CreateWhenMethodDeclaration(
                kvp.Key,
                methodAccessibility,
                statements.Cast<StatementSyntax>().ToList());

            classDatum.MethodData.Add(new(methodAccessibility, method));
        }
    }

    private static MethodDeclarationSyntax CreateWhenMethodDeclaration(
        MultiWhenStatementsDatum methodDatum,
        Accessibility methodAccessibility,
        List<StatementSyntax> statements)
    {
        var modifiers = methodAccessibility.GetAccessibilityTokens().ToList();

        var methodName = methodDatum.MethodName;
        var isExtension = methodDatum.IsExtensionMethod;

        var inputType = methodDatum.InputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var outputType = methodDatum.OutputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var parameterList = new List<ParameterSyntax>();

        if (isExtension)
        {
            modifiers.Add(SyntaxKind.StaticKeyword);
            parameterList.Add(Parameter(inputType, Constants.FromObjectVariable, new[] { SyntaxKind.ThisKeyword }));
        }

        var i = 0;
        foreach (var expressionArgumentMetadata in methodDatum.Arguments)
        {
            var expressionArgument = expressionArgumentMetadata.Argument;
            var propertyExpressionName = methodDatum.Arguments.Count > 1
                ? Constants.PropertyExpressionParameterName + (i + 1)
                : Constants.PropertyExpressionParameterName;
            var parameterInputType =
                expressionArgument.InputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var parameterOutputType =
                expressionArgument.OutputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            parameterList.Add(Parameter(
                GetExpressionFunc(parameterInputType, parameterOutputType),
                propertyExpressionName));
            i++;
        }

        if (methodDatum.TypeArguments.Count > 2)
        {
            var conversionTypes = new List<TypeSyntax>(methodDatum.TypeArguments.Count);
            conversionTypes.AddRange(methodDatum.TypeArguments.Skip(1).Select(typeArgument =>
                    IdentifierName(typeArgument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))
                .Cast<TypeSyntax>());

            parameterList.Add(Parameter(
                GenericName(Constants.FuncTypeName, conversionTypes),
                Constants.ConverterParameterName));
        }

        parameterList.AddRange(CallerMembersParameters());

        statements.Add(ThrowStatement(
            ObjectCreationExpression(
                Constants.InvalidOperationExceptionTypeName,
                new[] { Argument("\"No valid expression found.\"") }),
            isExtension ? 2 : 3));

        var body = Block(statements, isExtension ? 1 : 2);

        var returnType = GenericName(Constants.IObservableTypeName, new[] { IdentifierName(outputType) });
        return MethodDeclaration(
            GetMethodAttributes(),
            modifiers,
            returnType,
            methodName,
            parameterList,
            isExtension ? 1 : 2,
            body);
    }

    private static List<StatementSyntax> CreateWhenStatements(MultiWhenStatementsDatum multiMethod)
    {
        var methodName = multiMethod.MethodName;
        var isExtension = multiMethod.IsExtensionMethod;

        var statements = new List<StatementSyntax>();

        var isWhenChanged = methodName.Equals(Constants.WhenChangedMethodName, StringComparison.InvariantCulture);

        var eventName = isWhenChanged ? Constants.WhenChangedEventName : Constants.WhenChangingEventName;
        var handlerName = isWhenChanged ? Constants.WhenChangedEventHandler : Constants.WhenChangingEventHandler;

        // generates: var hostObs = fromObject.WhenChanged(fromProperty);
        var i = 0;
        foreach (var expressionArgumentMetadata in multiMethod.Arguments)
        {
            var expressionArgument = expressionArgumentMetadata.Argument;
            var expressionChain = expressionArgument.ExpressionChain;
            var fromName = isExtension ? Constants.FromObjectVariable : Constants.ThisObjectVariable;
            var observableChain =
                GetObservableChain(fromName, expressionChain, eventName, handlerName, isExtension ? 2 : 3);
            var outputTypeName =
                expressionArgument.OutputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var variableName = multiMethod.Arguments.Count > 1
                ? Constants.HostObservableVariable + (i + 1)
                : Constants.HostObservableVariable;
            var observable = LocalDeclarationStatement(VariableDeclaration(
                GenericName(Constants.IObservableTypeName, new[] { IdentifierName(outputTypeName) }),
                new[] { VariableDeclarator(variableName, EqualsValueClause(observableChain)) }));
            statements.Add(observable);
            i++;
        }

        if (multiMethod.Arguments.Count > 1)
        {
            var arguments = new List<ArgumentSyntax>(multiMethod.Arguments.Count + 1);

            for (i = 0; i < multiMethod.Arguments.Count; ++i)
            {
                var observableName = multiMethod.Arguments.Count > 1
                    ? Constants.HostObservableVariable + (i + 1)
                    : Constants.HostObservableVariable;

                arguments.Add(Argument(observableName));
            }

            arguments.Add(Argument(Constants.ConverterParameterName));

            statements.Add(ReturnStatement(InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    Constants.ObservableLinqTypeName,
                    Constants.CombineLatestMethodName),
                arguments)));
        }
        else
        {
            statements.Add(ReturnStatement(Constants.HostObservableVariable));
        }

        return statements;
    }

    private static InvocationExpressionSyntax GetObservableChain(
        string inputName,
        IReadOnlyList<ExpressionChain> members,
        string eventName,
        string handlerName,
        int level)
    {
        InvocationExpressionSyntax? observable = null;
        for (var i = 0; i < members.Count; ++i)
        {
            var (name, _, outputType) = members[i];

            observable = i == 0 || observable is null
                ? ObservableNotifyPropertyChanged(
                    outputType.ToDisplayString(),
                    inputName,
                    name,
                    eventName,
                    handlerName,
                    level)
                : SelectObservableNotifyPropertyChangedSwitch(
                    observable,
                    outputType.ToDisplayString(),
                    Constants.SourceParameterName,
                    name,
                    eventName,
                    handlerName,
                    level);
        }

        return observable!;
    }

    private static InvocationExpressionSyntax ObservableNotifyPropertyChanged(
        string returnType,
        string inputName,
        string memberName,
        string eventName,
        string handlerName,
        int level) =>
        InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(Constants.ObservableLinqTypeName),
                GenericName(Constants.CreateMethodName, new TypeSyntax[] { IdentifierName(returnType) })),
            new[]
            {
                Argument(
                    SimpleLambdaExpression(
                        Parameter(Constants.ObserverParameterName),
                        Block(
                            new StatementSyntax[]
                            {
                                IfStatement(
                                    BinaryExpression(
                                        SyntaxKind.EqualsExpression,
                                        IdentifierName(inputName),
                                        NullLiteral()),
                                    Block(
                                        new[]
                                        {
                                            ReturnStatement(MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                Constants.ReactiveDisposableTypeName,
                                                Constants.EmptyPropertyName))
                                        },
                                        level + 2)),
                                ExpressionStatement(InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        Constants.ObserverParameterName,
                                        Constants.OnNextMethodName),
                                    new[]
                                    {
                                        Argument(MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            inputName,
                                            memberName))
                                    })),
                                LocalDeclarationStatement(
                                    VariableDeclaration(
                                        handlerName,
                                        new[]
                                        {
                                            VariableDeclarator(
                                                Constants.HandlerParameterName,
                                                EqualsValueClause(ParenthesizedLambdaExpression(
                                                    new[]
                                                    {
                                                        Parameter(Constants.SenderParameterName),
                                                        Parameter(Constants.EventArgumentsParameterName),
                                                    },
                                                    Block(
                                                        new StatementSyntax[]
                                                        {
                                                            IfStatement(
                                                                BinaryExpression(
                                                                    SyntaxKind.EqualsExpression,
                                                                    MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        Constants.EventArgumentsParameterName,
                                                                        Constants.PropertyNamePropertyName),
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
                                                                                Argument(MemberAccessExpression(
                                                                                    SyntaxKind
                                                                                        .SimpleMemberAccessExpression,
                                                                                    inputName,
                                                                                    memberName)),
                                                                            })),
                                                                    },
                                                                    level + 4)),
                                                        },
                                                        level + 3)))),
                                        })),
                                ExpressionStatement(AssignmentExpression(
                                    SyntaxKind.AddAssignmentExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        inputName,
                                        eventName),
                                    IdentifierName("handler"))),
                                ReturnStatement(InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        Constants.ReactiveDisposableTypeName,
                                        Constants.CreateMethodName),
                                    new[]
                                    {
                                        Argument(TupleExpression(
                                            new[]
                                            {
                                                Argument(inputName, Constants.ParentPropertyName),
                                                Argument(
                                                    Constants.HandlerParameterName,
                                                    Constants.HandlerMethodName),
                                            })),
                                        Argument(SimpleLambdaExpression(
                                            Parameter(Constants.LambdaSingleParameterName),
                                            AssignmentExpression(
                                                SyntaxKind.SubtractAssignmentExpression,
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    inputName,
                                                    eventName),
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    Constants.LambdaSingleParameterName,
                                                    Constants.HandlerMethodName)))),
                                    })),
                            },
                            level + 1))),
            });

    private static InvocationExpressionSyntax SelectObservableNotifyPropertyChangedSwitch(
        ExpressionSyntax sourceInvoke,
        string returnType,
        string inputName,
        string memberName,
        string eventName,
        string handlerName,
        int level) =>
        InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                Constants.ObservableLinqTypeName,
                Constants.SwitchMethodName),
            new[]
            {
                Argument(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            Constants.ObservableLinqTypeName,
                            Constants.SelectMethod),
                        new[]
                        {
                            Argument(sourceInvoke),
                            Argument(SimpleLambdaExpression(
                                Parameter(inputName),
                                ObservableNotifyPropertyChanged(
                                    returnType,
                                    inputName,
                                    memberName,
                                    eventName,
                                    handlerName,
                                    level))),
                        }))
            });
}
