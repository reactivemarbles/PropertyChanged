// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    /// <summary>
    /// Extracts relevant information to be generated.
    /// </summary>
    internal interface IExtractor
    {
        /// <summary>
        /// Gets the information about invocations.
        /// </summary>
        /// <param name="context">The context which contains generator specific information.</param>
        /// <param name="compilation">The compilation of the source code.</param>
        /// <param name="syntaxReceiver">The receiver which will contain syntax nodes we are interested in.</param>
        /// <returns>The invocations.</returns>
        IEnumerable<TypeDatum> GetInvocations(GeneratorExecutionContext context, Compilation compilation, SyntaxReceiver syntaxReceiver);
    }
}
