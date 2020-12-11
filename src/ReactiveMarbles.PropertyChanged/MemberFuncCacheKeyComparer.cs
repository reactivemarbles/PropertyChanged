﻿// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Reflection;

namespace ReactiveMarbles.PropertyChanged
{
    internal sealed class MemberFuncCacheKeyComparer : IEqualityComparer<MemberInfo>
    {
        public bool Equals(MemberInfo x, MemberInfo y)
        {
            return (x.DeclaringType, x.Name) == (y.DeclaringType, y.Name);
        }

        public int GetHashCode(MemberInfo obj)
        {
            return (obj.DeclaringType, obj.Name).GetHashCode();
        }
    }
}
