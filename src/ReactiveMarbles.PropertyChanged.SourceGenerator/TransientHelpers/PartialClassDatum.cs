// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal sealed record PartialClassDatum : ClassDatum
    {
        public PartialClassDatum(string namespaceName, string name, IEnumerable<MethodDatum> methodData)
            : base(name, methodData)
        {
            NamespaceName = namespaceName;
        }

        public string NamespaceName { get; }

        public override string CreateSource(ISourceCreator sourceCreator)
        {
            var methodSource = CreateMethodSource(sourceCreator);

            return StringBuilderSourceCreatorHelper.GetPartialClass(NamespaceName, Name, methodSource);
        }
    }
}
