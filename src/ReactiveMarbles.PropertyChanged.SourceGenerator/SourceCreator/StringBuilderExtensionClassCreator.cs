// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal sealed record StringBuilderExtensionClassCreator : ISourceCreator
    {
        public string Create(ExtensionClassDatum classDatum)
        {
            var sb = new StringBuilder();
            foreach (var methodDatum in classDatum.MethodData)
            {
                sb.AppendLine(methodDatum.CreateSource(this));
            }

            var methodSource = sb.ToString();
            return StringBuilderSourceCreatorHelper.GetClass(methodSource);
        }

        public string Create(SingleExpressionDictionaryImplMethodDatum methodDatum)
        {
            var mapEntrySb = new StringBuilder();
            foreach (var entry in methodDatum.Map.Entries)
            {
                var valueChainSb = new StringBuilder();
                foreach (var memberName in entry.MemberNames)
                {
                    valueChainSb.Append(StringBuilderSourceCreatorHelper.GetMapEntryChain(memberName));
                }

                mapEntrySb.Append(StringBuilderSourceCreatorHelper.GetMapEntry(entry.Key, valueChainSb.ToString()));
            }

            var map = StringBuilderSourceCreatorHelper.GetMap(methodDatum.InputTypeName, methodDatum.OutputTypeName, methodDatum.Map.MapName, mapEntrySb.ToString());
            var method = StringBuilderSourceCreatorHelper.GetWhenChangedMethodForMap(methodDatum.InputTypeName, methodDatum.OutputTypeName, methodDatum.AccessModifier, methodDatum.Map.MapName);

            return map + "\n" + method;
        }

        public string Create(SingleExpressionOptimizedImplMethodDatum methodDatum)
        {
            var sb = new StringBuilder();
            foreach (var memberName in methodDatum.MemberNames)
            {
                sb.Append(StringBuilderSourceCreatorHelper.GetMapEntryChain(memberName));
            }

            return StringBuilderSourceCreatorHelper.GetWhenChangedMethodForDirectReturn(methodDatum.InputTypeName, methodDatum.OutputTypeName, methodDatum.AccessModifier, sb.ToString());
        }

        public string Create(MultiExpressionMethodDatum methodDatum)
        {
            var expressionParameters = StringBuilderSourceCreatorHelper.GetMultiExpressionMethodParameters(methodDatum.InputTypeFullName, methodDatum.OutputTypeFullName, methodDatum.TempReturnTypes);
            var body = StringBuilderSourceCreatorHelper.GetMultiExpressionMethodBody(methodDatum.TempReturnTypes.Count);

            return StringBuilderSourceCreatorHelper.GetMultiExpressionMethod(methodDatum.InputTypeFullName, methodDatum.OutputTypeFullName, methodDatum.AccessModifier, expressionParameters, body);
        }
    }
}
