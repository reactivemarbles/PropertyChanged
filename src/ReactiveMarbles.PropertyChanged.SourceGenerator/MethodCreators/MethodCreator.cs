// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;

using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators;

internal static partial class MethodCreator
{
    public static CompilationDatum Generate(SyntaxReceiver syntaxReceiver, CSharpCompilation compilation, in GeneratorExecutionContext context)
    {
        var compilationData = new CompilationDatum();
        var whenChangedData = new List<MultiWhenStatementsDatum>();

        GenerateBind(syntaxReceiver.BindOneWay, compilation, Constants.BindOneWayMethodName, whenChangedData, compilationData, context);
        GenerateBind(syntaxReceiver.BindTwoWay, compilation, Constants.BindTwoWayMethodName, whenChangedData, compilationData, context);

        GenerateWhenMetadata(syntaxReceiver.WhenChanged, compilation, Constants.WhenChangedMethodName, whenChangedData, context);
        GenerateWhenMetadata(syntaxReceiver.WhenChanging, compilation, Constants.WhenChangingMethodName, whenChangedData, context);

        GenerateWhenMethods(whenChangedData, compilationData);

        return compilationData;
    }

    private static List<AttributeListSyntax> GetMethodAttributes() =>
    [
        AttributeList(Attribute(Constants.ExcludeFromCodeCoverageAttributeTypeName)),
        AttributeList(Attribute(Constants.DebuggerNonUserCodeAttributeTypeName)),
        ////AttributeList(Attribute(Constants.PreserveAttributeTypeName, new[] { AttributeArgument(NameEquals(IdentifierName("AllMembers")), LiteralExpression(SyntaxKind.TrueLiteralExpression)) })),
        AttributeList(Attribute(Constants.ObfuscationAttributeTypeName, [AttributeArgument(NameEquals(IdentifierName("Exclude")), LiteralExpression(SyntaxKind.TrueLiteralExpression))])),
        AttributeList(Attribute(Constants.EditorBrowsableTypeName, [AttributeArgument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.EditorBrowsableStateTypeName, Constants.NeverEnumMemberName))])),
    ];

    private static IEnumerable<ParameterSyntax> CallerMembersParameters() =>
        [
            Parameter([AttributeList(Attribute(Constants.CallerMemberAttributeTypeName))], "string", Constants.CallerMemberParameterName, EqualsValueClause(NullLiteral())),
            Parameter([AttributeList(Attribute(Constants.CallerFilePathAttributeTypeName))], "string", Constants.CallerFilePathParameterName, EqualsValueClause(NullLiteral())),
            Parameter([AttributeList(Attribute(Constants.CallerLineNumberAttributeTypeName))], "int", Constants.CallerLineNumberParameterName, EqualsValueClause(LiteralExpression(0))),
        ];

    private static GenericNameSyntax GetExpressionFunc(string inputType, string returnType) =>
        GenericName(
            Constants.ExpressionTypeName,
            [GenericName(Constants.FuncTypeName, [IdentifierName(inputType), IdentifierName(returnType),]),]);
}
