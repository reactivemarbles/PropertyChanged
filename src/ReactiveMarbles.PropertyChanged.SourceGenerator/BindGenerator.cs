// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal static class BindGenerator
    {
        private const string ExtensionClassFullName = "BindExtensions";

        internal static void GenerateWhenChanged(GeneratorExecutionContext context, Compilation compilation, SyntaxReceiver syntaxReceiver)
        {
            var bindInfo = ExtractBindInvocationInfo(context, compilation, syntaxReceiver);

            if (!bindInfo.AllExpressionArgumentsAreValid)
            {
                return;
            }


        }

        private static List<T> CreateDatum<T>(SortedList<ITypeSymbol, HashSet<(ExpressionArgument ViewModel, ExpressionArgument View)>> argumentGroupings, Func<InputTypeGroup, List<MethodDatum>, T> createFunc)
        {
            var datumData = new List<T>();

            foreach (var grouping in argumentGroupings)
            {
                var arguments = new SortedList<ITypeSymbol, OutputTypeGroup>();

                foreach (var argument in grouping.Value)
                {
                    arguments.InsertOutputGroup(argument., argument);
                }

                if (arguments.Count == 0)
                {
                    continue;
                }

                var inputGroup = arguments.Select(x => x.Value).ToInputTypeGroup(grouping.Key);

                var allMethodData = inputGroup.OutputTypeGroups.Select(CreateSingleExpressionMethodDatum).Concat(multiMethods).ToList();

                var datum = createFunc(inputGroup, allMethodData);

                datumData.Add(datum);
            }

            return datumData;
        }

        private static BindInvocationInfo ExtractBindInvocationInfo(GeneratorExecutionContext context, Compilation compilation, SyntaxReceiver syntaxReceiver)
        {
            var allExpressionArgumentsAreValid = true;
            var expressionArguments = new SortedList<ITypeSymbol, HashSet<(ExpressionArgument ViewModel, ExpressionArgument View)>>(TypeSymbolComparer.Default);

            foreach (var invocationExpression in syntaxReceiver.WhenChangedMethods)
            {
                var model = compilation.GetSemanticModel(invocationExpression.SyntaxTree);
                var symbol = model.GetSymbolInfo(invocationExpression).Symbol;

                if (symbol is not IMethodSymbol methodSymbol)
                {
                    continue;
                }

                if (!methodSymbol.ContainingType.ToDisplayString().Equals(ExtensionClassFullName))
                {
                    continue;
                }

                var expressions = invocationExpression.ArgumentList.Arguments.Where(x => model.GetTypeInfo(x).ConvertedType.Name.Equals("Expression")).Select(x => (Expression: x.Expression as LambdaExpressionSyntax, Argument: x)).ToList();

                if (expressions.Count != 2)
                {
                    context.ReportDiagnostic(DiagnosticWarnings.BindingIncorrectNumberParameters, invocationExpression.GetLocation());
                    continue;
                }

                var viewModelExpression = expressions[0].Expression;

                if (viewModelExpression is null)
                {
                    // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                    context.ReportDiagnostic(DiagnosticWarnings.ExpressionMustBeInline, expressions[0].Argument.GetLocation());
                    continue;
                }

                var viewExpression = expressions[1].Expression;

                if (viewExpression is null)
                {
                    // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                    context.ReportDiagnostic(DiagnosticWarnings.ExpressionMustBeInline, expressions[1].Argument.GetLocation());
                    continue;
                }

                allExpressionArgumentsAreValid &= GeneratorHelpers.GetExpression(context, methodSymbol, viewModelExpression, compilation, model, out var viewModelExpressionArgument);
                allExpressionArgumentsAreValid &= GeneratorHelpers.GetExpression(context, methodSymbol, viewExpression, compilation, model, out var viewExpressionArgument);

                expressionArguments.ListInsert(viewExpressionArgument.InputType, (viewModelExpressionArgument, viewExpressionArgument));
            }

            return new BindInvocationInfo(allExpressionArgumentsAreValid, expressionArguments);
        }
    }
}
