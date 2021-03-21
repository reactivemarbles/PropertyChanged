// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal static class SourceCreatorHelpers
    {
        public static string GetMapMembers(SingleExpressionDictionaryImplMethodDatum methodDatum, bool isExtension, Func<string, string, bool, Accessibility, string, string> sourceGenerator)
        {
            var initialSource = isExtension ? "source" : "this";

            var mapEntrySb = new StringBuilder();
            foreach (var entry in methodDatum.Map.Entries)
            {
                var valueChainSb = new StringBuilder();
                for (int i = 0; i < entry.Members.Count; ++i)
                {
                    var (name, inputType, outputType) = entry.Members[i];

                    if (i == 0)
                    {
                        valueChainSb.Append(StringBuilderSourceCreatorHelper.GetObservableCreation(inputType, initialSource, outputType, name));
                    }
                    else
                    {
                        valueChainSb.Append(StringBuilderSourceCreatorHelper.GetMapEntryChain(inputType, outputType, name));
                    }
                }

                mapEntrySb.Append(StringBuilderSourceCreatorHelper.GetMapEntry(entry.Key, valueChainSb.ToString()));
            }

            var map = StringBuilderSourceCreatorHelper.GetMap(methodDatum.InputTypeName, methodDatum.OutputTypeName, methodDatum.Map.MapName, mapEntrySb.ToString());
            var method = sourceGenerator.Invoke(methodDatum.InputTypeName, methodDatum.OutputTypeName, isExtension, methodDatum.AccessModifier, methodDatum.Map.MapName);

            return map + "\n" + method;
        }
    }
}
