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
            new(inputType, source.ToList());

        public static OutputTypeGroup ToOutputTypeGroup(this List<ExpressionArgument> source, ITypeSymbol outputType) =>
            new OutputTypeGroup(outputType, source);

        public static string GetVariableName(this ITypeSymbol outputTypeSymbol) =>
            $"{outputTypeSymbol.ToDisplayParts().Where(x => x.Kind != SymbolDisplayPartKind.Punctuation).Select(x => x.ToString()).Aggregate((a, b) => a + b)}".FirstCharToUpper();

        public static string FirstCharToUpper(this string input) =>
           input switch
           {
               null => throw new ArgumentNullException(nameof(input)),
               "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
               _ => input.First().ToString().ToUpper() + input.Substring(1)
           };

        public static void ReportDiagnostic(this GeneratorExecutionContext context, DiagnosticDescriptor descriptor, Location location = null)
        {
            context.ReportDiagnostic(Diagnostic.Create(descriptor: descriptor, location: location));
        }

        public static void BinaryListInsert<TKey, TItem>(this SortedList<TKey, List<TItem>> dictionary, TKey key, TItem item, IComparer<TItem> comparer = null)
        {
            if (!dictionary.TryGetValue(key, out var list))
            {
                list = new List<TItem>();
                dictionary[key] = list;
            }

            var index = comparer != null ? list.BinarySearch(item, comparer) : list.BinarySearch(item);

            if (index < 0)
            {
                list.Insert(~index, item);
            }
        }

        public static void ListInsert<TKey, TItem>(this SortedList<TKey, HashSet<TItem>> dictionary, TKey key, TItem item)
        {
            if (!dictionary.TryGetValue(key, out var list))
            {
                list = new HashSet<TItem>();
                dictionary[key] = list;
            }

            list.Add(item);
        }

        public static void InsertOutputGroup(this SortedList<ITypeSymbol, OutputTypeGroup> inputList, ITypeSymbol type, ExpressionArgument expression)
        {
            if (!inputList.TryGetValue(type, out var outputGroup))
            {
                outputGroup = new OutputTypeGroup(type);
                inputList[type] = outputGroup;
            }

            outputGroup.ExpressionArguments.Add(expression);
        }

        public static void BinaryInsertOutputGroup(this SortedList<ITypeSymbol, OutputTypeGroup> inputList, ITypeSymbol type, ExpressionArgument expression)
        {
            if (!inputList.TryGetValue(type, out var outputGroup))
            {
                outputGroup = new OutputTypeGroup(type);
                inputList[type] = outputGroup;
            }

            var index = outputGroup.ExpressionArguments.BinarySearch(expression);

            if (index < 0)
            {
                outputGroup.ExpressionArguments.Insert(~index, expression);
            }
        }
    }
}
