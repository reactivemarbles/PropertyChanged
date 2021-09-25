// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators;
using ReactiveMarbles.PropertyChanged.SourceGenerator.RoslynHelpers;

using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    /// <summary>
    /// The main source generator.
    /// </summary>
    [Generator]
    public class Generator : ISourceGenerator
    {
        ////private static readonly WhenChangedExtractor _whenChangedExtractor = new("NotifyPropertyChangedExtensions", syntaxReceiver => syntaxReceiver.WhenChanged);
        ////private static readonly WhenChangedExtractor _whenChangingExtractor = new("NotifyPropertyChangingExtensions", syntaxReceiver => syntaxReceiver.WhenChanging);
        ////private static readonly BindTwoWayExtractor _bindTwoWayExtractor = new();
        ////private static readonly BindTwoWayExtractor _bindOneWayExtractor = new();

        ////private static readonly WhenChangedGenerator _whenChangedGenerator = WhenChangedGenerator.WhenChanged();
        ////private static readonly WhenChangedGenerator _whenChangingGenerator = WhenChangedGenerator.WhenChanging();
        ////private static readonly BindGenerator _bindTwoWayGenerator = new();

        private readonly MethodCreator _methodCreator = new MethodCreator();

        /// <inheritdoc/>
        public void Initialize(GeneratorInitializationContext context) => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        /// <inheritdoc/>
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var compilation = (CSharpCompilation)context.Compilation;
                var options = compilation.SyntaxTrees[0].Options as CSharpParseOptions;
                compilation = compilation.AddSyntaxTrees(
                    CSharpSyntaxTree.ParseText(Constants.WhenChangingSource, options),
                    CSharpSyntaxTree.ParseText(Constants.WhenChangedSource, options),
                    CSharpSyntaxTree.ParseText(Constants.BindSource, options));
                context.AddSource($"PropertyChanged.SourceGenerator.{Constants.WhenChangingMethodName}.Stubs.g.cs", SourceText.From(Constants.WhenChangingSource, Encoding.UTF8));
                context.AddSource($"PropertyChanged.SourceGenerator.{Constants.WhenChangedMethodName}.Stubs.g.cs", SourceText.From(Constants.WhenChangedSource, Encoding.UTF8));
                context.AddSource("PropertyChanged.SourceGenerator.PreserveAttribute.g.cs", SourceText.From(Constants.PreserveAttribute, Encoding.UTF8));
                context.AddSource("PropertyChanged.SourceGenerator.Binding.Stubs.g.cs", SourceText.From(Constants.BindSource, Encoding.UTF8));

                if (context.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
                {
                    return;
                }

                WriteMembers(_methodCreator, syntaxReceiver, compilation, context);
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(new DiagnosticDescriptor(
                  "RXMERR",
                  "Error has happened",
                  $"Exception {ex}",
                  "Compiler",
                  DiagnosticSeverity.Error,
                  true));
            }
        }

        private static void WriteMembers(IMethodCreator methodSourceGenerator, SyntaxReceiver syntaxReceiver, CSharpCompilation compilation, in GeneratorExecutionContext context)
        {
            var (extensions, partials) = methodSourceGenerator.Generate(syntaxReceiver, compilation, context);

            WriteMembers(extensions, true, context);
            WriteMembers(partials, false, context);
        }

        private static void WriteMembers(HashSet<MethodDatum> methods, bool isExtension, in GeneratorExecutionContext context)
        {
            if (methods.Count == 0)
            {
                return;
            }

            foreach (var memberGrouping in methods.GroupBy(x => x.SemanticModel.SyntaxTree.FilePath))
            {
                var filePath = memberGrouping.Key;
                var members = GenerateMembers(memberGrouping.ToList(), isExtension);
                var sb = new StringBuilder(Constants.WarningDisable);

                var compilationUnit = CompilationUnit(Array.Empty<AttributeListSyntax>(), members, Array.Empty<UsingDirectiveSyntax>());

                sb.AppendLine(compilationUnit.ToFullString());

                var mode = isExtension ? "Extensions" : "Partials";

                context.AddSource("PropertyChanged.SourceGenerator." + mode + '.' + Path.GetFileName(filePath), SourceText.From(sb.ToString(), Encoding.UTF8));
            }
        }

        private static List<MemberDeclarationSyntax> GenerateMembers(List<MethodDatum> elements, bool isExtension)
        {
            if (elements.Count == 0)
            {
                return new List<MemberDeclarationSyntax>();
            }

            var members = new List<MemberDeclarationSyntax>(elements.Count);
            if (isExtension)
            {
                members.Add(GenerateExtensionClass(elements.Select(x => x.Expression).Where(x => x is not null)));
            }
            else
            {
                foreach (var namespaceDeclaration in GeneratePartialClasses(elements))
                {
                    members.Add(namespaceDeclaration);
                }
            }

            return members;
        }

        private static ClassDeclarationSyntax GenerateExtensionClass(IEnumerable<MethodDeclarationSyntax> methods)
        {
            if (methods is null)
            {
                throw new ArgumentNullException(nameof(methods));
            }

            return ClassDeclaration(Constants.BindExtensionClass, Array.Empty<AttributeListSyntax>(), new[] { SyntaxKind.InternalKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword }, methods.ToList(), 0);
        }

        private static IEnumerable<NamespaceDeclarationSyntax> GeneratePartialClasses(IEnumerable<MethodDatum> methods)
        {
            foreach (var methodNamespaceGrouping in methods.GroupBy(x => x.NamespaceName))
            {
                var classDeclarations = new List<ClassDeclarationSyntax>();
                foreach (var methodClassGrouping in methodNamespaceGrouping.GroupBy(x => x.ClassName))
                {
                    var className = methodClassGrouping.Key;

                    foreach (var classGrouping in methodClassGrouping)
                    {
                        var accessibility = classGrouping.AccessibilityModifier.GetAccessibilityTokens().Concat(new[] { SyntaxKind.PartialKeyword }).ToArray();
                        ClassDeclaration(className, Array.Empty<AttributeListSyntax>(), accessibility, methods.Cast<MemberDeclarationSyntax>().ToList(), 1);
                    }
                }
            }

            return Array.Empty<NamespaceDeclarationSyntax>();
        }

        private static IReadOnlyList<AttributeListSyntax> GetClassHeaderAttributes() =>
            new[]
            {
                AttributeList(Attribute("global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute")),
                AttributeList(Attribute("global::System.Diagnostics.DebuggerNonUserCodeAttribute")),
                AttributeList(Attribute("PreserveAttribute", new[] { AttributeArgument(NameEquals(IdentifierName("AllMembers")), LiteralExpression(SyntaxKind.TrueLiteralExpression)) })),
                AttributeList(Attribute("global::System.Reflection.ObfuscationAttribute", new[] { AttributeArgument(NameEquals(IdentifierName("Exclude")), LiteralExpression(SyntaxKind.TrueLiteralExpression)) })),
                AttributeList(Attribute("global::System.ComponentModel.EditorBrowsableAttribute", new[] { AttributeArgument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "global::System.ComponentModel.EditorBrowsableState", "Never")) })),
            };

        private static void GenerateMethodGroups(IEnumerable<InvocationExpressionSyntax> invocations, CSharpCompilation compilation, Dictionary<SemanticModel, MethodGrouping> models, Action<MethodGrouping, IEnumerable<InvocationExpressionSyntax>> addItems)
        {
            foreach (var invocationGrouping in invocations.GroupBy(x => x.SyntaxTree))
            {
                var model = compilation.GetSemanticModel(invocationGrouping.Key);

                if (!models.TryGetValue(model, out var methodGrouping))
                {
                    methodGrouping = new MethodGrouping(model);
                    models[model] = methodGrouping;
                }

                addItems.Invoke(methodGrouping, invocationGrouping);
            }
        }
    }
}
