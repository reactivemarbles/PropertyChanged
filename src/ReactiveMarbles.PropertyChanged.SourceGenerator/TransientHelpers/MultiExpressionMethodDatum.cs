﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    /// <summary>
    /// Holds the data for the creating the implementation of obs1.CombineLatest(obs2, obs3, ..., conversionFunc).
    /// </summary>
    internal sealed record MultiExpressionMethodDatum : MethodDatum
    {
        public MultiExpressionMethodDatum(Accessibility accessModifier, IEnumerable<string> typeNames, bool containsPrivateOrProtectedTypeArgument)
        {
            AccessModifier = accessModifier;
            ContainsPrivateOrProtectedTypeArgument = containsPrivateOrProtectedTypeArgument;

            var list = typeNames.ToArray();
            InputTypeFullName = list[0];
            OutputTypeFullName = list[list.Length - 1];
            TempReturnTypes = new List<string>(list.Length - 2);
            for (var i = 1; i < list.Length - 1; i++)
            {
                TempReturnTypes.Add(list[i]);
            }
        }

        public string InputTypeFullName { get; }

        public string OutputTypeFullName { get; }

        public Accessibility AccessModifier { get; }

        public List<string> TempReturnTypes { get; }

        public bool ContainsPrivateOrProtectedTypeArgument { get; }

        public override string CreateSource(ISourceCreator sourceCreator)
        {
            return sourceCreator.Create(this);
        }

        public bool Equals(MultiExpressionMethodDatum other)
        {
            if (other is null)
            {
                return false;
            }

            var result =
                InputTypeFullName == other.InputTypeFullName &&
                OutputTypeFullName == other.OutputTypeFullName &&
                TempReturnTypes.Count == other.TempReturnTypes.Count;

            if (!result)
            {
                return false;
            }

            for (var i = 0; i < TempReturnTypes.Count; ++i)
            {
                result &= EqualityComparer<string>.Default.Equals(TempReturnTypes[i], other.TempReturnTypes[i]);
            }

            return result;
        }

        public override int GetHashCode()
        {
            var hashCode = 1230885993;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(InputTypeFullName);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(OutputTypeFullName);

            foreach (var typeName in TempReturnTypes)
            {
                hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(typeName);
            }

            return hashCode;
        }
    }
}
