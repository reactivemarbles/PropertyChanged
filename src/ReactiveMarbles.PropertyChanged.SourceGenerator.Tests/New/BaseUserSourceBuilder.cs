// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    internal abstract class BaseUserSourceBuilder
    {
        protected abstract bool IsNested { get; }

        protected abstract BaseUserSourceBuilder ContainerClass { get; }

        public abstract string GetTypeName();

        public abstract string BuildSource();

        public abstract IEnumerable<string> GetNamespacesRecursive();

        public abstract IEnumerable<string> GetNamespaces();

        public string BuildRoot()
        {
            var current = this;
            while (current.IsNested)
            {
                current = current.ContainerClass;
            }

            return current.BuildSource();
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Keep generic and non-generic versions together.")]
    internal abstract class BaseUserSourceBuilder<TBuilder> : BaseUserSourceBuilder
        where TBuilder : BaseUserSourceBuilder<TBuilder>
    {
        private static int _classCounter;
        private readonly TBuilder _instance;

        private string _className;
        private Accessibility _classAccess;
        private string _namespaceName;
        private BaseUserSourceBuilder _containerClass;
        private List<BaseUserSourceBuilder> _nestedClasses;
        private HashSet<string> _usings;

        public BaseUserSourceBuilder()
        {
            _instance = (TBuilder)this;
            _className = $"CustomClass{_classCounter++}";
            _classAccess = Accessibility.Public;
            _namespaceName = null;
            _containerClass = null;
            _nestedClasses = new List<BaseUserSourceBuilder>();
            _usings = new HashSet<string>();
        }

        protected override bool IsNested => _containerClass != null;

        protected override BaseUserSourceBuilder ContainerClass => _containerClass;

        protected bool HasNamespace => !string.IsNullOrEmpty(_namespaceName);

        protected string ClassName => _className;

        protected Accessibility ClassAccess => _classAccess;

        public override string GetTypeName()
        {
            return IsNested
                ? $"{_containerClass.GetTypeName()}+{_className}"
                : HasNamespace ? $"{_namespaceName}.{_className}" : _className;
        }

        public TBuilder WithClassName(string value)
        {
            _className = value;
            return _instance;
        }

        public TBuilder WithClassAccess(Accessibility value)
        {
            _classAccess = value;
            return _instance;
        }

        public TBuilder WithNamespace(string value)
        {
            if (_containerClass != null)
            {
                throw new InvalidOperationException("Tried to add a class to a namespace but the class is nested.");
            }

            _namespaceName = value;
            return _instance;
        }

        public TBuilder AddNestedClass<T>(BaseUserSourceBuilder<T> nestee)
            where T : BaseUserSourceBuilder<T>
        {
            if (nestee._containerClass != null)
            {
                throw new InvalidOperationException("Tried to nest a class but it's already nested.");
            }

            if (nestee._namespaceName != null)
            {
                throw new InvalidOperationException("Tried to nest a class but it's inside a namespace.");
            }

            _nestedClasses.Add(nestee);
            nestee._containerClass = this;
            return _instance;
        }

        public override string BuildSource()
        {
            var nestedClasses = string.Join('\n', _nestedClasses.Select(x => x.BuildSource()));
            var source = CreateClass(nestedClasses);
            source = ApplyNamespace(source);
            source = ApplyUsings(source);
            return source;
        }

        public sealed override IEnumerable<string> GetNamespacesRecursive()
        {
            return GetNamespaces().Concat(_nestedClasses.SelectMany(x => x.GetNamespacesRecursive()));
        }

        public override IEnumerable<string> GetNamespaces()
        {
            return Enumerable.Empty<string>();
        }

        protected abstract string CreateClass(string nestedClasses);

        private string ApplyNamespace(string source)
        {
            if (!string.IsNullOrEmpty(_namespaceName))
            {
                source = $@"
namespace {_namespaceName}
{{
{source}
}}
";
            }

            return source;
        }

        private string ApplyUsings(string source)
        {
            if (!IsNested)
            {
                var namespacesToInclude = new HashSet<string>(GetNamespacesRecursive());
                var usingDirectives = namespacesToInclude.Select(x => $"using {x};");
                var usingDirectivesString = string.Join('\n', usingDirectives);
                source = source.Insert(0, usingDirectivesString);
            }

            return source;
        }
    }
}
