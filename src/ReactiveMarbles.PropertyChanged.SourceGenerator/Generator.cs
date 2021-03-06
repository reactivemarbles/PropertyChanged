// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
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
        /// <inheritdoc/>
        public void Initialize(GeneratorInitializationContext context) => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        /// <inheritdoc/>
        public void Execute(GeneratorExecutionContext context)
        {
            var options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
            var whenChangedStub = SourceText.From(StringBuilderSourceCreatorHelper.GetWhenChangedStubClass(), Encoding.UTF8);
            var bindingStub = SourceText.From(StringBuilderSourceCreatorHelper.GetBindingStubClass(), Encoding.UTF8);
            var compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(whenChangedStub, options), CSharpSyntaxTree.ParseText(bindingStub, options));
            context.AddSource("WhenChanged.Stubs.g.cs", whenChangedStub);
            context.AddSource("Binding.Stubs.g.cs", bindingStub);

            if (context.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
            {
                return;
            }

            WhenChangedGenerator.GenerateWhenChanged(context, compilation, syntaxReceiver);
        }
    }
}
