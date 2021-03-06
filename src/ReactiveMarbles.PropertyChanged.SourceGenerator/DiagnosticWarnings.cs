﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal static class DiagnosticWarnings
    {
        internal static readonly DiagnosticDescriptor ExpressionMustBeInline = new DiagnosticDescriptor(
            id: "RXM001",
            title: "Expression chain must be inline",
            messageFormat: "The expression must be inline (e.g. not a variable or method invocation).",
            category: "Compiler",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        internal static readonly DiagnosticDescriptor OnlyPropertyAndFieldAccessAllowed = new DiagnosticDescriptor(
            id: "RXM002",
            title: "Expression chain may only consist of property and field access",
            messageFormat: "The expression may only consist of field and property access",
            category: "Compiler",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        internal static readonly DiagnosticDescriptor LambdaParameterMustBeUsed = new DiagnosticDescriptor(
            id: "RXM003",
            title: "Lambda parameter must be used in expression chain",
            messageFormat: "The lambda parameter must be used in the expression chain",
            category: "Compiler",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
