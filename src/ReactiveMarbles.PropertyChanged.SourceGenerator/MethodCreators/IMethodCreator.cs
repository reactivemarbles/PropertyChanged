// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators
{
    /// <summary>
    /// Takes metadata about required methods and generates source code for them.
    /// </summary>
    internal interface IMethodCreator
    {
        /// <summary>
        /// Generates the code for the specified invocations.
        /// </summary>
        /// <param name="syntaxReceiver">The receiver to generate code for.</param>
        /// <param name="compilation">The compilation to generate code for.</param>
        /// <param name="context">The context from the source generator.</param>
        /// <returns>The method expression with metadata.</returns>
        (HashSet<MethodDatum> Extensions, HashSet<MethodDatum> Partials) Generate(SyntaxReceiver syntaxReceiver, CSharpCompilation compilation, GeneratorExecutionContext context);
    }
}
