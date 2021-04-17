﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
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
        private static readonly IExtractor[] _extractors = new IExtractor[]
        {
            new WhenChangedExtractor(),
            new BindExtractor()
        };

        private static readonly BindGenerator _bindGenerator = new BindGenerator();
        private readonly WhenChangedGenerator _whenChangedGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="Generator"/> class.
        /// </summary>
        public Generator()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Generator"/> class.
        /// </summary>
        /// <param name="useRoslyn">If we should use Roslyn.</param>
        public Generator(bool useRoslyn)
        {
            _whenChangedGenerator = new WhenChangedGenerator(useRoslyn);
        }

        /// <inheritdoc/>
        public void Initialize(GeneratorInitializationContext context) => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        /// <inheritdoc/>
        public void Execute(GeneratorExecutionContext context)
        {
            var options = (context.Compilation as CSharpCompilation)?.SyntaxTrees[0].Options as CSharpParseOptions;
            var compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(Constants.WhenChangedSource, options), CSharpSyntaxTree.ParseText(Constants.BindSource, options));
            context.AddSource("WhenChanged.Stubs.g.cs", Constants.WhenChangedSource);
            context.AddSource("Binding.Stubs.g.cs", Constants.BindSource);

            if (context.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
            {
                return;
            }

            var bindInvocations = new SortedList<ITypeSymbol, HashSet<InvocationInfo>>(TypeSymbolComparer.Default);
            var whenChangedInvocations = new SortedList<ITypeSymbol, HashSet<InvocationInfo>>(TypeSymbolComparer.Default);

            foreach (var extractor in _extractors)
            {
                foreach (var item in extractor.GetInvocations(context, compilation, syntaxReceiver))
                {
                    if (item is WhenChangedMultiMethodInvocationInfo whenChangedMulti)
                    {
                        whenChangedInvocations.ListInsert(item.Type, whenChangedMulti);
                    }

                    if (item is WhenChangedExpressionInvocationInfo whenChangedExpression)
                    {
                        whenChangedInvocations.ListInsert(item.Type, whenChangedExpression);
                    }

                    if (item is BindInvocationInfo bindInfo)
                    {
                        bindInvocations.ListInsert(item.Type, bindInfo);
                    }
                }
            }

            foreach (var kvp in whenChangedInvocations)
            {
                foreach (var (fileName, source) in _whenChangedGenerator.GenerateSourceFromInvocations(kvp.Key, kvp.Value))
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
