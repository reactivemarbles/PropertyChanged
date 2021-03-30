// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Builders
{
    /// <summary>
    /// The type of invocation.
    /// </summary>
    public enum InvocationKind
    {
        /// <summary>
        /// The invocation is a member access.
        /// </summary>
        MemberAccess,

        /// <summary>
        /// The invocation is explicit.
        /// </summary>
        Explicit,
    }
}
