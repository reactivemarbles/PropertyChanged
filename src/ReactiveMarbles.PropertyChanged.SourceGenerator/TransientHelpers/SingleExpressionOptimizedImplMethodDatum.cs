// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    /// <summary>
    /// Single expression with a particular input/output type so it can return the chain directly without needing a dictionary.
    /// </summary>
    internal sealed record SingleExpressionOptimizedImplMethodDatum(string InputTypeName, string OutputTypeName, Accessibility AccessModifier, List<ExpressionChain> Members) : MethodDatum;
}
