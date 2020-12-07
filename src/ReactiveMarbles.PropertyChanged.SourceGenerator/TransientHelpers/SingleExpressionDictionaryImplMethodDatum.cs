// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal sealed record SingleExpressionDictionaryImplMethodDatum : MethodDatum
    {
        public SingleExpressionDictionaryImplMethodDatum(string inputTypeName, string outputTypeName, string accessModifier, MapDatum map)
        {
            InputTypeName = inputTypeName;
            OutputTypeName = outputTypeName;
            AccessModifier = accessModifier;
            Map = map;
        }

        public string InputTypeName { get; }

        public string OutputTypeName { get; }

        public string AccessModifier { get; }

        public MapDatum Map { get; }

        public override string CreateSource(ISourceCreator sourceCreator)
        {
            return sourceCreator.Create(this);
        }
    }
}
