// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Comparers
{
    internal class SyntaxNodeComparer : IComparer<SyntaxNode>, IEqualityComparer<SyntaxNode>
    {
        private SyntaxNodeComparer()
        {
        }

        public static SyntaxNodeComparer Default { get; } = new SyntaxNodeComparer();

        /// <inheritdoc />
        public int Compare(SyntaxNode x, SyntaxNode y)
        {
            if (x is null && y is null)
            {
                return 0;
            }

            if (x is null)
            {
                return 1;
            }

            if (y is null)
            {
                return -1;
            }

            if (x.IsEquivalentTo(y))
            {
                return 0;
            }

            return x.ToString().CompareTo(y.ToString());
        }

        /// <inheritdoc />
        public bool Equals(SyntaxNode x, SyntaxNode y) => x?.IsEquivalentTo(y) == true;

        /// <inheritdoc />
        public int GetHashCode(SyntaxNode obj) => obj.ToString().GetHashCode();
    }
}
