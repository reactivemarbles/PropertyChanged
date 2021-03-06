﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal sealed record PartialClassDatum : ClassDatum
    {
        public PartialClassDatum(string namespaceName, string name, Accessibility accessModifier, IEnumerable<AncestorClassInfo> ancestorClasses, IEnumerable<MethodDatum> methodData)
            : base(name, methodData)
        {
            NamespaceName = namespaceName;
            AccessModifier = accessModifier;
            AncestorClasses = ancestorClasses;
        }

        public string NamespaceName { get; }

        public Accessibility AccessModifier { get; }

        public IEnumerable<AncestorClassInfo> AncestorClasses { get; }
    }
}
