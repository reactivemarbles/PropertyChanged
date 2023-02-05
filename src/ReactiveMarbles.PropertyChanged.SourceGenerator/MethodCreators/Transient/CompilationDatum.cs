// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Helpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;

internal sealed class CompilationDatum
{
    private readonly Dictionary<string, NamespaceDatum> _extensionNamespaces = new();
    private readonly Dictionary<string, NamespaceDatum> _partialsNamespaces = new();

    public NamespaceDatum GetNamespace(string namespaceName, bool isExtension)
    {
        var isGlobal = string.IsNullOrWhiteSpace(namespaceName);

        var dictionary = isExtension ? _extensionNamespaces : _partialsNamespaces;

        if (!dictionary.TryGetValue(namespaceName, out var namespaceDatum))
        {
            namespaceDatum = new(namespaceName, isGlobal);
            dictionary[namespaceName] = namespaceDatum;
        }

        return namespaceDatum;
    }

    public ClassDatum GetClass(ITypeSymbol symbol, Accessibility classAccessibility, bool isExtension, string extensionClass)
    {
        var namespaceName = isExtension ? string.Empty : symbol.ContainingNamespace.ToNamespaceName();
        var className = isExtension ? extensionClass : symbol.Name;
        var ancestors = isExtension ? Array.Empty<ClassDatum>() : (IReadOnlyList<ClassDatum>)symbol.GetAncestorsClassDatum();
        var fileDatum = GetNamespace(namespaceName, isExtension);

        var dictionary = fileDatum.ClassDictionary;

        if (dictionary.TryGetValue(className, out var classMethodDatum))
        {
            return classMethodDatum;
        }

        classMethodDatum = new(className, classAccessibility, isExtension, ancestors);
        dictionary.Add(className, classMethodDatum);

        return classMethodDatum;
    }

    public IEnumerable<NamespaceDatum> GetExtensions() => _extensionNamespaces.Values;

    public IEnumerable<NamespaceDatum> GetPartials() => _partialsNamespaces.Values;

    public IEnumerable<ClassDatum> GetExtensionClasses() => GetExtensions().SelectMany(namespaceDefinition => namespaceDefinition.Classes);
}
