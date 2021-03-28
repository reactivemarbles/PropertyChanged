// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq;
using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    internal class TestCaseUtil
    {
        public static bool ValidateAccessModifierCombination(
            Accessibility hostContainerTypeAccess,
            Accessibility hostTypeAccess,
            Accessibility propertyTypeAccess,
            Accessibility propertyAccess)
        {
            var propertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(propertyTypeAccess);
            var hostTypeInfo = new WhenChangedHostBuilder()
                .WithClassName("Host")
                .WithPropertyType(propertyTypeInfo)
                .WithPropertyAccess(propertyAccess)
                .WithClassAccess(hostTypeAccess)
                .AddNestedClass(propertyTypeInfo);
            var hostContainerSource = new EmptyClassBuilder()
                .WithClassName("HostContainer")
                .WithClassAccess(hostContainerTypeAccess)
                .AddNestedClass(hostTypeInfo)
                .BuildSource();

            Compilation compilation = CompilationUtil.CreateCompilation(hostContainerSource, GetWhenChangedStubClass());
            return compilation.GetDiagnostics().All(x => x.Severity < DiagnosticSeverity.Error);
        }

        public static string GetWhenChangedStubClass()
        {
            var assembly = typeof(Generator).Assembly;
            const string resourceName = "ReactiveMarbles.PropertyChanged.SourceGenerator.NotifyPropertyChangedExtensions.cs";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new System.IO.StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
