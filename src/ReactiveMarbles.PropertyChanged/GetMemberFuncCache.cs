// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactiveMarbles.PropertyChanged;

internal static class GetMemberFuncCache<TFrom, TReturn>
{
#if !UIKIT
    private static readonly ConcurrentDictionary<MemberInfo, Func<TFrom, TReturn>> Cache = new(new MemberFuncCacheKeyComparer());
#endif

    public static Func<TFrom, TReturn> GetCache(MemberInfo memberInfo) =>
#if UIKIT
            memberInfo switch
            {
                PropertyInfo propertyInfo => input => (TReturn)propertyInfo.GetValue(input),
                FieldInfo fieldInfo => input => (TReturn)fieldInfo.GetValue(input),
                _ => throw new ArgumentException($"Cannot handle member {memberInfo.Name}", nameof(memberInfo)),
            };
#else
        Cache.GetOrAdd(memberInfo, static memberInfo =>
        {
            var instance = Expression.Parameter(typeof(TFrom), "instance");

            var castInstance = Expression.Convert(instance, memberInfo.DeclaringType);

            Expression body = memberInfo switch
            {
                PropertyInfo propertyInfo => Expression.Call(castInstance, propertyInfo.GetGetMethod()),
                FieldInfo fieldInfo => Expression.Field(castInstance, fieldInfo),
                _ => throw new ArgumentException($"Cannot handle member {memberInfo.Name}", nameof(memberInfo)),
            };

            var parameters = new[] { instance };

            var lambdaExpression = Expression.Lambda<Func<TFrom, TReturn>>(body, parameters);

            return lambdaExpression.Compile();
        });
#endif

}
