// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal sealed record StringBuilderWhenChangedExtensionClassCreator : ISourceCreator
    {
        public string Create(IEnumerable<IDatum> sources)
        {
            var sb = new StringBuilder();
            sb.AppendLine(StringBuilderSourceCreatorHelper.GetUsingStatements());
            foreach (var datum in sources)
            {
                sb.AppendLine(datum switch
                    {
                        ExtensionClassDatum extension => Create(extension),
                        _ => throw new NotImplementedException("Unknown type of datum."),
                    });
            }

            return sb.ToString();
        }

        public string Create(ExtensionClassDatum classDatum)
        {
            var sb = new StringBuilder();

            var methodSources = string.Join(Environment.NewLine, classDatum.MethodData.Select(x => x switch
                {
                    SingleExpressionDictionaryImplMethodDatum methodDatum => Create(methodDatum),
                    SingleExpressionOptimizedImplMethodDatum methodDatum => Create(methodDatum),
                    MultiExpressionMethodDatum methodDatum => Create(methodDatum),
                    _ => throw new NotImplementedException("Unknown type of datum."),
                }));

            return StringBuilderSourceCreatorHelper.GetClass(methodSources);
        }

        public string Create(SingleExpressionDictionaryImplMethodDatum methodDatum) => SourceCreatorHelpers.GetMapMembers(methodDatum, true, StringBuilderWhenChangedSourceCreatorHelper.GetWhenChangedMapMethod);

        public string Create(SingleExpressionOptimizedImplMethodDatum methodDatum)
        {
            var sb = new StringBuilder();
            foreach (var (name, inputType, outputType) in methodDatum.Members)
            {
                sb.Append(StringBuilderSourceCreatorHelper.GetMapEntryChain(inputType.ToDisplayString(), outputType.ToDisplayString(), name));
            }

            return StringBuilderWhenChangedSourceCreatorHelper.GetWhenChangedMethodForDirectReturn(methodDatum.InputTypeName, methodDatum.OutputTypeName, methodDatum.AccessModifier, sb.ToString());
        }

        public string Create(MultiExpressionMethodDatum methodDatum)
        {
            var expressionParameters = StringBuilderSourceCreatorHelper.GetMultiExpressionMethodParameters(methodDatum.InputType.ToDisplayString(), methodDatum.OutputType.ToDisplayString(), methodDatum.TempReturnTypes);
            var body = StringBuilderSourceCreatorHelper.GetMultiExpressionMethodBody(methodDatum.TempReturnTypes.Count);

            return StringBuilderWhenChangedSourceCreatorHelper.GetMultiExpressionMethod(methodDatum.InputType.ToDisplayString(), methodDatum.OutputType.ToDisplayString(), methodDatum.AccessModifier, expressionParameters, body);
        }
    }
}
