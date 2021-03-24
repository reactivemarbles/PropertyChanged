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

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
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
                .WithModifiers(TokenList(accessibility.GetAccessibilityTokens()))
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

        public static MethodDeclarationSyntax WhenChangedConversionWithoutBody(string inputType, string outputType, List<string> returnTypes, bool isExtension, Accessibility accessibility)
        {
            var modifiers = accessibility.GetAccessibilityTokens().ToList();

            var parameterList = new List<ParameterSyntax>();

            if (isExtension)
            {
                modifiers.Add(Token(SyntaxKind.StaticKeyword));
                parameterList.Add(Parameter(Identifier("source"))
                    .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                    .WithType(IdentifierName(inputType)));
            }

            for (int i = 0; i < returnTypes.Count; ++i)
            {
                parameterList.Add(Parameter(Identifier("propertyExpression" + (i + 1)))
                    .WithType(GenericName("Expression")
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    GenericName(Identifier("Func")).WithTypeArgumentList(
                                        TypeArgumentList(
                                            SeparatedList<TypeSyntax>(new[]
                                            {
                                                IdentifierName(inputType),
                                                IdentifierName(returnTypes[i])
                                            }))))))));
            }

            var conversionTypes = SeparatedList<TypeSyntax>(returnTypes.Select(x => IdentifierName(x)).Concat(new[] { IdentifierName(outputType) }));

            parameterList.Add(Parameter(Identifier("conversionFunc"))
                .WithType(GenericName("Func")
                    .WithTypeArgumentList(TypeArgumentList(conversionTypes))));

            parameterList.AddRange(CallerMembersParameters());

            return MethodDeclaration(IdentifierName($"IObservable<{outputType}>"), Identifier("WhenChanged"))
                .WithModifiers(TokenList(modifiers))
                .WithParameterList(ParameterList(SeparatedList(parameterList)));
        }

        public static MethodDeclarationSyntax WhenChangedWithoutBody(string inputType, string outputType, bool isExtension, Accessibility accessibility)
        {
            var modifiers = accessibility.GetAccessibilityTokens().ToList();

            var parameterList = new List<ParameterSyntax>();

            if (isExtension)
            {
                modifiers.Add(Token(SyntaxKind.StaticKeyword));
                parameterList.Add(Parameter(Identifier("source"))
                    .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                    .WithType(IdentifierName(inputType)));
            }

            parameterList.Add(Parameter(Identifier("propertyExpression"))
                .WithType(GenericName("Expression")
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList<TypeSyntax>(
                                GenericName(Identifier("Func")).WithTypeArgumentList(
                                    TypeArgumentList(
                                        SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[]
                                            {
                                                IdentifierName(inputType),
                                                Token(SyntaxKind.CommaToken),
                                                IdentifierName(outputType)
                                            }))))))));

            parameterList.AddRange(CallerMembersParameters());

            return MethodDeclaration(IdentifierName($"IObservable<{outputType}>"), Identifier("WhenChanged"))
                .WithModifiers(TokenList(modifiers))
                .WithParameterList(ParameterList(SeparatedList(parameterList)));
        }

        public static AssignmentExpressionSyntax MapEntry(string keyName, InvocationExpressionSyntax observableExpression) =>
            AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    ImplicitElementAccess()
                    .WithArgumentList(
                        BracketedArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(keyName)))))),
                    SimpleLambdaExpression(Parameter(Identifier("source")))
                        .WithExpressionBody(observableExpression));

        public static FieldDeclarationSyntax MapDictionary(string inputTypeName, string outputTypeName, string mapName, IEnumerable<ExpressionSyntax> initializerMembers)
        {
            return FieldDeclaration(
                        VariableDeclaration(
                            GenericName(
                                Identifier("Dictionary"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SeparatedList<TypeSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            PredefinedType(Token(SyntaxKind.StringKeyword)),
                                            Token(SyntaxKind.CommaToken),
                                            GenericName(
                                                Identifier("Func"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                    SeparatedList<TypeSyntax>(
                                                        new SyntaxNodeOrToken[]
                                                        {
                                                            IdentifierName(inputTypeName),
                                                            Token(SyntaxKind.CommaToken),
                                                            GenericName(
                                                                Identifier("IObservable"))
                                                            .WithTypeArgumentList(
                                                                TypeArgumentList(
                                                                    SingletonSeparatedList<TypeSyntax>(
                                                                        IdentifierName(outputTypeName))))
                                                        })))
                                        }))))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier(mapName))
                                .WithInitializer(
                                    EqualsValueClause(
                                        ObjectCreationExpression(
                                            GenericName(
                                                Identifier("Dictionary"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                    SeparatedList<TypeSyntax>(
                                                        new SyntaxNodeOrToken[]
                                                        {
                                                            PredefinedType(
                                                                Token(SyntaxKind.StringKeyword)),
                                                            Token(SyntaxKind.CommaToken),
                                                            GenericName(
                                                                Identifier("Func"))
                                                            .WithTypeArgumentList(
                                                                TypeArgumentList(
                                                                    SeparatedList<TypeSyntax>(
                                                                        new SyntaxNodeOrToken[]
                                                                        {
                                                                            IdentifierName(inputTypeName),
                                                                            Token(SyntaxKind.CommaToken),
                                                                            GenericName(
                                                                                Identifier("IObservable"))
                                                                            .WithTypeArgumentList(
                                                                                TypeArgumentList(
                                                                                    SingletonSeparatedList<TypeSyntax>(
                                                                                        IdentifierName(outputTypeName))))
                                                                        })))
                                                        }))))
                                        .WithInitializer(
                                            InitializerExpression(
                                                SyntaxKind.ObjectInitializerExpression,
                                                SeparatedList<ExpressionSyntax>(initializerMembers))))))))
                    .WithModifiers(
                        TokenList(
                            new[]
                            {
                                Token(SyntaxKind.PrivateKeyword),
                                Token(SyntaxKind.StaticKeyword),
                                Token(SyntaxKind.ReadOnlyKeyword)
                            }));
        }

        public static InvocationExpressionSyntax MapInvokeExpression(string invokeName, string mapName, string expressionParameterName) =>
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ElementAccessExpression(
                        IdentifierName(mapName))
                    .WithArgumentList(
                        BracketedArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName(expressionParameterName),
                                                IdentifierName("Body")),
                                            IdentifierName("ToString"))))))),
                    IdentifierName("Invoke")))
            .WithArgumentList(
                ArgumentList(
                    SingletonSeparatedList(
                        Argument(
                            IdentifierName(invokeName)))));

        public static InvocationExpressionSyntax SelectObservableNotifyPropertyChangedSwitch(InvocationExpressionSyntax sourceInvoke, string returnType, string inputName, string memberName) =>
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            sourceInvoke,
                            IdentifierName("Select")))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(SimpleLambdaExpression(Parameter(Identifier(inputName)))
                                                .WithExpressionBody(InvocationExpression(ObservableNotifyPropertyChanged(returnType, inputName, memberName))))))),
                    IdentifierName("Switch")));

        public static InvocationExpressionSyntax GetObservableChain(string inputName, List<(string Name, string InputType, string OutputType)> members)
        {
            InvocationExpressionSyntax observable = null;
            for (int i = 0; i < members.Count; ++i)
            {
                var member = members[i];

                if (i == 0)
                {
                    observable = ObservableNotifyPropertyChanged(member.OutputType, inputName, member.Name);
                }
                else
                {
                    observable = SelectObservableNotifyPropertyChangedSwitch(observable, member.OutputType, "source", member.Name);
                }
            }

            return observable;
        }

        public static InvocationExpressionSyntax ObservableNotifyPropertyChanged(string returnType, string inputName, string memberName) =>
            InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("Observable"), GenericName(Identifier("Create"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(IdentifierName(returnType))))))
                            .WithArgumentList(
                                ArgumentList(SingletonSeparatedList(
                                    Argument(
                                        SimpleLambdaExpression(
                                            Parameter(
                                                Identifier("observer")))
                                        .WithBlock(
                                            Block(
                                                IfStatement(
                                                    BinaryExpression(
                                                        SyntaxKind.EqualsExpression,
                                                        IdentifierName(inputName),
                                                        LiteralExpression(
                                                            SyntaxKind.NullLiteralExpression)),
                                                    Block(
                                                        SingletonList<StatementSyntax>(
                                                            ReturnStatement(
                                                                MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName("Disposable"),
                                                                    IdentifierName("Empty")))))),
                                                ExpressionStatement(
                                                    InvocationExpression(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("observer"),
                                                            IdentifierName("OnNext")))
                                                    .WithArgumentList(
                                                        ArgumentList(
                                                            SingletonSeparatedList(
                                                                Argument(
                                                                    MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        IdentifierName(inputName),
                                                                        IdentifierName(memberName))))))),
                                                LocalDeclarationStatement(
                                                    VariableDeclaration(
                                                        IdentifierName("PropertyChangedEventHandler"))
                                                    .WithVariables(
                                                        SingletonSeparatedList(
                                                            VariableDeclarator(
                                                                Identifier("handler"))
                                                            .WithInitializer(
                                                                EqualsValueClause(
                                                                    ParenthesizedLambdaExpression()
                                                                    .WithParameterList(
                                                                        ParameterList(
                                                                            SeparatedList<ParameterSyntax>(
                                                                                new SyntaxNodeOrToken[]
                                                                                {
                                                                                    Parameter(
                                                                                        Identifier("sender")),
                                                                                    Token(SyntaxKind.CommaToken),
                                                                                    Parameter(
                                                                                        Identifier("e"))
                                                                                })))
                                                                    .WithBlock(
                                                                        Block(
                                                                            SingletonList<StatementSyntax>(
                                                                                IfStatement(
                                                                                    BinaryExpression(
                                                                                        SyntaxKind.EqualsExpression,
                                                                                        MemberAccessExpression(
                                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                                            IdentifierName("e"),
                                                                                            IdentifierName("PropertyName")),
                                                                                        LiteralExpression(
                                                                                            SyntaxKind.StringLiteralExpression,
                                                                                            Literal(memberName))),
                                                                                    Block(
                                                                                        SingletonList<StatementSyntax>(
                                                                                            ExpressionStatement(
                                                                                                InvocationExpression(
                                                                                                    MemberAccessExpression(
                                                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                                                        IdentifierName("observer"),
                                                                                                        IdentifierName("OnNext")))
                                                                                                .WithArgumentList(
                                                                                                    ArgumentList(SingletonSeparatedList(
                                                                                                            Argument(MemberAccessExpression(
                                                                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                                                                    IdentifierName(inputName),
                                                                                                                    IdentifierName(memberName)))))))))))))))))),
                                                ExpressionStatement(
                                                    AssignmentExpression(
                                                        SyntaxKind.AddAssignmentExpression,
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName(inputName),
                                                            IdentifierName("PropertyChanged")),
                                                        IdentifierName("handler"))),
                                                ReturnStatement(
                                                    InvocationExpression(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("Disposable"),
                                                            IdentifierName("Create")))
                                                    .WithArgumentList(
                                                        ArgumentList(
                                                            SeparatedList<ArgumentSyntax>(
                                                                new SyntaxNodeOrToken[]
                                                                {
                                                                    Argument(
                                                                        TupleExpression(
                                                                            SeparatedList<ArgumentSyntax>(
                                                                                new SyntaxNodeOrToken[]
                                                                                {
                                                                                    Argument(
                                                                                        IdentifierName(memberName))
                                                                                    .WithNameColon(
                                                                                        NameColon(
                                                                                            IdentifierName("Parent"))),
                                                                                    Token(SyntaxKind.CommaToken),
                                                                                    Argument(
                                                                                        IdentifierName("handler"))
                                                                                    .WithNameColon(
                                                                                        NameColon(
                                                                                            IdentifierName("Handler")))
                                                                                }))),
                                                                    Token(SyntaxKind.CommaToken),
                                                                    Argument(
                                                                        SimpleLambdaExpression(
                                                                            Parameter(
                                                                                Identifier("x")))
                                                                        .WithExpressionBody(
                                                                            AssignmentExpression(
                                                                                SyntaxKind.SubtractAssignmentExpression,
                                                                                MemberAccessExpression(
                                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                                    MemberAccessExpression(
                                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                                        IdentifierName("x"),
                                                                                        IdentifierName("Parent")),
                                                                                    IdentifierName("PropertyChanged")),
                                                                                MemberAccessExpression(
                                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                                    IdentifierName("x"),
                                                                                    IdentifierName("Handler")))))
                                                                }))))))))));

        public static IEnumerable<ParameterSyntax> CallerMembersParameters() =>
            new ParameterSyntax[]
            {
                 Parameter(Identifier("callerMemberName"))
                    .WithAttributeLists(
                        SingletonList(
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("CallerMemberName"))))))
                    .WithType(
                        PredefinedType(
                            Token(SyntaxKind.StringKeyword)))
                    .WithDefault(
                        EqualsValueClause(
                            LiteralExpression(
                                SyntaxKind.NullLiteralExpression))),

                 Parameter(Identifier("callerFilePath"))
                    .WithAttributeLists(
                        SingletonList(
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("CallerFilePath"))))))
                    .WithType(
                        PredefinedType(
                            Token(SyntaxKind.StringKeyword)))
                    .WithDefault(
                        EqualsValueClause(
                            LiteralExpression(
                                SyntaxKind.NullLiteralExpression))),

                 Parameter(Identifier("callerLineNumber"))
                    .WithAttributeLists(
                        SingletonList(
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("CallerLineNumber"))))))
                    .WithType(
                        PredefinedType(
                            Token(SyntaxKind.IntKeyword)))
                    .WithDefault(
                        EqualsValueClause(
                            LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                Literal(0))))
            };

        public static MethodDeclarationSyntax GetMethodToProperty(string propertyType, string propertyName, string methodName, Accessibility accessibility) =>
            MethodDeclaration(
                    IdentifierName(propertyType),
                    Identifier(methodName))
                .WithModifiers(TokenList(accessibility.GetAccessibilityTokens()))
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
                    .WithModifiers(TokenList(accessibility.GetAccessibilityTokens()))
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
                    .WithModifiers(TokenList(accessibility.GetAccessibilityTokens()))
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
