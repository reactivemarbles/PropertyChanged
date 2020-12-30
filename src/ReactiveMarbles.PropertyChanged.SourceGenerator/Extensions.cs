﻿// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal static class Extensions
    {
        public static InputTypeGroup ToInputTypeGroup(this IEnumerable<OutputTypeGroup> source, ITypeSymbol inputType)
        {
            return new InputTypeGroup(inputType, source);
        }

        public static OutputTypeGroup ToOuputTypeGroup(this IEnumerable<ExpressionArgument> source)
        {
            return new OutputTypeGroup(source.ToList());
        }
    }
}
