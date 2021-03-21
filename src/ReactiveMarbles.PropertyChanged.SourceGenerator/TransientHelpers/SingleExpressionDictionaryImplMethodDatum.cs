// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    /// <summary>
    /// There are multiple expressions with the same input/output type so it needs a map to return the correct chain.
    /// e.g. x.String1, x.String2, x.Child.String1 would each be a different entry in the dictionary.
    /// </summary>
    internal sealed record SingleExpressionDictionaryImplMethodDatum : MethodDatum
    {
        public SingleExpressionDictionaryImplMethodDatum(string inputTypeName, string outputTypeName, Accessibility accessModifier, MapDatum map)
        {
            InputTypeName = inputTypeName;
            OutputTypeName = outputTypeName;
            AccessModifier = accessModifier;
            Map = map;
        }

        public string InputTypeName { get; }

        public string OutputTypeName { get; }

        public Accessibility AccessModifier { get; }

        public MapDatum Map { get; }
    }
}
