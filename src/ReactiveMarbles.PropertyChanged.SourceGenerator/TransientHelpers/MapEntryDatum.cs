// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal sealed record MapEntryDatum
    {
        public MapEntryDatum(string key, List<(string Name, string InputType, string OutputType)> members)
        {
            Key = key;
            Members = members;
        }

        public string Key { get; }

        /// <summary>
        /// Gets the members.
        /// This would be the Expression's Input and Output.
        /// </summary>
        public List<(string Name, string InputType, string OutputType)> Members { get; }
    }
}
