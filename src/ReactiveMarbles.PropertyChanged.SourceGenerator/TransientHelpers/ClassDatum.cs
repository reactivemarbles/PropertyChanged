// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal sealed record ClassDatum
    {
        public ClassDatum(string inputTypeName, IEnumerable<MethodDatum> methodData)
        {
            InputTypeName = inputTypeName;
            MethodData = methodData;
        }

        public string InputTypeName { get; }

        public IEnumerable<MethodDatum> MethodData { get; }
    }
}
