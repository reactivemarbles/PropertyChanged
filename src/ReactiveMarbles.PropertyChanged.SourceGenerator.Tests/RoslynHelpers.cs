// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    internal static class RoslynHelpers
    {
        public static MemberAccessExpressionSyntax MemberAccess(string startVariable, params string[] identifierNames) =>
            (MemberAccessExpressionSyntax)identifierNames.Aggregate<string, ExpressionSyntax>(IdentifierName(startVariable), (expression, name) =>
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, IdentifierName(name)));

        public static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax startVariable, params string[] identifierNames) =>
            (MemberAccessExpressionSyntax)identifierNames.Aggregate(startVariable, (expression, name) =>
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, IdentifierName(name)));

        public static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax startVariable, IEnumerable<string> identifierNames) =>
            MemberAccess(startVariable, identifierNames.ToArray());

        public static MemberAccessExpressionSyntax MemberAccess(string startVariable, IEnumerable<string> identifierNames) =>
            MemberAccess(startVariable, identifierNames.ToArray());

        public static ArgumentSyntax LambdaAccessArgument(string variableName, MemberAccessExpressionSyntax members) =>
            Argument(SimpleLambdaExpression(Parameter(Identifier(variableName)))
                .WithExpressionBody(members));

        public static InvocationExpressionSyntax InvokeExplicitMethod(string className, string methodName, params ArgumentSyntax[] arguments) =>
            InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(className), IdentifierName(methodName)))
                .WithArgumentList(ArgumentList(SeparatedList(arguments)));

        public static InvocationExpressionSyntax InvokeMethod(string methodName, ExpressionSyntax instance, params ArgumentSyntax[] arguments) =>
            InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, instance, IdentifierName(methodName)))
                .WithArgumentList(ArgumentList(SeparatedList(arguments)));

        public static ArgumentSyntax PropertyArgument(string argument) => Argument(IdentifierName(argument));

        public static ArgumentSyntax MethodArgument(string methodName) => Argument(InvocationExpression(IdentifierName(methodName)));

        public static FieldDeclarationSyntax Field(string fieldType, string fieldName)
        {
            return FieldDeclaration(
                VariableDeclaration(IdentifierName(fieldType))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                Identifier(fieldName)))))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PrivateKeyword)));
        }

        public static EventFieldDeclarationSyntax PropertyChanged() =>
            EventFieldDeclaration(
                        VariableDeclaration(
                            IdentifierName("PropertyChangedEventHandler"))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier("PropertyChanged")))))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword)));

        public static PropertyDeclarationSyntax RaiseAndSetProperty(string typeName, string propertyName, Accessibility accessibility, string fieldName) =>
            PropertyDeclaration(
                    IdentifierName(typeName),
                    Identifier(propertyName))
                .WithModifiers(accessibility.GetToken())
                .WithAccessorList(
                    AccessorList(
                        List(
                            new AccessorDeclarationSyntax[]
                            {
                                            AccessorDeclaration(
                                                SyntaxKind.GetAccessorDeclaration)
                                            .WithExpressionBody(
                                                ArrowExpressionClause(
                                                    IdentifierName(fieldName)))
                                            .WithSemicolonToken(
                                                Token(SyntaxKind.SemicolonToken)),
                                            AccessorDeclaration(
                                                SyntaxKind.SetAccessorDeclaration)
                                            .WithExpressionBody(
                                                ArrowExpressionClause(
                                                    InvocationExpression(
                                                        IdentifierName("RaiseAndSetIfChanged"))
                                                    .WithArgumentList(
                                                        ArgumentList(
                                                            SeparatedList<ArgumentSyntax>(
                                                                new SyntaxNodeOrToken[]
                                                                {
                                                                    Argument(
                                                                        IdentifierName(fieldName))
                                                                    .WithRefOrOutKeyword(
                                                                        Token(SyntaxKind.RefKeyword)),
                                                                    Token(SyntaxKind.CommaToken),
                                                                    Argument(
                                                                        IdentifierName("value"))
                                                                })))))
                                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                            })));

        public static MethodDeclarationSyntax OnPropertyChanged() =>
            MethodDeclaration(
                        PredefinedType(
                            Token(SyntaxKind.VoidKeyword)),
                        Identifier("OnPropertyChanged"))
                    .WithModifiers(
                        TokenList(
                            new[]
                            {
                                Token(SyntaxKind.ProtectedKeyword),
                                Token(SyntaxKind.VirtualKeyword)
                            }))
                    .WithParameterList(
                        ParameterList(
                            SingletonSeparatedList(
                                Parameter(
                                    Identifier("propertyName"))
                                .WithType(
                                    PredefinedType(
                                        Token(SyntaxKind.StringKeyword))))))
                    .WithBody(
                        Block(
                            SingletonList<StatementSyntax>(
                                ExpressionStatement(
                                    ConditionalAccessExpression(
                                        IdentifierName("PropertyChanged"),
                                        InvocationExpression(
                                            MemberBindingExpression(
                                                IdentifierName("Invoke")))
                                        .WithArgumentList(
                                            ArgumentList(
                                                SeparatedList<ArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                    {
                                                        Argument(
                                                            ThisExpression()),
                                                        Token(SyntaxKind.CommaToken),
                                                        Argument(
                                                            ObjectCreationExpression(
                                                                IdentifierName("PropertyChangedEventArgs"))
                                                            .WithArgumentList(
                                                                ArgumentList(
                                                                    SingletonSeparatedList(
                                                                        Argument(
                                                                            IdentifierName("propertyName"))))))
                                                    }))))))));

        public static MethodDeclarationSyntax RaiseAndSetIfChanged() =>
            MethodDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.VoidKeyword)),
                    Identifier("RaiseAndSetIfChanged"))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.ProtectedKeyword)))
                .WithTypeParameterList(
                    TypeParameterList(
                        SingletonSeparatedList(
                            TypeParameter(
                                Identifier("T")))))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Parameter(Identifier("fieldValue"))
                                    .WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)))
                                    .WithType(IdentifierName("T")),
                                Token(SyntaxKind.CommaToken),
                                Parameter(Identifier("value"))
                                    .WithType(
                                        IdentifierName("T")),
                                Token(SyntaxKind.CommaToken),
                                Parameter(Identifier("propertyName"))
                                    .WithAttributeLists(
                                        SingletonList(
                                            AttributeList(
                                                SingletonSeparatedList(
                                                    Attribute(
                                                        IdentifierName("CallerMemberName"))))))
                                .WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
                                    .WithDefault(
                                        EqualsValueClause(
                                            LiteralExpression(
                                                SyntaxKind.NullLiteralExpression)))
                            })))
                .WithBody(
                    Block(
                        IfStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        GenericName(Identifier("EqualityComparer"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                    SingletonSeparatedList<TypeSyntax>(
                                                        IdentifierName("T")))),
                                        IdentifierName("Default")),
                                    IdentifierName("Equals")))
                            .WithArgumentList(
                                ArgumentList(
                                    SeparatedList<ArgumentSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            Argument(
                                                IdentifierName("fieldValue")),
                                            Token(SyntaxKind.CommaToken),
                                            Argument(
                                                IdentifierName("value"))
                                        }))),
                            Block(
                                SingletonList<StatementSyntax>(
                                    ReturnStatement()))),
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName("fieldValue"),
                                IdentifierName("value"))),
                        ExpressionStatement(
                            InvocationExpression(
                                IdentifierName("OnPropertyChanged"))
                            .WithArgumentList(
                                ArgumentList(
                                    SingletonSeparatedList(
                                        Argument(
                                            IdentifierName("propertyName"))))))));

        public static MethodDeclarationSyntax GetMethodToProperty(string propertyType, string propertyName, string methodName, Accessibility accessibility) =>
            MethodDeclaration(
                    IdentifierName(propertyType),
                    Identifier(methodName))
                .WithModifiers(accessibility.GetToken())
                .WithExpressionBody(
                    ArrowExpressionClause(
                        IdentifierName(propertyName)))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        public static MethodDeclarationSyntax GetMethodExpressionToProperty(string className, string propertyType, string propertyName, string methodName, Accessibility accessibility) =>
            MethodDeclaration(
                        GenericName(
                            Identifier("Expression"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    GenericName(
                                        Identifier("Func"))
                                    .WithTypeArgumentList(
                                        TypeArgumentList(
                                            SeparatedList<TypeSyntax>(
                                                new SyntaxNodeOrToken[]
                                                {
                                                    IdentifierName(className),
                                                    Token(SyntaxKind.CommaToken),
                                                    IdentifierName(propertyType)
                                                })))))),
                        Identifier(methodName))
                    .WithModifiers(accessibility.GetToken())
                    .WithExpressionBody(
                        ArrowExpressionClause(
                            SimpleLambdaExpression(
                                Parameter(
                                    Identifier("x")))
                            .WithExpressionBody(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("x"),
                                    IdentifierName(propertyName)))))
                    .WithSemicolonToken(
                        Token(SyntaxKind.SemicolonToken));

        public static PropertyDeclarationSyntax GetPropertyExpressionToProperty(string inputType, string outputType, string propertyName, string valuePropertyName, Accessibility accessibility) =>
            PropertyDeclaration(
                GenericName(Identifier("Expression"))
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList<TypeSyntax>(
                                GenericName(
                                    Identifier("Func"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SeparatedList<TypeSyntax>(
                                            new SyntaxNodeOrToken[]
                                            {
                                                IdentifierName(inputType),
                                                Token(SyntaxKind.CommaToken),
                                                IdentifierName(outputType)
                                            })))))),
                Identifier(propertyName))
                    .WithModifiers(accessibility.GetToken())
                .WithAccessorList(
                    AccessorList(SingletonList(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))))
                .WithInitializer(
                    EqualsValueClause(
                        SimpleLambdaExpression(
                            Parameter(Identifier("x")))
                        .WithExpressionBody(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("x"),
                                IdentifierName(valuePropertyName)))))
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        public static SimpleLambdaExpressionSyntax LambdaIndexer(string variableName, string arrayName, int index) =>
            SimpleLambdaExpression(Parameter(Identifier(variableName)))
                .WithExpressionBody(
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
            Argument(SimpleLambdaExpression(Parameter(Identifier(variableName)))
                .WithExpressionBody(
                    InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(variableName), IdentifierName(methodName)))));

        public static ArgumentSyntax LambdaInvokeMethodArgument(string variableName, string methodName, IEnumerable<string> members) =>
            Argument(SimpleLambdaExpression(Parameter(Identifier(variableName))).WithExpressionBody(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, MemberAccess(variableName, members), IdentifierName(methodName))));

        public static ArgumentSyntax LambdaNoVariableUseArgument(string variableName, string propertyName) =>
            Argument(SimpleLambdaExpression(Parameter(Identifier(variableName))).WithExpressionBody(IdentifierName(propertyName)));

        public static ArgumentSyntax LambdaNoVariableUseArgument(string variableName, IEnumerable<string> properties) =>
            Argument(SimpleLambdaExpression(Parameter(Identifier(variableName))).WithExpressionBody((MemberAccessExpressionSyntax)properties.Skip(1).Aggregate<string, ExpressionSyntax>(IdentifierName(properties.First()), (expression, name) =>
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, IdentifierName(name)))));
    }
}
