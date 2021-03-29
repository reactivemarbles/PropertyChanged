﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Builders
{
    /// <summary>
    /// The base class used for simplifying the creation of source code.
    /// </summary>
    public abstract class BaseUserSourceBuilder
    {
        /// <summary>
        /// Gets a value indicating whether this class is nested.
        /// </summary>
        protected abstract bool IsNested { get; }

        /// <summary>
        /// Gets the class that this class is nested inside.
        /// </summary>
        protected abstract BaseUserSourceBuilder ContainerClass { get; }

        /// <summary>
        /// Gets the name of this type, including container classes.
        /// </summary>
        /// <remarks>
        /// Note that this name includes the '+' character, required when retrieving a type from an assembly.
        /// So, it needs to be replaced with '.' if used in source code.
        /// </remarks>
        /// <returns>The full name of the type.</returns>
        public abstract string GetTypeName();

        /// <summary>
        /// Builds the source code for this class.
        /// </summary>
        /// <returns>The created source code.</returns>
        public abstract string BuildSource();

        /// <summary>
        /// Gets the namespaces to include as using directives, including those required by nested classes.
        /// </summary>
        /// <returns>A collection of namespaces.</returns>
        public abstract IEnumerable<string> GetNamespacesRecursive();

        /// <summary>
        /// Gets the namespaces to include as using directives, <i>not</i> including those required by nested classes.
        /// </summary>
        /// <returns>A collection of namespaces.</returns>
        public abstract IEnumerable<string> GetNamespaces();

        /// <summary>
        /// Builds the source code starting from the root class (in case this class is nested).
        /// </summary>
        /// <returns>The created source code.</returns>
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

    /// <summary>
    /// The base class used for simplifying the creation of source code.
    /// </summary>
    /// <typeparam name="TBuilder">The type of builder.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Keep generic and non-generic versions together.")]
    public abstract class BaseUserSourceBuilder<TBuilder> : BaseUserSourceBuilder
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

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseUserSourceBuilder{TBuilder}"/> class.
        /// </summary>
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

        /// <inheritdoc/>
        protected override bool IsNested => _containerClass != null;

        /// <inheritdoc/>
        protected override BaseUserSourceBuilder ContainerClass => _containerClass;

        /// <summary>
        /// Gets a value indicating whether this class has a namespace.
        /// </summary>
        protected bool HasNamespace => !string.IsNullOrEmpty(_namespaceName);

        /// <summary>
        /// Gets the name of this class.
        /// </summary>
        protected string ClassName => _className;

        /// <summary>
        /// Gets the access modifier of this class.
        /// </summary>
        protected Accessibility ClassAccess => _classAccess;

        /// <inheritdoc/>
        public override string GetTypeName()
        {
            return IsNested
                ? $"{_containerClass.GetTypeName()}+{_className}"
                : HasNamespace ? $"{_namespaceName}.{_className}" : _className;
        }

        /// <summary>
        /// Sets the class name.
        /// </summary>
        /// <param name="value">A class name.</param>
        /// <returns>A reference to this builder.</returns>
        public TBuilder WithClassName(string value)
        {
            _className = value;
            return _instance;
        }

        /// <summary>
        /// Sets the class access modifier.
        /// </summary>
        /// <param name="value">An access modifier.</param>
        /// <returns>A reference to this builder.</returns>
        public TBuilder WithClassAccess(Accessibility value)
        {
            _classAccess = value;
            return _instance;
        }

        /// <summary>
        /// Sets the namespace name.
        /// </summary>
        /// <param name="value">A namespace name.</param>
        /// <returns>A reference to this builder.</returns>
        public TBuilder WithNamespace(string value)
        {
            if (_containerClass != null)
            {
                throw new InvalidOperationException("Tried to add a class to a namespace but the class is nested.");
            }

            _namespaceName = value;
            return _instance;
        }

        /// <summary>
        /// Sets the class name.
        /// </summary>
        /// <typeparam name="T">The type of builder.</typeparam>
        /// <param name="nestee">The class to nest inside of this one.</param>
        /// <returns>A reference to this builder.</returns>
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

        /// <inheritdoc/>
        public override string BuildSource()
        {
            var nestedClasses = string.Join(Environment.NewLine, _nestedClasses.Select(x => x.BuildSource()));
            var source = CreateClass(nestedClasses);
            source = ApplyNamespace(source);
            source = ApplyUsings(source);
            return source;
        }

        /// <inheritdoc/>
        public sealed override IEnumerable<string> GetNamespacesRecursive()
        {
            return GetNamespaces().Concat(_nestedClasses.SelectMany(x => x.GetNamespacesRecursive()));
        }

        /// <inheritdoc/>
        public override IEnumerable<string> GetNamespaces()
        {
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Creates the source code for this class.
        /// </summary>
        /// <param name="nestedClasses">The source code for classes nested inside of this one.</param>
        /// <returns>The created source code.</returns>
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
                var usingDirectivesString = string.Join(Environment.NewLine, usingDirectives);
                source = source.Insert(0, usingDirectivesString);
            }

            return source;
        }
    }
}
