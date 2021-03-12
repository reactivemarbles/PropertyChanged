// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal readonly struct OutputTypeGroup
    {
        public OutputTypeGroup(ITypeSymbol type, List<ExpressionArgument> expressionArguments)
        {
            ExpressionArguments = expressionArguments;
            Type = type;
        }

        public OutputTypeGroup(ITypeSymbol type)
        {
            ExpressionArguments = new List<ExpressionArgument>();
            Type = type;
        }

        public List<ExpressionArgument> ExpressionArguments { get; }

        public ITypeSymbol Type { get; }
    }
}
