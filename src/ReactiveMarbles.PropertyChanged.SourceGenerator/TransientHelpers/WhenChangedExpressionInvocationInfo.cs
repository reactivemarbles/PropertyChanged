// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal sealed record WhenChangedExpressionInvocationInfo(
        ITypeSymbol Type,
        bool IsPublic,
        ExpressionArgument ExpressionArgument)
        : InvocationInfo(Type);
}
