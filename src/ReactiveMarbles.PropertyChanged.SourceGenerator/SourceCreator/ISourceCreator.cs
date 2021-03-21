// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    /// <summary>
    /// Represents a contract for an object that creates source code for WhenChanged extensions.
    /// </summary>
    internal interface ISourceCreator
    {
        /// <summary>
        /// Creates the source code for the specified source datums.
        /// </summary>
        /// <param name="sourceDatums">The source datums.</param>
        /// <returns>The source.</returns>
        string Create(IEnumerable<IDatum> sourceDatums);
    }
}
