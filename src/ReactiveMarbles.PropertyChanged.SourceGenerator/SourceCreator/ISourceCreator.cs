// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    /// <summary>
    /// Represents a contract for an object that creates source code for WhenChanged extensions.
    /// </summary>
    internal interface ISourceCreator
    {
        /// <summary>
        /// Creates the source for a multi-expression method.
        /// </summary>
        /// <param name="methodDatum">The method datum.</param>
        /// <returns>The source code.</returns>
        string Create(MultiExpressionMethodDatum methodDatum);

        /// <summary>
        /// Creates the source for a single expression method with a dictionary implementation.
        /// </summary>
        /// <param name="methodDatum">The method datum.</param>
        /// <returns>The source code.</returns>
        string Create(SingleExpressionDictionaryImplMethodDatum methodDatum);

        /// <summary>
        /// Creates the source for a single expression method with a 'direct return' implementation.
        /// </summary>
        /// <param name="methodDatum">The method datum.</param>
        /// <returns>The source code.</returns>
        string Create(SingleExpressionOptimizedImplMethodDatum methodDatum);
    }
}