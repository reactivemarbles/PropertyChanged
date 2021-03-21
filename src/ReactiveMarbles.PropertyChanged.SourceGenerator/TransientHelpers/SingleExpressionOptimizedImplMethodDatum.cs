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
    internal sealed record SingleExpressionOptimizedImplMethodDatum : MethodDatum
    {
        public SingleExpressionOptimizedImplMethodDatum(string inputType, string outputType, Accessibility accessModifier, List<(string Name, string InputType, string OutputType)> members)
        {
            InputTypeName = inputType;
            OutputTypeName = outputType;
            AccessModifier = accessModifier;
            Members = members;
        }

        public string InputTypeName { get; }

        public string OutputTypeName { get; }

        public Accessibility AccessModifier { get; }

        public List<(string Name, string InputType, string OutputType)> Members { get; }
    }
}
