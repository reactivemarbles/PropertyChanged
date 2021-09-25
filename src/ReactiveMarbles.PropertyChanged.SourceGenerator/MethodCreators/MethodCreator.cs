// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators
{
    internal partial class MethodCreator : IMethodCreator
    {
        public (HashSet<MethodDatum> Extensions, HashSet<MethodDatum> Partials) Generate(SyntaxReceiver syntaxReceiver, CSharpCompilation compilation, GeneratorExecutionContext context)
        {
            var extensions = new HashSet<MethodDatum>();
            var partials = new HashSet<MethodDatum>();

            HandleMethod(() => GenerateBind(syntaxReceiver.BindOneWay, compilation, Constants.BindOneWayMethodName, CreateOneWayBindStatements, CreateBindOneWayMethod, context), extensions, partials);
            HandleMethod(() => GenerateBind(syntaxReceiver.BindTwoWay, compilation, Constants.BindTwoWayMethodName, CreateTwoWayBindStatements, CreateBindTwoWayMethod, context), extensions, partials);
            ////hashSet.UnionWith(GenerateWhen(syntaxReceiver.WhenChanging, compilation, Constants.WhenChangingMethodName, CreateWhenChanging, context));
            ////hashSet.UnionWith(GenerateWhen(syntaxReceiver.WhenChanged, compilation, Constants.WhenChangedMethodName, CreateWhenChanged, context));

            return (extensions, partials);
        }

        private static void HandleMethod(Func<(HashSet<MethodDatum> Extensions, HashSet<MethodDatum> Partials)> generateMethod, HashSet<MethodDatum> extensions, HashSet<MethodDatum> partials)
        {
            var (extensionsReturns, partialsReturns) = generateMethod();
            extensions.UnionWith(extensionsReturns);
            partials.UnionWith(partialsReturns);
        }

        private static IEnumerable<ParameterSyntax> CallerMembersParameters() =>
            new[]
            {
                    Parameter(new[] { AttributeList(Attribute(Constants.CallerMemberAttributeTypeName)) }, "string", Constants.CallerMemberParameterName, EqualsValueClause(NullLiteral())),
                    Parameter(new[] { AttributeList(Attribute(Constants.CallerFilePathAttributeTypeName)) }, "string", Constants.CallerFilePathParameterName, EqualsValueClause(NullLiteral())),
                    Parameter(new[] { AttributeList(Attribute(Constants.CallerLineNumberAttributeTypeName)) }, "int", Constants.CallerLineNumberParameterName, EqualsValueClause(LiteralExpression(0))),
            };

        private static GenericNameSyntax GetExpressionFunc(string inputType, string returnType) =>
            GenericName(
                Constants.ExpressionTypeName,
                new[]
                {
                    GenericName(
                        Constants.FuncTypeName,
                        new[]
                        {
                            IdentifierName(inputType),
                            IdentifierName(returnType),
                        }),
                });
    }
}
