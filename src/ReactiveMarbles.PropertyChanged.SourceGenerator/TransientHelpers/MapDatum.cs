// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal sealed record MapDatum
    {
        public MapDatum(string mapName, List<MapEntryDatum> entries)
        {
            MapName = mapName;
            Entries = entries;
        }

        public string InputType { get; }

        public string OutputType { get; }

        public string MapName { get; }

        public List<MapEntryDatum> Entries { get; }
    }
}
