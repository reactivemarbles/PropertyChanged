// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Helpers;
using ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators;
using ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;

using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator;

/// <summary>
/// The main source generator.
/// </summary>
[Generator]
public class Generator : ISourceGenerator
{
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
                CSharpSyntaxTree.ParseText(Constants.WhenExtensionClassSource, options),
                CSharpSyntaxTree.ParseText(Constants.PreserveAttribute, options),
                CSharpSyntaxTree.ParseText(Constants.BindExtensionClassSource, options));
            context.AddSource("PropertyChanged.SourceGenerator.When.Stubs.g.cs", SourceText.From(Constants.WhenExtensionClassSource, Encoding.UTF8));

            // TODO: Fix this.
            ////context.AddSource("PropertyChanged.SourceGenerator.PreserveAttribute.g.cs", SourceText.From(Constants.PreserveAttribute, Encoding.UTF8));
            context.AddSource("PropertyChanged.SourceGenerator.Binding.Stubs.g.cs", SourceText.From(Constants.BindExtensionClassSource, Encoding.UTF8));

            if (context.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
            {
                return;
            }

            WriteMembers(syntaxReceiver, compilation, context);
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

    private static void WriteMembers(SyntaxReceiver syntaxReceiver, CSharpCompilation compilation, in GeneratorExecutionContext context)
    {
        var compilationData = MethodCreator.Generate(syntaxReceiver, compilation, context);

        foreach (var namespaceDatum in compilationData.GetPartials())
        {
            var source = GenerateNamespaceSource(namespaceDatum, false);
            var namespaceName = string.IsNullOrWhiteSpace(namespaceDatum.NamespaceName) ? "global" : namespaceDatum.NamespaceName;
            context.AddSource("PropertyChanged.SourceGenerator.Partials." + namespaceName + ".cs", SourceText.From(source, Encoding.UTF8));
        }

        var extensionClasses = compilationData.GetExtensionClasses().ToList();

        context.AddSource("PropertyChanged.SourceGenerator.Extensions.cs", SourceText.From(GenerateClassesSource(extensionClasses, true), Encoding.UTF8));
    }

    private static string GenerateNamespaceSource(NamespaceDatum namespaceEntry, bool isExtension)
    {
        var members = GenerateNamespaces(namespaceEntry, isExtension);
        var sb = new StringBuilder(Constants.WarningDisable);

        var compilationUnit = CompilationUnit(Array.Empty<AttributeListSyntax>(), members, Array.Empty<UsingDirectiveSyntax>());

        sb.AppendLine(compilationUnit.ToFullString());

        return sb.ToString();
    }

    private static string GenerateClassesSource(IReadOnlyCollection<ClassDatum> classes, bool isExtension)
    {
        var members = classes.Select(classEntry => GenerateClass(classEntry, isExtension)).Cast<MemberDeclarationSyntax>().ToList();

        var sb = new StringBuilder(Constants.WarningDisable);

        var compilationUnit = CompilationUnit(Array.Empty<AttributeListSyntax>(), members, Array.Empty<UsingDirectiveSyntax>());

        sb.AppendLine(compilationUnit.ToFullString());

        return sb.ToString();
    }

    private static List<MemberDeclarationSyntax> GenerateNamespaces(NamespaceDatum namespaceEntry, bool isExtension)
    {
        var members = new List<MemberDeclarationSyntax>();

        var globalNamespace = namespaceEntry.IsGlobal;

        var classDeclarations = !globalNamespace ? new() : members;
        members.AddRange(namespaceEntry.Classes.Select(classEntry => GenerateClass(classEntry, isExtension)).Cast<MemberDeclarationSyntax>());

        if (!globalNamespace)
        {
            var namespaceDeclaration = NamespaceDeclaration(namespaceEntry.NamespaceName, classDeclarations, false);
            members.Add(namespaceDeclaration);
        }

        return members;
    }

    private static ClassDeclarationSyntax GenerateClass(ClassDatum classEntry, bool isExtension)
    {
        var accessibility = isExtension ?
            new[] { SyntaxKind.InternalKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword } :
            classEntry.AccessibilityModifier.GetAccessibilityTokens().Concat(new[] { SyntaxKind.PartialKeyword }).ToArray();

        var className = classEntry.ClassName;
        var methods = classEntry.MethodData;

        var currentClass = ClassDeclaration(className, Array.Empty<AttributeListSyntax>(), accessibility, methods.Select(x => x.Expression).ToList(), 1);

        foreach (var ancestor in classEntry.Ancestors)
        {
            accessibility = ancestor.AccessibilityModifier.GetAccessibilityTokens().Concat(new[] { SyntaxKind.PartialKeyword }).ToArray();
            currentClass = ClassDeclaration(ancestor.ClassName, accessibility, new[] { currentClass }, 0);
        }

        return currentClass;
    }
}
