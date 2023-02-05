// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

/// <summary>
/// Simplifies the source code creation of an empty class.
/// </summary>
public class EmptyClassBuilder : BaseUserSourceBuilder<EmptyClassBuilder>
{
    /// <inheritdoc/>
    protected override string CreateClass(string nestedClasses) =>
        $@"
    {ClassAccess.ToFriendlyString()} partial class {ClassName}
    {{
        {nestedClasses}
    }}
";
}