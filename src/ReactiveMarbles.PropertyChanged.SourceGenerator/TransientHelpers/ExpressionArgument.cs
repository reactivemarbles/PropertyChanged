﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal sealed record ExpressionArgument(string LambdaBodyString, List<(string Name, string InputType, string outputType)> ExpressionChain, ITypeSymbol InputType, ITypeSymbol OutputType, bool ContainsPrivateOrProtectedMember)
    {
        public bool Equals(ExpressionArgument other)
        {
            if (other is null)
            {
                return false;
            }

            return
                EqualityComparer<string>.Default.Equals(LambdaBodyString, other.LambdaBodyString) &&
                SymbolEqualityComparer.Default.Equals(InputType, other.InputType) &&
                SymbolEqualityComparer.Default.Equals(OutputType, other.OutputType);
        }

        public override int GetHashCode()
        {
            var hashCode = 1230885993;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(LambdaBodyString);
            hashCode = (hashCode * -1521134295) + SymbolEqualityComparer.Default.GetHashCode(InputType);
            hashCode = (hashCode * -1521134295) + SymbolEqualityComparer.Default.GetHashCode(OutputType);

            return hashCode;
        }
    }
}
