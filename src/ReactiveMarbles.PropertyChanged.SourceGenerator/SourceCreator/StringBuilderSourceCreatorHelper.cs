// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
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
