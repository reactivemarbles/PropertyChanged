// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal sealed record SingleExpressionOptimizedImplMethodDatum : MethodDatum
    {
        public SingleExpressionOptimizedImplMethodDatum(string inputType, string outputType, string accessModifier, List<string> memberNames)
        {
            InputTypeName = inputType;
            OutputTypeName = outputType;
            AccessModifier = accessModifier;
            MemberNames = memberNames;
        }

        public string InputTypeName { get; }

        public string OutputTypeName { get; }

        public string AccessModifier { get; }

        public List<string> MemberNames { get; }

        public override string CreateSource(ISourceCreator sourceCreator)
        {
            return sourceCreator.Create(this);
        }
    }
}
