// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal static class StringBuilderWhenChangedSourceCreatorHelper
    {
        public static string GetMultiExpressionMethod(string inputType, string outputType, Accessibility accessModifier, string expressionParameters, string body)
        {
            return $@"
    {accessModifier.ToFriendlyString()} static IObservable<{outputType}> WhenChanged(
        this {inputType} objectToMonitor,
{expressionParameters}
    {{
{body}
    }}
";
        }

        public static string GetMultiExpressionMethodForPartialClass(string inputType, string outputType, Accessibility accessModifier, string expressionParameters, string body)
        {
            return $@"
    {accessModifier.ToFriendlyString()} IObservable<{outputType}> WhenChanged(
{expressionParameters}
    {{
{body}
    }}
";
        }

        public static string GetWhenChangedMethodForMap(string inputType, string outputType, Accessibility accessModifier, string mapName)
        {
            return $@"
    {accessModifier.ToFriendlyString()} static IObservable<{outputType}> WhenChanged(
        this {inputType} source,
        Expression<Func<{inputType}, {outputType}>> propertyExpression,
        [CallerMemberName]string callerMemberName = null,
        [CallerFilePath]string callerFilePath = null,
        [CallerLineNumber]int callerLineNumber = 0)
    {{
        return {mapName}[propertyExpression.Body.ToString()].Invoke(source);
    }}
";
        }

        public static string GetWhenChangedMethodForDirectReturn(string inputType, string outputType, Accessibility accessModifier, string valueChain)
        {
            return $@"
    /// <summary>
    /// Generates a IObservable which signals with updated property value changes.
    /// </summary>
    /// <param name=""source"">The source of the property changes.</param>
    /// <param name=""propertyExpression"">The property.</param>
    /// <returns>The observable which signals with updates.</returns>
    {accessModifier.ToFriendlyString()} static IObservable<{outputType}> WhenChanged(
        this {inputType} source,
        Expression<Func<{inputType}, {outputType}>> propertyExpression,
        [CallerMemberName]string callerMemberName = null,
        [CallerFilePath]string callerFilePath = null,
        [CallerLineNumber]int callerLineNumber = 0)
    {{
        return Observable.Return(source){valueChain};
    }}
";
        }

        public static string GetWhenChangedMapMethod(string inputType, string outputType, bool isExtension, Accessibility accessModifier, string mapName)
        {
            var staticExpression = isExtension ? "static" : string.Empty;

            var sb = new StringBuilder($"   {accessModifier.ToFriendlyString()} {staticExpression} IObservable<{outputType}> WhenChanged(").AppendLine();

            string invokeName;
            if (isExtension)
            {
                invokeName = "source";
                sb.AppendLine($"        this {inputType} source,");
            }
            else
            {
                invokeName = "this";
            }

            sb.AppendLine($@"        Expression<Func<{inputType}, {outputType}>> propertyExpression, 
        [CallerMemberName]string callerMemberName = null,
        [CallerFilePath]string callerFilePath = null,
        [CallerLineNumber]int callerLineNumber = 0)
    {{
        return {mapName}[propertyExpression.Body.ToString()].Invoke({invokeName});
    }}");

            return sb.ToString();
        }

        public static string GetPartialClassWhenChangedMethodForDirectReturn(string inputType, string outputType, Accessibility accessModifier, List<ExpressionChain> members)
        {
            var observableChainStringBuilder = new StringBuilder(StringBuilderSourceCreatorHelper.GetObservableCreation(members[0].InputType.ToDisplayString(), "this", members[0].OutputType.ToDisplayString(), members[0].Name));

            foreach (var member in members.Skip(1))
            {
                observableChainStringBuilder.Append(StringBuilderSourceCreatorHelper.GetMapEntryChain(member.InputType.ToDisplayString(), member.OutputType.ToDisplayString(), member.Name));
            }

            // Making the access modifier public so multi-expression extensions will able to access it, if needed.
            return $@"
    {accessModifier.ToFriendlyString()} IObservable<{outputType}> WhenChanged(
        Expression<Func<{inputType}, {outputType}>> propertyExpression,
        [CallerMemberName]string callerMemberName = null,
        [CallerFilePath]string callerFilePath = null,
        [CallerLineNumber]int callerLineNumber = 0)
    {{
        return {observableChainStringBuilder};
    }}
";
        }
    }
}
