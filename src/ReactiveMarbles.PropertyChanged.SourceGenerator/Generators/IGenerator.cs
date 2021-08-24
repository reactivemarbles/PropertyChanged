// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    /// <summary>
    /// Generates source code from InvocationInfo data.
    /// </summary>
    internal interface IGenerator
    {
        /// <summary>
        /// Generates the source from invocations.
        /// </summary>
        /// <param name="type">The types.</param>
        /// <param name="invocations">The invocations.</param>
        /// <returns>The source details.</returns>
        IEnumerable<(string FileName, string SourceCode)> GenerateSourceFromInvocations(ITypeSymbol type, HashSet<TypeDatum> invocations);
    }
}
