﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal abstract record ClassDatum : IDatum
    {
        public ClassDatum(string name, List<MethodDatum> methodData)
        {
            Name = name;
            MethodData = methodData;
        }

        public string Name { get; }

        public List<MethodDatum> MethodData { get; }
    }
}
