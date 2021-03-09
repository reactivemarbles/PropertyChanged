// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal static class Extensions
    {
        public static InputTypeGroup ToInputTypeGroup(this IEnumerable<OutputTypeGroup> source, ITypeSymbol inputType) =>
            new InputTypeGroup(inputType, source);

        public static OutputTypeGroup ToOuputTypeGroup(this IEnumerable<ExpressionArgument> source) =>
            new OutputTypeGroup(source.ToList());

        public static string GetVariableName(this ITypeSymbol outputTypeSymbol) =>
            $"{outputTypeSymbol.ToDisplayParts().Where(x => x.Kind != SymbolDisplayPartKind.Punctuation).Select(x => x.ToString()).Aggregate((a, b) => a + b)}".FirstCharToUpper();

        public static string FirstCharToUpper(this string input) =>
           input switch
           {
               null => throw new ArgumentNullException(nameof(input)),
               "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
               _ => input.First().ToString().ToUpper() + input.Substring(1)
           };
    }
}
