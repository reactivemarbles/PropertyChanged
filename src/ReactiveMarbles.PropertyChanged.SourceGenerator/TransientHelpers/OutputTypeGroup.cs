// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal readonly struct OutputTypeGroup
    {
        public OutputTypeGroup(List<ExpressionArgument> expressionArguments)
        {
            ExpressionArguments = expressionArguments;
        }

        public List<ExpressionArgument> ExpressionArguments { get; }
    }
}
