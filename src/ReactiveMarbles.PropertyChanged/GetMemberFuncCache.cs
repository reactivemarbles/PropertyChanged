// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactiveMarbles.PropertyChanged
{
    internal static class GetMemberFuncCache<TFrom, TReturn>
    {
#if !UIKIT
        private static readonly ConcurrentDictionary<MemberInfo, Func<TFrom, TReturn>> Cache = new(new MemberFuncCacheKeyComparer());
#endif

        public static Func<TFrom, TReturn> GetCache(MemberInfo memberInfo)
        {
#if UIKIT
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    return input => (TReturn)propertyInfo.GetValue(input);
                case FieldInfo fieldInfo:
                    return input => (TReturn)fieldInfo.GetValue(input);
                default:
                    throw new ArgumentException($"Cannot handle member {memberInfo.Name}", nameof(memberInfo));
            }
#else
            return Cache.GetOrAdd(memberInfo, static mi =>
            {
                var instance = Expression.Parameter(typeof(TFrom), "instance");

                var castInstance = Expression.Convert(instance, mi.DeclaringType);

                Expression body = mi switch
                {
                    PropertyInfo propertyInfo => Expression.Call(castInstance, propertyInfo.GetGetMethod()),
                    FieldInfo fieldInfo => Expression.Field(castInstance, fieldInfo),
                    _ => throw new ArgumentException($"Cannot handle member {mi.Name}", nameof(mi)),
                };

                var parameters = new[] { instance };

                var lambdaExpression = Expression.Lambda<Func<TFrom, TReturn>>(body, parameters);

                return lambdaExpression.Compile();
            });
#endif
        }
    }
}