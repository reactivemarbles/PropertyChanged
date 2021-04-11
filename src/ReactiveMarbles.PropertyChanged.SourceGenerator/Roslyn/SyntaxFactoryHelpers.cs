// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    /// <summary>
    /// Versions of the syntax factory that handles some trivia for us automatically, to avoid the NormalizeWhitespace command.
    /// </summary>
    internal static class SyntaxFactoryHelpers
    {
        private const int LeadingSpacesPerLevel = 4;

        public static SyntaxTrivia Space => SyntaxFactory.Space;

        public static SyntaxTrivia CarriageReturnLineFeed => SyntaxFactory.CarriageReturnLineFeed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ThisExpressionSyntax ThisExpression() => SyntaxFactory.ThisExpression();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OmittedArraySizeExpressionSyntax OmittedArraySize() => SyntaxFactory.OmittedArraySizeExpression();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NamespaceDeclarationSyntax NamespaceDeclaration(string nameText, IReadOnlyCollection<MemberDeclarationSyntax> members, bool createLeadingNewLine)
        {
            var name = IdentifierName(nameText).AddLeadingSpaces();

            var membersList = members == null || members.Count == 0 ? default : List(GetIndentedNodes(members, 1));

            var namespaceToken = createLeadingNewLine ? SyntaxFactory.Token(SyntaxKind.NamespaceKeyword).AddLeadingNewLines() : SyntaxFactory.Token(SyntaxKind.NamespaceKeyword);

            return SyntaxFactory.NamespaceDeclaration(namespaceToken, name, SyntaxFactory.Token(SyntaxKind.OpenBraceToken).AddLeadingNewLines(), default, default, membersList, SyntaxFactory.Token(SyntaxKind.CloseBraceToken), default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IfStatementSyntax IfStatement(ExpressionSyntax conditionStatement, StatementSyntax statement) =>
            SyntaxFactory.IfStatement(default, Token(SyntaxKind.IfKeyword).AddTrialingSpaces(), Token(SyntaxKind.OpenParenToken).AddTrialingSpaces(), conditionStatement, Token(SyntaxKind.CloseParenToken).AddTrialingSpaces(), statement, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IfStatementSyntax IfStatement(ExpressionSyntax conditionStatement, StatementSyntax statement, ElseClauseSyntax elseStatement) =>
            SyntaxFactory.IfStatement(default, Token(SyntaxKind.IfKeyword).AddTrialingSpaces(), Token(SyntaxKind.OpenParenToken).AddTrialingSpaces(), conditionStatement, Token(SyntaxKind.CloseParenToken).AddTrialingSpaces(), statement, elseStatement);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssignmentExpressionSyntax AssignmentExpression(SyntaxKind token, ExpressionSyntax left, ExpressionSyntax right) => SyntaxFactory.AssignmentExpression(token, left, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssignmentExpressionSyntax AssignmentExpression(SyntaxKind token, ExpressionSyntax left, string right) => SyntaxFactory.AssignmentExpression(token, left, IdentifierName(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssignmentExpressionSyntax AssignmentExpression(SyntaxKind token, string left, ExpressionSyntax right) => SyntaxFactory.AssignmentExpression(token, IdentifierName(left), right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssignmentExpressionSyntax AssignmentExpression(SyntaxKind token, string left, string right) => SyntaxFactory.AssignmentExpression(token, IdentifierName(left), IdentifierName(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttributeArgumentSyntax AttributeArgument(ExpressionSyntax expression) => SyntaxFactory.AttributeArgument(expression);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttributeArgumentSyntax AttributeArgument(NameEqualsSyntax nameEquals, ExpressionSyntax expression) => SyntaxFactory.AttributeArgument(nameEquals, default, expression);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NameEqualsSyntax NameEquals(IdentifierNameSyntax name) => SyntaxFactory.NameEquals(name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IdentifierNameSyntax IdentifierName(string name) => SyntaxFactory.IdentifierName(name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttributeSyntax Attribute(string name) => Attribute(name, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttributeSyntax Attribute(string name, IReadOnlyCollection<AttributeArgumentSyntax> arguments)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var argumentsList = AttributeArgumentList(arguments);

            if (name.EndsWith("Attribute", StringComparison.InvariantCulture))
            {
                var lastIndex = name.LastIndexOf("Attribute", StringComparison.InvariantCulture);
                name = name.Substring(0, lastIndex);
            }

            return SyntaxFactory.Attribute(IdentifierName(name), argumentsList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttributeArgumentListSyntax AttributeArgumentList(IReadOnlyCollection<AttributeArgumentSyntax> arguments)
        {
            if (arguments == null || arguments.Count == 0)
            {
                return default;
            }

            if (arguments.Count == 1)
            {
                return SyntaxFactory.AttributeArgumentList(SingletonSeparatedList(arguments.First()));
            }

            return SyntaxFactory.AttributeArgumentList(SeparatedList(arguments));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CompilationUnitSyntax CompilationUnit(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<MemberDeclarationSyntax> members, IReadOnlyCollection<UsingDirectiveSyntax> usings)
        {
            var attributesList = attributes == null || attributes.Count == 0 ? default : List(GetIndentedNodes(attributes, 0, true));
            var membersList = members?.Count > 0 ? List(members) : default;
            var usingList = usings?.Count > 0 ? List(usings) : default;
            return SyntaxFactory.CompilationUnit(default, usingList, attributesList, membersList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttributeListSyntax AttributeList(AttributeSyntax attribute) => AttributeList(attribute, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttributeListSyntax AttributeList(AttributeSyntax attribute, SyntaxKind? target)
        {
            var attributeList = SyntaxFactory.SingletonSeparatedList(attribute);

            AttributeTargetSpecifierSyntax attributeTarget = null;
            if (target != null)
            {
                attributeTarget = SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(target.Value), SyntaxFactory.Token(SyntaxKind.ColonToken).AddTrialingSpaces());
            }

            return SyntaxFactory.AttributeList(SyntaxFactory.Token(SyntaxKind.OpenBracketToken), attributeTarget, attributeList, SyntaxFactory.Token(SyntaxKind.CloseBracketToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UsingDirectiveSyntax UsingDirective(string identifier) => SyntaxFactory.UsingDirective(Token(SyntaxKind.UsingKeyword).AddTrialingSpaces(), default(SyntaxToken), null, IdentifierName(identifier), Token(SyntaxKind.SemicolonToken).AddTrialingNewLines());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(IReadOnlyCollection<TNode> nodes)
            where TNode : SyntaxNode
        {
            if (nodes == null || nodes.Count == 0)
            {
                return default;
            }

            if (nodes.Count == 1)
            {
                return SingletonSeparatedList(nodes.First());
            }

            var commaSeparation = Enumerable.Repeat(SyntaxFactory.Token(SyntaxKind.CommaToken).AddTrialingSpaces(), nodes.Count - 1);
            return SyntaxFactory.SeparatedList(nodes, commaSeparation);
        }

        public static ConditionalAccessExpressionSyntax ConditionalAccessExpression(ExpressionSyntax expression, ExpressionSyntax whenNotNull) => SyntaxFactory.ConditionalAccessExpression(expression, SyntaxFactory.Token(SyntaxKind.QuestionToken).AddTrialingSpaces(), whenNotNull);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArgumentListSyntax ArgumentList(IReadOnlyCollection<ArgumentSyntax> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return default;
            }

            if (nodes.Count == 1)
            {
                return SyntaxFactory.ArgumentList(SingletonSeparatedList(nodes.First()));
            }

            return SyntaxFactory.ArgumentList(SeparatedList<ArgumentSyntax>(nodes));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberAccessExpressionSyntax MemberAccess(string startVariable, params string[] identifierNames) =>
            (MemberAccessExpressionSyntax)identifierNames.Aggregate<string, ExpressionSyntax>(IdentifierName(startVariable), (expression, name) =>
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, IdentifierName(name)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax startVariable, params string[] identifierNames) =>
            (MemberAccessExpressionSyntax)identifierNames.Aggregate(startVariable, (expression, name) =>
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, IdentifierName(name)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax startVariable, IEnumerable<string> identifierNames) =>
            MemberAccess(startVariable, identifierNames.ToArray());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberAccessExpressionSyntax MemberAccess(string startVariable, IEnumerable<string> identifierNames) =>
            MemberAccess(startVariable, identifierNames.ToArray());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArgumentSyntax LambdaAccessArgument(string variableName, MemberAccessExpressionSyntax members) =>
            Argument(SimpleLambdaExpression(Parameter(variableName), members));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static InvocationExpressionSyntax InvokeExplicitMethod(string className, string methodName, params ArgumentSyntax[] arguments) =>
            InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(className), IdentifierName(methodName)), arguments);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static InvocationExpressionSyntax InvokeMethod(string methodName, ExpressionSyntax instance, params ArgumentSyntax[] arguments) =>
            InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, instance, IdentifierName(methodName)), arguments);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SimpleLambdaExpressionSyntax SimpleLambdaExpression(ParameterSyntax parameter, ExpressionSyntax body) =>
            SyntaxFactory.SimpleLambdaExpression(parameter, default, body);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SimpleLambdaExpressionSyntax SimpleLambdaExpression(ParameterSyntax parameter, BlockSyntax body) =>
            SyntaxFactory.SimpleLambdaExpression(parameter, body, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberBindingExpressionSyntax MemberBindingExpression(string name) => SyntaxFactory.MemberBindingExpression(Token(SyntaxKind.DotToken).AddTrialingSpaces(), IdentifierName(name));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ParameterSyntax Parameter(string name) => Parameter(default, default, default, name, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ParameterSyntax Parameter(string name, IReadOnlyCollection<SyntaxKind> modifier) => Parameter(default, modifier, default, name, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ParameterSyntax Parameter(string type, string name, IReadOnlyCollection<SyntaxKind> modifier) => Parameter(default, modifier, IdentifierName(type).AddTrialingSpaces(), name, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ParameterSyntax Parameter(string type, string name) => Parameter(default, default, IdentifierName(type).AddTrialingSpaces(), name, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ParameterSyntax Parameter(string type, string name, EqualsValueClauseSyntax equals) => Parameter(default, default, IdentifierName(type).AddTrialingSpaces(), name, equals);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ParameterSyntax Parameter(IReadOnlyCollection<AttributeListSyntax> attributes, string type, string name) => Parameter(attributes, default, IdentifierName(type).AddTrialingSpaces(), name, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ParameterSyntax Parameter(IReadOnlyCollection<AttributeListSyntax> attributes, string type, string name, EqualsValueClauseSyntax equals) => Parameter(attributes, default, IdentifierName(type).AddTrialingSpaces(), name, equals);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ParameterSyntax Parameter(TypeSyntax type, string name) => Parameter(default, default, type.AddTrialingSpaces(), name, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ParameterSyntax Parameter(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, TypeSyntax type, string identifier, EqualsValueClauseSyntax equals)
        {
            var name = SyntaxFactory.Identifier(identifier);
            type = type.AddTrialingSpaces();
            var modifiersList = TokenList(modifiers);
            var attributesList = List(attributes);
            equals = equals?.AddLeadingSpaces();

            return SyntaxFactory.Parameter(attributesList, modifiersList, type, name, equals);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static InvocationExpressionSyntax InvocationExpression(string methodName, IReadOnlyCollection<ArgumentSyntax> arguments) =>
            SyntaxFactory.InvocationExpression(IdentifierName(methodName), ArgumentList(arguments));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static InvocationExpressionSyntax InvocationExpression(string methodName) =>
            SyntaxFactory.InvocationExpression(IdentifierName(methodName));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static InvocationExpressionSyntax InvocationExpression(ExpressionSyntax expression, IReadOnlyCollection<ArgumentSyntax> arguments) =>
            SyntaxFactory.InvocationExpression(expression, ArgumentList(arguments));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static InvocationExpressionSyntax InvocationExpression(ExpressionSyntax expression) =>
            SyntaxFactory.InvocationExpression(expression);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImplicitElementAccessSyntax ImplicitElementAccess(IReadOnlyCollection<ArgumentSyntax> arguments) =>
            SyntaxFactory.ImplicitElementAccess(BracketedArgumentList(arguments));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BracketedArgumentListSyntax BracketedArgumentList(IReadOnlyCollection<ArgumentSyntax> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return default;
            }

            if (nodes.Count == 1)
            {
                return SyntaxFactory.BracketedArgumentList(SingletonSeparatedList(nodes.First()));
            }

            return SyntaxFactory.BracketedArgumentList(SeparatedList(nodes));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SeparatedSyntaxList<EnumMemberDeclarationSyntax> SeparatedList(IReadOnlyCollection<EnumMemberDeclarationSyntax> nodes, int level)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return default;
            }

            if (nodes.Count == 1)
            {
                return SingletonSeparatedList(nodes.First());
            }

            return SyntaxFactory.SeparatedList(GetIndentedNodes(nodes, level));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SeparatedSyntaxList<TNode> SingletonSeparatedList<TNode>(TNode node)
            where TNode : SyntaxNode => SyntaxFactory.SingletonSeparatedList(node);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ParenthesizedLambdaExpressionSyntax ParenthesizedLambdaExpression(IReadOnlyCollection<ParameterSyntax> parameters, BlockSyntax block)
        {
            var parameterList = ParameterList(parameters);
            return SyntaxFactory.ParenthesizedLambdaExpression(parameterList, block);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ParameterListSyntax ParameterList(IReadOnlyCollection<ParameterSyntax> parameters)
        {
            var (openParentheses, closeParentheses) = GetParentheses();

            return SyntaxFactory.ParameterList(openParentheses, SeparatedList<ParameterSyntax>(parameters), closeParentheses);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Identifier(string text) => SyntaxFactory.Identifier(text);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VariableDeclaratorSyntax VariableDeclarator(string identifier)
        {
            var name = SyntaxFactory.Identifier(identifier);
            return SyntaxFactory.VariableDeclarator(name, default, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VariableDeclaratorSyntax VariableDeclarator(string identifier, EqualsValueClauseSyntax initializer)
        {
            var name = SyntaxFactory.Identifier(identifier);
            return SyntaxFactory.VariableDeclarator(name, default, initializer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EventFieldDeclarationSyntax EventFieldDeclaration(IReadOnlyCollection<SyntaxKind> modifiers, string eventType, string eventName, int level)
        {
            var variableDeclaration = VariableDeclaration(IdentifierName(eventType), SeparatedList(new[] { VariableDeclarator(eventName) }));

            return EventFieldDeclaration(default, modifiers, variableDeclaration, level);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EventFieldDeclarationSyntax EventFieldDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, VariableDeclarationSyntax declaration, int level)
        {
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes?.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);

            return SyntaxFactory.EventFieldDeclaration(attributeList, modifiersList, SyntaxFactory.Token(SyntaxKind.EventKeyword).AddTrialingSpaces(), declaration, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VariableDeclarationSyntax VariableDeclaration(TypeSyntax type) => SyntaxFactory.VariableDeclaration(type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VariableDeclarationSyntax VariableDeclaration(TypeSyntax type, IReadOnlyCollection<VariableDeclaratorSyntax> variableDeclaratorSyntaxes)
        {
            var variableDeclaratorList = SeparatedList(variableDeclaratorSyntaxes);
            return SyntaxFactory.VariableDeclaration(type.AddTrialingSpaces(), variableDeclaratorList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VariableDeclarationSyntax VariableDeclaration(string type, IReadOnlyCollection<VariableDeclaratorSyntax> variableDeclaratorSyntaxes)
        {
            var variableDeclaratorList = SeparatedList(variableDeclaratorSyntaxes);
            return SyntaxFactory.VariableDeclaration(IdentifierName(type).AddTrialingSpaces(), variableDeclaratorList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VariableDeclarationSyntax VariableDeclaration(string type) => SyntaxFactory.VariableDeclaration(IdentifierName(type), default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConstructorDeclarationSyntax ConstructorDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<ParameterSyntax> parameters, string identifier, int level)
        {
            var name = SyntaxFactory.Identifier(identifier);
            var parametersList = SyntaxFactory.ParameterList(SeparatedList(parameters));
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes?.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            return SyntaxFactory.ConstructorDeclaration(attributeList, modifiersList, name, parametersList, default, default, default, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DestructorDeclarationSyntax DestructorDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, string identifier, int level)
        {
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes?.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            return SyntaxFactory.DestructorDeclaration(attributeList, modifiersList, SyntaxFactory.Token(SyntaxKind.TildeToken), name, SyntaxFactory.ParameterList(), default, default, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodDeclarationSyntax MethodDeclaration(IReadOnlyCollection<SyntaxKind> modifiers, TypeSyntax type, string identifier, IReadOnlyCollection<ParameterSyntax> parameters, int level, BlockSyntax body) =>
            MethodDeclaration(default, modifiers, type, default, identifier, parameters, default, default, body, default, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodDeclarationSyntax MethodDeclaration(IReadOnlyCollection<SyntaxKind> modifiers, TypeSyntax type, string identifier, int level, ArrowExpressionClauseSyntax body) =>
            MethodDeclaration(default, modifiers, type, default, identifier, default, default, default, default, body, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodDeclarationSyntax MethodDeclaration(IReadOnlyCollection<SyntaxKind> modifiers, TypeSyntax type, string identifier, IReadOnlyCollection<ParameterSyntax> parameters, int level, ArrowExpressionClauseSyntax body) =>
            MethodDeclaration(default, modifiers, type, default, identifier, parameters, default, default, default, body, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodDeclarationSyntax MethodDeclaration(IReadOnlyCollection<SyntaxKind> modifiers, string typeName, string identifier, IReadOnlyCollection<ParameterSyntax> parameters, int level, BlockSyntax body) =>
            MethodDeclaration(default, modifiers, IdentifierName(typeName), default, identifier, parameters, default, default, body, default, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodDeclarationSyntax MethodDeclaration(IReadOnlyCollection<SyntaxKind> modifiers, string typeName, string identifier, IReadOnlyCollection<ParameterSyntax> parameters, int level, ArrowExpressionClauseSyntax body) =>
            MethodDeclaration(default, modifiers, IdentifierName(typeName), default, identifier, parameters, default, default, default, body, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodDeclarationSyntax MethodDeclaration(IReadOnlyCollection<SyntaxKind> modifiers, string typeName, string identifier, int level, ArrowExpressionClauseSyntax body) =>
            MethodDeclaration(default, modifiers, IdentifierName(typeName), default, identifier, default, default, default, default, body, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodDeclarationSyntax MethodDeclaration(IReadOnlyCollection<SyntaxKind> modifiers, string typeName, string identifier, IReadOnlyCollection<ParameterSyntax> parameters, IReadOnlyCollection<TypeParameterSyntax> typeParameters, int level, BlockSyntax body) =>
            MethodDeclaration(default, modifiers, IdentifierName(typeName), default, identifier, parameters, default, typeParameters, body, default, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodDeclarationSyntax MethodDeclaration(IReadOnlyCollection<SyntaxKind> modifiers, string typeName, string identifier, IReadOnlyCollection<ParameterSyntax> parameters, IReadOnlyCollection<TypeParameterSyntax> typeParameters, int level, ArrowExpressionClauseSyntax body) =>
            MethodDeclaration(default, modifiers, IdentifierName(typeName), default, identifier, parameters, default, typeParameters, default, body, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodDeclarationSyntax MethodDeclaration(IReadOnlyCollection<SyntaxKind> modifiers, string typeName, string identifier, IReadOnlyCollection<TypeParameterSyntax> typeParameters, int level, ArrowExpressionClauseSyntax body) =>
            MethodDeclaration(default, modifiers, IdentifierName(typeName), default, identifier, default, default, typeParameters, default, body, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodDeclarationSyntax MethodDeclaration(IReadOnlyCollection<SyntaxKind> modifiers, TypeSyntax type, string identifier, IReadOnlyCollection<TypeParameterSyntax> typeParameters, int level, ArrowExpressionClauseSyntax body) =>
            MethodDeclaration(default, modifiers, type, default, identifier, default, default, typeParameters, default, body, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodDeclarationSyntax MethodDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterface, string identifier, IReadOnlyCollection<ParameterSyntax> parameters, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, BlockSyntax body, ArrowExpressionClauseSyntax arrowSyntax, int level)
        {
            var name = SyntaxFactory.Identifier(identifier);
            if (explicitInterface == null)
            {
                name = name.AddLeadingSpaces();
            }

            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes?.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);

            var typeParameterList = TypeParameterList(typeParameters);
            var typeParameterConstraintList = typeParameterConstraintClauses?.Count > 0 ? List(GetIndentedNodes(typeParameterConstraintClauses, level + 1)) : default;

            var parametersList = SyntaxFactory.ParameterList(SeparatedList(parameters));

            explicitInterface = explicitInterface?.AddLeadingSpaces();

            SyntaxToken token = body == default ? SyntaxFactory.Token(SyntaxKind.SemicolonToken) : default;

            return SyntaxFactory.MethodDeclaration(attributeList, modifiersList, type, explicitInterface, name, typeParameterList, parametersList, typeParameterConstraintList, body, arrowSyntax, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConversionOperatorDeclarationSyntax ConversionOperatorDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, SyntaxToken implicitOrExplicitKeyword, string type, IReadOnlyCollection<ParameterSyntax> parameters, int level)
        {
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes?.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            var typeName = SyntaxFactory.IdentifierName(type).AddLeadingSpaces();

            var parametersList = SyntaxFactory.ParameterList(SeparatedList(parameters));

            return SyntaxFactory.ConversionOperatorDeclaration(attributeList, modifiersList, implicitOrExplicitKeyword, SyntaxFactory.Token(SyntaxKind.OperatorKeyword).AddTrialingSpaces().AddLeadingSpaces(), typeName, parametersList, default, default, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OperatorDeclarationSyntax OperatorDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<ParameterSyntax> parameters, TypeSyntax returnType, SyntaxToken operatorToken, int level)
        {
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes?.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            var parametersList = SyntaxFactory.ParameterList(SeparatedList(parameters));
            return SyntaxFactory.OperatorDeclaration(attributeList, modifiersList, returnType, SyntaxFactory.Token(SyntaxKind.OperatorKeyword).AddTrialingSpaces().AddLeadingSpaces(), operatorToken, parametersList, default, default, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Token(SyntaxKind kind) => SyntaxFactory.Token(kind);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyDeclarationSyntax PropertyDeclaration(string typeName, string identifier, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<AccessorDeclarationSyntax> accessors, int level) =>
            PropertyDeclaration(IdentifierName(typeName), identifier, default, modifiers, accessors, default, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyDeclarationSyntax PropertyDeclaration(TypeSyntax type, string identifier, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<AccessorDeclarationSyntax> accessors, int level) =>
            PropertyDeclaration(type, identifier, default, modifiers, accessors, default, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyDeclarationSyntax PropertyDeclaration(TypeSyntax type, string identifier, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<AccessorDeclarationSyntax> accessors, EqualsValueClauseSyntax initializer, int level) =>
            PropertyDeclaration(type, identifier, default, modifiers, accessors, initializer, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyDeclarationSyntax PropertyDeclaration(TypeSyntax type, string identifier, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<AccessorDeclarationSyntax> accessors, EqualsValueClauseSyntax initializer, int level)
        {
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes?.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            var accessorList = SyntaxFactory.AccessorList(SyntaxFactory.Token(SyntaxKind.OpenBraceToken).AddTrialingSpaces().AddLeadingSpaces(), List(GetSpacedAccessors(accessors)), SyntaxFactory.Token(SyntaxKind.CloseBraceToken).AddLeadingSpaces());

            return SyntaxFactory.PropertyDeclaration(attributeList, modifiersList, type, default, name, accessorList, default, initializer, Token(SyntaxKind.SemicolonToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrowExpressionClauseSyntax ArrowExpressionClause(ExpressionSyntax expression) => SyntaxFactory.ArrowExpressionClause(expression);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind) =>
            SyntaxFactory.AccessorDeclaration(kind).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers)
        {
            var modifiersList = TokenList(modifiers);
            var attributesList = List(attributes);
            return SyntaxFactory.AccessorDeclaration(kind, attributesList, modifiersList, default, default).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, BlockSyntax body)
        {
            var modifiersList = TokenList(modifiers);
            var attributesList = List(attributes);
            return SyntaxFactory.AccessorDeclaration(kind, attributesList, modifiersList, body, default).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, ArrowExpressionClauseSyntax body)
        {
            var modifiersList = TokenList(modifiers);
            var attributesList = List(attributes);
            return SyntaxFactory.AccessorDeclaration(kind, attributesList, modifiersList, default, body).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldDeclarationSyntax FieldDeclaration(string type, string name, IReadOnlyCollection<SyntaxKind> modifiers, int level) => FieldDeclaration(type, name, default, modifiers, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldDeclarationSyntax FieldDeclaration(TypeSyntax type, string name, IReadOnlyCollection<SyntaxKind> modifiers, int level) => FieldDeclaration(type, name, Array.Empty<AttributeListSyntax>(), modifiers, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldDeclarationSyntax FieldDeclaration(string type, string name, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, int level) =>
            FieldDeclaration(IdentifierName(type), name, attributes, modifiers, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldDeclarationSyntax FieldDeclaration(TypeSyntax type, string name, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, int level)
        {
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = modifiers?.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);

            var variableDeclaration = VariableDeclaration(type, SeparatedList(new[] { VariableDeclarator(name) }));

            return SyntaxFactory.FieldDeclaration(attributeList, modifiersList, variableDeclaration, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldDeclarationSyntax FieldDeclaration(TypeSyntax type, string name, EqualsValueClauseSyntax initializer, IReadOnlyCollection<SyntaxKind> modifiers, int level)
        {
            var modifiersList = modifiers?.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);

            var variableDeclaration = VariableDeclaration(type, SeparatedList(new[] { VariableDeclarator(name, initializer) }));

            return SyntaxFactory.FieldDeclaration(default, modifiersList, variableDeclaration, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldDeclarationSyntax FieldDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, VariableDeclarationSyntax declaration, int level)
        {
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes?.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            return SyntaxFactory.FieldDeclaration(attributeList, modifiersList, declaration, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockSyntax Block(IReadOnlyCollection<StatementSyntax> statements, int level)
        {
            var statementsList = List(statements);
            var (openingBrace, closingBrace) = GetBraces(level);
            return SyntaxFactory.Block(default, openingBrace, statementsList, closingBrace);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeParameterSyntax TypeParameter(string identifier) => TypeParameter(default, default, identifier);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeParameterSyntax TypeParameter(IReadOnlyCollection<AttributeListSyntax> attributes, SyntaxKind varianceKind, string identifier)
        {
            var name = SyntaxFactory.Identifier(identifier);
            var varianceKeyword = varianceKind == SyntaxKind.None ? default : SyntaxFactory.Token(varianceKind).AddTrialingSpaces();
            var attributesList = List(attributes);
            return SyntaxFactory.TypeParameter(attributesList, varianceKeyword, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnumMemberDeclarationSyntax EnumMemberDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, string identifier, EqualsValueClauseSyntax equalsValue)
        {
            var attributesList = List(attributes);
            var name = SyntaxFactory.Identifier(identifier);
            return SyntaxFactory.EnumMemberDeclaration(attributesList, name, equalsValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BaseListSyntax BaseList(BaseTypeSyntax baseType)
        {
            if (baseType == null)
            {
                return default;
            }

            return SyntaxFactory.BaseList(SyntaxFactory.Token(SyntaxKind.ColonToken).AddLeadingSpaces().AddTrialingSpaces(), SingletonSeparatedList(baseType));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ExpressionStatementSyntax ExpressionStatement(ExpressionSyntax expression) =>
            SyntaxFactory.ExpressionStatement(default, expression, SyntaxFactory.Token(SyntaxKind.SemicolonToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ElementAccessExpressionSyntax ElementAccessExpression(ExpressionSyntax expression) => SyntaxFactory.ElementAccessExpression(expression);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ElementAccessExpressionSyntax ElementAccessExpression(string expression) => SyntaxFactory.ElementAccessExpression(IdentifierName(expression));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ElementAccessExpressionSyntax ElementAccessExpression(string expression, IReadOnlyCollection<ArgumentSyntax> arguments)
        {
            var argumentsList = SeparatedList(arguments);
            var (openBrace, closeBrace) = GetBrackets();
            var bracketedArgumentList = SyntaxFactory.BracketedArgumentList(openBrace, argumentsList, closeBrace);

            return SyntaxFactory.ElementAccessExpression(IdentifierName(expression), bracketedArgumentList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BaseListSyntax BaseList(IReadOnlyCollection<BaseTypeSyntax> baseItems)
        {
            if (baseItems == null || baseItems.Count == 0)
            {
                return default;
            }

            return SyntaxFactory.BaseList(SyntaxFactory.Token(SyntaxKind.ColonToken).AddLeadingSpaces().AddTrialingSpaces(), SeparatedList(baseItems));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReturnStatementSyntax ReturnStatement(ExpressionSyntax expression) => SyntaxFactory.ReturnStatement(default, Token(SyntaxKind.ReturnKeyword).AddTrialingSpaces(), expression, Token(SyntaxKind.SemicolonToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SimpleBaseTypeSyntax SimpleBaseType(TypeSyntax type) => SyntaxFactory.SimpleBaseType(type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GenericNameSyntax GenericName(string name, IReadOnlyCollection<TypeSyntax> types)
        {
            var typesList = types == null || types.Count == 0 ? SyntaxFactory.TypeArgumentList() : SyntaxFactory.TypeArgumentList(SeparatedList(types));
            return SyntaxFactory.GenericName(Identifier(name), typesList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EqualsValueClauseSyntax EqualsValueClause(ExpressionSyntax value) => SyntaxFactory.EqualsValueClause(SyntaxFactory.Token(SyntaxKind.EqualsToken).AddLeadingSpaces().AddTrialingSpaces(), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DelegateDeclarationSyntax DelegateDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, TypeSyntax returnType, string identifier, IReadOnlyCollection<ParameterSyntax> parameters, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, int level)
        {
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var modifiersList = attributes?.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            var typeParameterList = TypeParameterList(typeParameters);
            var typeParameterConstraintList = List(typeParameterConstraintClauses);
            var parametersList = SyntaxFactory.ParameterList(SeparatedList(parameters));
            return SyntaxFactory.DelegateDeclaration(attributeList, modifiersList, SyntaxFactory.Token(SyntaxKind.DelegateKeyword), returnType.AddLeadingSpaces(), name, typeParameterList, parametersList, typeParameterConstraintList, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnumDeclarationSyntax EnumDeclaration(string identifier, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<EnumMemberDeclarationSyntax> members, IReadOnlyCollection<SyntaxKind> modifiers, string baseIdentifier, int level)
        {
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var modifiersList = attributes?.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            var membersList = SeparatedList(members, level + 1);

            var baseList = !string.IsNullOrWhiteSpace(baseIdentifier) ?
                BaseList(SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(baseIdentifier))) :
                default;

            var (openingBrace, closingBrace) = GetBraces(level);
            return SyntaxFactory.EnumDeclaration(attributeList, modifiersList, SyntaxFactory.Token(SyntaxKind.EnumKeyword), name, baseList, openingBrace, membersList, closingBrace, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ClassDeclarationSyntax ClassDeclaration(string identifier, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<MemberDeclarationSyntax> members, int level) =>
            ClassDeclaration(identifier, default, modifiers, members, default, default, default, level);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ClassDeclarationSyntax ClassDeclaration(string identifier, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<MemberDeclarationSyntax> members, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, IReadOnlyCollection<BaseTypeSyntax> bases, int level)
        {
            var classSyntax = SyntaxFactory.Token(SyntaxKind.ClassKeyword);
            GetTypeValues(identifier, attributes, modifiers, members, typeParameterConstraintClauses, typeParameters, bases, level, out var attributesList, out var name, out var modifiersList, out var baseList, out var membersList, out var typeParameterList, out var typeParameterConstraintList, out var openingBrace, out var closingBrace);

            return SyntaxFactory.ClassDeclaration(attributesList, modifiersList, classSyntax, name, typeParameterList, baseList, typeParameterConstraintList, openingBrace, membersList, closingBrace, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static InterfaceDeclarationSyntax InterfaceDeclaration(string identifier, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<MemberDeclarationSyntax> members, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, IReadOnlyCollection<BaseTypeSyntax> bases, int level)
        {
            GetTypeValues(identifier, attributes, modifiers, members, typeParameterConstraintClauses, typeParameters, bases, level, out var attributesList, out var name, out var modifiersList, out var baseList, out var membersList, out var typeParameterList, out var typeParameterConstraintList, out var openingBrace, out var closingBrace);

            var typeIdentifier = SyntaxFactory.Token(SyntaxKind.InterfaceKeyword);

            return SyntaxFactory.InterfaceDeclaration(attributesList, modifiersList, typeIdentifier, name, typeParameterList, baseList, typeParameterConstraintList, openingBrace, membersList, closingBrace, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StructDeclarationSyntax StructDeclaration(string identifier, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<MemberDeclarationSyntax> members, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, IReadOnlyCollection<BaseTypeSyntax> bases, int level)
        {
            GetTypeValues(identifier, attributes, modifiers, members, typeParameterConstraintClauses, typeParameters, bases, level, out var attributesList, out var name, out var modifiersList, out var baseList, out var membersList, out var typeParameterList, out var typeParameterConstraintList, out var openingBrace, out var closingBrace);
            var typeIdentifier = SyntaxFactory.Token(SyntaxKind.StructKeyword);

            return SyntaxFactory.StructDeclaration(attributesList, modifiersList, typeIdentifier, name, typeParameterList, baseList, typeParameterConstraintList, openingBrace, membersList, closingBrace, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxTokenList TokenList(IReadOnlyCollection<SyntaxKind> tokens)
        {
            if (tokens == null || tokens.Count == 0)
            {
                return default;
            }

            var items = new List<SyntaxToken>(tokens.Count);

            foreach (var token in tokens)
            {
                items.Add(SyntaxFactory.Token(token).AddTrialingSpaces());
            }

            return SyntaxFactory.TokenList(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxTokenList TokenList(IReadOnlyCollection<SyntaxKind> tokens, int level)
        {
            if (tokens == null || tokens.Count == 0)
            {
                return default;
            }

            var items = new List<SyntaxToken>(tokens.Count);

            var i = 0;

            foreach (var token in tokens)
            {
                items.Add(i == 0 ? SyntaxFactory.Token(token).AddLeadingSpaces(level * LeadingSpacesPerLevel).AddTrialingSpaces() : SyntaxFactory.Token(token).AddTrialingSpaces());

                i++;
            }

            return SyntaxFactory.TokenList(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxList<TNode> List<TNode>()
            where TNode : SyntaxNode => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArgumentSyntax Argument(string argumentName) => SyntaxFactory.Argument(IdentifierName(argumentName));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArgumentSyntax Argument(string argumentName, SyntaxToken refKind) => SyntaxFactory.Argument(null, refKind, IdentifierName(argumentName));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArgumentSyntax Argument(string argumentName, string nameColon) => SyntaxFactory.Argument(NameColon(nameColon), default, IdentifierName(argumentName));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArgumentSyntax Argument(ExpressionSyntax expression) => SyntaxFactory.Argument(expression);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArgumentSyntax ArgumentThis() => SyntaxFactory.Argument(SyntaxFactory.ThisExpression());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NameColonSyntax NameColon(string name) => SyntaxFactory.NameColon(IdentifierName(name), SyntaxFactory.Token(SyntaxKind.ColonToken).AddTrialingSpaces());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ObjectCreationExpressionSyntax ObjectCreationExpression(string typeName, IReadOnlyCollection<ArgumentSyntax> arguments, InitializerExpressionSyntax initializer)
        {
            var argumentList = ArgumentList(arguments);
            return SyntaxFactory.ObjectCreationExpression(Token(SyntaxKind.NewKeyword).AddTrialingSpaces(), IdentifierName(typeName), argumentList, initializer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ObjectCreationExpressionSyntax ObjectCreationExpression(string typeName, IReadOnlyCollection<ArgumentSyntax> arguments)
        {
            var argumentList = ArgumentList(arguments);
            return SyntaxFactory.ObjectCreationExpression(Token(SyntaxKind.NewKeyword).AddTrialingSpaces(), IdentifierName(typeName), argumentList, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ObjectCreationExpressionSyntax ObjectCreationExpression(TypeSyntax type, IReadOnlyCollection<ArgumentSyntax> arguments, InitializerExpressionSyntax initializer)
        {
            var argumentList = ArgumentList(arguments);
            return SyntaxFactory.ObjectCreationExpression(Token(SyntaxKind.NewKeyword).AddTrialingSpaces(), type, argumentList, initializer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ObjectCreationExpressionSyntax ObjectCreationExpression(TypeSyntax type) => SyntaxFactory.ObjectCreationExpression(Token(SyntaxKind.NewKeyword).AddTrialingSpaces(), type, default, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static InitializerExpressionSyntax InitializerExpression(SyntaxKind syntaxKind, IReadOnlyCollection<ExpressionSyntax> expressions)
        {
            var (openBrace, closeBrace) = GetBraces();

            var list = SeparatedList<ExpressionSyntax>(expressions);

            return SyntaxFactory.InitializerExpression(syntaxKind, openBrace, list, closeBrace);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PredefinedTypeSyntax Void() => SyntaxFactory.PredefinedType(Token(SyntaxKind.VoidKeyword));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LiteralExpressionSyntax NullLiteral() => SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxList<TNode> List<TNode>(IReadOnlyCollection<TNode> nodes)
            where TNode : SyntaxNode => nodes == null || nodes.Count == 0 ? default : SyntaxFactory.List(nodes);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RefTypeSyntax RefType(TypeSyntax type, bool isReadOnly)
        {
            var readOnlySyntax = isReadOnly ? SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword).AddTrialingSpaces() : SyntaxFactory.Token(SyntaxKind.None);
            return SyntaxFactory.RefType(SyntaxFactory.Token(SyntaxKind.RefKeyword).AddTrialingSpaces(), readOnlySyntax, type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointerTypeSyntax PointerType(TypeSyntax type) => SyntaxFactory.PointerType(type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TupleExpressionSyntax TupleExpression(IReadOnlyCollection<ArgumentSyntax> arguments)
        {
            var argumentLists = SeparatedList(arguments);
            var (openBracket, closeBracket) = GetParentheses();
            return SyntaxFactory.TupleExpression(openBracket, argumentLists, closeBracket);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayTypeSyntax ArrayType(TypeSyntax elementType, IReadOnlyCollection<ArrayRankSpecifierSyntax> rankSpecifiers)
        {
            var rank = rankSpecifiers == null || rankSpecifiers.Count == 0 ? List(new[] { SyntaxFactory.ArrayRankSpecifier() }) : List(rankSpecifiers);
            return SyntaxFactory.ArrayType(elementType, rank);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayRankSpecifierSyntax ArrayRankSpecifier(OmittedArraySizeExpressionSyntax rank) => SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(rank));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayRankSpecifierSyntax ArrayRankSpecifier(IReadOnlyCollection<int?> sizes)
        {
            var sizeSpecifier = sizes.Select(x => x == null ? (ExpressionSyntax)SyntaxFactory.OmittedArraySizeExpression() : SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(x.Value))).ToList();
            return SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SeparatedList(sizeSpecifier));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConstructorConstraintSyntax ConstructorConstraint() => SyntaxFactory.ConstructorConstraint();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ClassOrStructConstraintSyntax ClassOrStructConstraint(SyntaxKind syntaxKind) => SyntaxFactory.ClassOrStructConstraint(syntaxKind);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeConstraintSyntax TypeConstraint(TypeSyntax typeName) => SyntaxFactory.TypeConstraint(typeName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(string name, IReadOnlyCollection<TypeParameterConstraintSyntax> constraints)
        {
            var constraintsList = SeparatedList(constraints);
            return SyntaxFactory.TypeParameterConstraintClause(SyntaxFactory.Token(SyntaxKind.WhereKeyword).AddLeadingSpaces().AddTrialingSpaces(), SyntaxFactory.IdentifierName(name), SyntaxFactory.Token(SyntaxKind.ColonToken).AddLeadingSpaces().AddTrialingSpaces(), constraintsList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeParameterListSyntax TypeParameterList(IReadOnlyCollection<TypeParameterSyntax> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return default;
            }

            return SyntaxFactory.TypeParameterList(SeparatedList(parameters));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BinaryExpressionSyntax BinaryExpression(SyntaxKind kind, ExpressionSyntax left, ExpressionSyntax right) => SyntaxFactory.BinaryExpression(kind, left.AddTrialingSpaces(), right.AddLeadingSpaces());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BinaryExpressionSyntax BinaryExpression(SyntaxKind kind, string left, ExpressionSyntax right) => BinaryExpression(kind, IdentifierName(left), right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberAccessExpressionSyntax MemberAccessExpression(SyntaxKind kind, ExpressionSyntax expression, SimpleNameSyntax name) => SyntaxFactory.MemberAccessExpression(kind, expression, name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberAccessExpressionSyntax MemberAccessExpression(SyntaxKind kind, ExpressionSyntax expression, string name) => SyntaxFactory.MemberAccessExpression(kind, expression, IdentifierName(name));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberAccessExpressionSyntax MemberAccessExpression(SyntaxKind kind, string expression, string name) => SyntaxFactory.MemberAccessExpression(kind, IdentifierName(expression), IdentifierName(name));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayCreationExpressionSyntax ArrayCreationExpression(ArrayTypeSyntax type) => SyntaxFactory.ArrayCreationExpression(SyntaxFactory.Token(SyntaxKind.NewKeyword).AddTrialingSpaces(), type, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayCreationExpressionSyntax ArrayCreationExpression(ArrayTypeSyntax type, InitializerExpressionSyntax initializer) => SyntaxFactory.ArrayCreationExpression(SyntaxFactory.Token(SyntaxKind.NewKeyword).AddTrialingSpaces(), type, initializer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalDeclarationStatementSyntax LocalDeclarationStatement(VariableDeclarationSyntax declaration) => SyntaxFactory.LocalDeclarationStatement(default, default, default, default, declaration, Token(SyntaxKind.SemicolonToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LiteralExpressionSyntax LiteralExpression(SyntaxKind syntaxKind) => SyntaxFactory.LiteralExpression(syntaxKind);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LiteralExpressionSyntax LiteralExpression(SyntaxKind syntaxKind, SyntaxToken token) => SyntaxFactory.LiteralExpression(syntaxKind, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LiteralExpressionSyntax LiteralExpression(string value) => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LiteralExpressionSyntax LiteralExpression(int value) => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(ulong value) => SyntaxFactory.Literal(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(uint value) => SyntaxFactory.Literal(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(int value) => SyntaxFactory.Literal(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(long value) => SyntaxFactory.Literal(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(char value) => SyntaxFactory.Literal(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(double value) => SyntaxFactory.Literal(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(float value) => SyntaxFactory.Literal(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(string value) => SyntaxFactory.Literal(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeOfExpressionSyntax TypeOfExpression(TypeSyntax type) => SyntaxFactory.TypeOfExpression(SyntaxFactory.Token(SyntaxKind.TypeOfKeyword), SyntaxFactory.Token(SyntaxKind.OpenParenToken), type, SyntaxFactory.Token(SyntaxKind.CloseParenToken));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier(string name) => SyntaxFactory.ExplicitInterfaceSpecifier(IdentifierName(name));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TupleTypeSyntax TupleType(IReadOnlyCollection<TupleElementSyntax> elements) => SyntaxFactory.TupleType(SeparatedList(elements));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TupleElementSyntax TupleElement(TypeSyntax type, string name)
        {
            var identifier = string.IsNullOrWhiteSpace(name) ? default : SyntaxFactory.Identifier(name);
            type = identifier == default ? type : type.AddTrialingSpaces();
            return SyntaxFactory.TupleElement(type, identifier);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NullableTypeSyntax NullableType(TypeSyntax typeSyntax) => SyntaxFactory.NullableType(typeSyntax);

        private static IReadOnlyCollection<T> GetIndentedNodes<T>(IReadOnlyCollection<T> modifiers, int level, bool lastNodeTrailingLine = false)
            where T : SyntaxNode
        {
            if (modifiers is null)
            {
                return Array.Empty<T>();
            }

            var items = new List<T>(modifiers.Count);

            var i = 0;
            foreach (var modifier in modifiers)
            {
                var addModifier = modifier.AddLeadingNewLinesAndSpaces(1, level * LeadingSpacesPerLevel);
                if (i == modifiers.Count - 1 && lastNodeTrailingLine)
                {
                    addModifier = addModifier.AddTrialingNewLines();
                }

                items.Add(addModifier);

                i++;
            }

            return items;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (SyntaxToken OpeningBrace, SyntaxToken ClosingBrace) GetBraces()
        {
            var openingBrace = SyntaxFactory.Token(SyntaxKind.OpenBraceToken).AddTrialingSpaces();
            var closingBrace = SyntaxFactory.Token(SyntaxKind.CloseBraceToken).AddTrialingSpaces();

            return (openingBrace, closingBrace);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (SyntaxToken OpeningBrace, SyntaxToken ClosingBrace) GetBraces(int level)
        {
            var openingBrace = SyntaxFactory.Token(SyntaxKind.OpenBraceToken).AddLeadingNewLinesAndSpaces(1, level * LeadingSpacesPerLevel);
            var closingBrace = SyntaxFactory.Token(SyntaxKind.CloseBraceToken).AddLeadingNewLinesAndSpaces(1, level * LeadingSpacesPerLevel);

            return (openingBrace, closingBrace);
        }

        private static (SyntaxToken OpenParentheses, SyntaxToken ClosedParentheses) GetParentheses()
        {
            var openingBrace = SyntaxFactory.Token(SyntaxKind.OpenParenToken).AddTrialingSpaces();
            var closingBrace = SyntaxFactory.Token(SyntaxKind.CloseParenToken).AddTrialingSpaces();

            return (openingBrace, closingBrace);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (SyntaxToken OpeningBracket, SyntaxToken ClosingBracket) GetBrackets(int level)
        {
            var openingBrace = SyntaxFactory.Token(SyntaxKind.OpenBracketToken).AddLeadingNewLinesAndSpaces(1, level * LeadingSpacesPerLevel);
            var closingBrace = SyntaxFactory.Token(SyntaxKind.CloseBracketToken).AddLeadingNewLinesAndSpaces(1, level * LeadingSpacesPerLevel);

            return (openingBrace, closingBrace);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (SyntaxToken OpeningBracket, SyntaxToken ClosingBracket) GetBrackets()
        {
            var openingBrace = SyntaxFactory.Token(SyntaxKind.OpenBracketToken).AddTrialingSpaces();
            var closingBrace = SyntaxFactory.Token(SyntaxKind.CloseBracketToken).AddLeadingSpaces();

            return (openingBrace, closingBrace);
        }

        private static IReadOnlyCollection<AccessorDeclarationSyntax> GetSpacedAccessors(IReadOnlyCollection<AccessorDeclarationSyntax> accessors)
        {
            var i = 0;

            var items = new List<AccessorDeclarationSyntax>(accessors.Count);

            foreach (var accessor in accessors)
            {
                var returnValue = accessor;
                if (i != accessors.Count - 1)
                {
                    returnValue = returnValue.AddTrialingSpaces();
                }

                items.Add(returnValue);
                i++;
            }

            return items;
        }

        private static void GetTypeValues(string identifier, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<MemberDeclarationSyntax> members, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, IReadOnlyCollection<BaseTypeSyntax> bases, int level, out SyntaxList<AttributeListSyntax> attributeList, out SyntaxToken name, out SyntaxTokenList modifiersList, out BaseListSyntax baseList, out SyntaxList<MemberDeclarationSyntax> membersList, out TypeParameterListSyntax typeParameterList, out SyntaxList<TypeParameterConstraintClauseSyntax> typeParameterConstraintList, out SyntaxToken openingBrace, out SyntaxToken closingBrace)
        {
            attributeList = List(GetIndentedNodes(attributes, level, true));
            name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            modifiersList = attributes?.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            baseList = BaseList(bases);
            membersList = members.Count > 0 ? List(GetIndentedNodes(members, level + 1)) : default;
            typeParameterList = TypeParameterList(typeParameters);
            typeParameterConstraintList = typeParameterConstraintClauses?.Count > 0 ? List(typeParameterConstraintClauses) : default;
            (openingBrace, closingBrace) = GetBraces(level);
        }
    }
}
