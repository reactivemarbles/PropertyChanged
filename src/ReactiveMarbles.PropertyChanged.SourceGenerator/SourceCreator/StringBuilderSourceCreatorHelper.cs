// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal static class StringBuilderSourceCreatorHelper
    {
        public static string GetMultiExpressionMethodParameters(string inputType, string outputType, List<string> tempReturnTypes)
        {
            var sb = new StringBuilder();
            var counter = tempReturnTypes.Count;

            for (int i = 0; i < counter; i++)
            {
                sb.AppendLine($"        Expression<Func<{inputType}, {tempReturnTypes[i]}>> propertyExpression{i + 1},");
            }

            sb.Append("        Func<");
            for (int i = 0; i < counter; i++)
            {
                sb.Append($"{tempReturnTypes[i]}, ");
            }

            sb.Append($"{outputType}> conversionFunc)");

            return sb.ToString();
        }

        public static string GetMultiExpressionMethodBody(int counter)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < counter; i++)
            {
                sb.AppendLine($"        var obs{i + 1} = objectToMonitor.WhenChanged(propertyExpression{i + 1});");
            }

            sb.Append("        return obs1.CombineLatest(");
            for (int i = 1; i < counter; i++)
            {
                sb.Append($"obs{i + 1}, ");
            }

            sb.Append("conversionFunc);");

            return sb.ToString();
        }

        public static string GetMultiExpressionMethodBodyForPartialClass(int counter)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < counter; i++)
            {
                sb.AppendLine($"        var obs{i + 1} = this.WhenChanged(propertyExpression{i + 1});");
            }

            sb.Append("        return obs1.CombineLatest(");
            for (int i = 1; i < counter; i++)
            {
                sb.Append($"obs{i + 1}, ");
            }

            sb.Append("conversionFunc);");

            return sb.ToString();
        }

        public static string GetMultiExpressionMethod(string inputType, string outputType, string accessModifier, string expressionParameters, string body)
        {
            return $@"
    {accessModifier} static IObservable<{outputType}> WhenChanged(
        this {inputType} objectToMonitor,
{expressionParameters}
    {{
{body}
    }}
";
        }

        public static string GetMultiExpressionMethodForPartialClass(string inputType, string outputType, string accessModifier, string expressionParameters, string body)
        {
            return $@"
    {accessModifier} IObservable<{outputType}> WhenChanged(
{expressionParameters}
    {{
{body}
    }}
";
        }

        public static string GetWhenChangedMethodForMap(string inputType, string outputType, string accessModifier, string mapName)
        {
            return $@"
    {accessModifier} static IObservable<{outputType}> WhenChanged(this {inputType} source, Expression<Func<{inputType}, {outputType}>> propertyExpression)
    {{
        return {mapName}[propertyExpression.Body.ToString()].Invoke(source);
    }}
";
        }

        public static string GetWhenChangedMethodForDirectReturn(string inputType, string outputType, string accessModifier, string valueChain)
        {
            return $@"
    {accessModifier} static IObservable<{outputType}> WhenChanged(this {inputType} source, Expression<Func<{inputType}, {outputType}>> propertyExpression)
    {{
        return Observable.Return(source){valueChain};
    }}
";
        }

        public static string GetMapEntryChain(string memberName)
        {
            return $@"
                .Where(x => x != null)
                .Select(x => GenerateObservable(x, ""{memberName}"", y => y.{memberName}))
                .Switch()";
        }

        public static string GetMapEntry(string key, string valueChain)
        {
            return $@"
        {{
            ""{key}"",
            source => Observable.Return(source){valueChain}
        }},";
        }

        public static string GetMap(string inputType, string outputType, string mapName, string entries)
        {
            return $@"
    private static readonly Dictionary<string, Func<{inputType}, IObservable<{outputType}>>> {mapName} = new Dictionary<string, Func<{inputType}, IObservable<{outputType}>>>()
    {{
{entries}
    }};
";
        }

        public static string GetClass(string body)
        {
            return $@"
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive.Linq;

public static partial class NotifyPropertyChangedExtensions
{{
    {body}
}}
";
        }

        public static string GetPartialClass(string namespaceName, string className, string accessModifier, IEnumerable<AncestorClassInfo> ancestorClasses, string body)
        {
            var source = $@"
{accessModifier} partial class {className}
{{
    {body}

    private static IObservable<T> GenerateObservable<TObj, T>(
            TObj parent,
            string memberName,
            Func<TObj, T> getter)
        where TObj : INotifyPropertyChanged
    {{
        return Observable.Create<T>(
                observer =>
                {{
                    PropertyChangedEventHandler handler = (object sender, PropertyChangedEventArgs e) =>
                    {{
                        if (e.PropertyName == memberName)
                        {{
                            observer.OnNext(getter(parent));
                        }}
                    }};

                    parent.PropertyChanged += handler;

                    return Disposable.Create((parent, handler), x => x.parent.PropertyChanged -= x.handler);
                }})
            .StartWith(getter(parent));
    }}
}}
";

            foreach (var ancestorClass in ancestorClasses)
            {
                source = $@"
{ancestorClass.AccessModifier} partial class {ancestorClass.Name}
{{
{source}
}}
";
            }

            if (!string.IsNullOrEmpty(namespaceName))
            {
                source = $@"
namespace {namespaceName}
{{
{source}
}}
";
            }

            var usingClauses = @"
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
";

            return source.Insert(0, usingClauses);
        }

        public static string GetPartialClassWhenChangedMethodForMap(string inputType, string outputType, string accessModifier, string mapName)
        {
            return $@"
    {accessModifier} IObservable<{outputType}> WhenChanged(Expression<Func<{inputType}, {outputType}>> propertyExpression)
    {{
        return {mapName}[propertyExpression.Body.ToString()].Invoke(this);
    }}
";
        }

        public static string GetPartialClassWhenChangedMethodForDirectReturn(string inputType, string outputType, string accessModifier, string valueChain)
        {
            // Making the access modifier public so multi-expression extensions will able to access it, if needed.
            return $@"
    {accessModifier} IObservable<{outputType}> WhenChanged(Expression<Func<{inputType}, {outputType}>> propertyExpression)
    {{
        return Observable.Return(this){valueChain};
    }}
";
        }

        public static string GetWhenChangedStubClass()
        {
            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "ReactiveMarbles.PropertyChanged.SourceGenerator.NotifyPropertyChangedExtensions.cs";

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
