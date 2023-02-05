// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#pragma warning disable SA1313 // Use lower case -- Not working for record structs.

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;

internal sealed record WhenStatementsDatum(ExpressionArgument Argument)
{
    /// <inheritdoc/>
    public override int GetHashCode() => Argument.GetHashCode();

    public bool Equals(WhenStatementsDatum? other) => other?.Argument.Equals(Argument) ?? false;
}
