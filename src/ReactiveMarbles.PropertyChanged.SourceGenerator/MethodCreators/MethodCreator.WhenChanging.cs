﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators
{
    internal partial class MethodCreator
    {
        private static MethodDeclarationSyntax CreateWhenChanging(ExpressionArgument expressionArgument, bool isExplicitInvocation, bool isExtension, Accessibility accessModifier) => throw new NotImplementedException();
    }
}
