// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal class StringBuilderBindPartialClassCreator : ISourceCreator
    {
        public string Create(PartialClassDatum classDatum) => throw new NotImplementedException();

        public string Create(MultiExpressionMethodDatum methodDatum) => throw new NotImplementedException();

        public string Create(SingleExpressionDictionaryImplMethodDatum methodDatum) => throw new NotImplementedException();

        public string Create(SingleExpressionOptimizedImplMethodDatum methodDatum) => throw new NotImplementedException();

        public string Create(IEnumerable<IDatum> sourceDatums) => throw new NotImplementedException();
    }
}
