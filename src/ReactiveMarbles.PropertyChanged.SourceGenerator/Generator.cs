// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    /// <summary>
    /// The main source generator.
    /// </summary>
    [Generator]
    public class Generator : ISourceGenerator
    {
        private static readonly WhenChangedExtractor _whenChangedExtractor = new("NotifyPropertyChangedExtensions", syntaxReceiver => syntaxReceiver.WhenChangedMethods);
        private static readonly WhenChangedExtractor _whenChangingExtractor = new("NotifyPropertyChangingExtensions", syntaxReceiver => syntaxReceiver.WhenChangingMethods);
        private static readonly BindTwoWayExtractor _bindExtractor = new();

        private static readonly WhenChangedGenerator _whenChangedGenerator = WhenChangedGenerator.WhenChanged();
        private static readonly WhenChangedGenerator _whenChangingGenerator = WhenChangedGenerator.WhenChanging();
        private static readonly BindGenerator _bindGenerator = new();

        /// <inheritdoc/>
        public void Initialize(GeneratorInitializationContext context) => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        /// <inheritdoc/>
        public void Execute(GeneratorExecutionContext context)
        {
            var options = (context.Compilation as CSharpCompilation)?.SyntaxTrees[0].Options as CSharpParseOptions;
            var compilation = context.Compilation.AddSyntaxTrees(
                CSharpSyntaxTree.ParseText(Constants.WhenChangingSource, options),
                CSharpSyntaxTree.ParseText(Constants.WhenChangedSource, options),
                CSharpSyntaxTree.ParseText(Constants.BindSource, options));
            context.AddSource($"{Constants.WhenChangingMethodName}.Stubs.g.cs", Constants.WhenChangingSource);
            context.AddSource($"{Constants.WhenChangedMethodName}.Stubs.g.cs", Constants.WhenChangedSource);
            context.AddSource("Binding.Stubs.g.cs", Constants.BindSource);

            if (context.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
            {
                return;
            }

            var bindInvocations = new SortedList<ITypeSymbol, HashSet<TypeDatum>>(TypeSymbolComparer.Default);
            var whenChangedInvocations = new SortedList<ITypeSymbol, HashSet<TypeDatum>>(TypeSymbolComparer.Default);
            var whenChangingInvocations = new SortedList<ITypeSymbol, HashSet<TypeDatum>>(TypeSymbolComparer.Default);

            foreach (var item in _whenChangedExtractor.GetInvocations(context, compilation, syntaxReceiver))
            {
                whenChangedInvocations.ListInsert(item.Type, item);
            }

            foreach (var item in _whenChangingExtractor.GetInvocations(context, compilation, syntaxReceiver))
            {
                whenChangingInvocations.ListInsert(item.Type, item);
            }

            foreach (var item in _bindExtractor.GetInvocations(context, compilation, syntaxReceiver))
            {
                bindInvocations.ListInsert(item.Type, item);
            }

            foreach (var kvp in whenChangedInvocations)
            {
                foreach (var (fileName, source) in _whenChangedGenerator.GenerateSourceFromInvocations(kvp.Key, kvp.Value))
                {
                    context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
                }
            }

            foreach (var kvp in whenChangingInvocations)
            {
                foreach (var (fileName, source) in _whenChangingGenerator.GenerateSourceFromInvocations(kvp.Key, kvp.Value))
                {
                    context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
                }
            }

            foreach (var kvp in bindInvocations)
            {
                foreach (var (fileName, source) in _bindGenerator.GenerateSourceFromInvocations(kvp.Key, kvp.Value))
                {
                    context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
                }
            }
        }
    }
}
