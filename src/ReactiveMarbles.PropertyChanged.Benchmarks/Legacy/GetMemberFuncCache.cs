// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactiveMarbles.PropertyChanged.Benchmarks.Legacy
{
    internal static class GetMemberFuncCache<TFrom, TReturn>
    {
#if !UIKIT
        private static readonly
            ConcurrentDictionary<(Type FromType, string MemberName), Func<TFrom, TReturn>> Cache
                = new ConcurrentDictionary<(Type, string), Func<TFrom, TReturn>>();
#endif

        [SuppressMessage("Design", "CA1801: Parameter not used", Justification = "Used on some platforms")]
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
            return Cache.GetOrAdd((memberInfo.DeclaringType, memberInfo.Name), _ =>
            {
                ParameterExpression instance = Expression.Parameter(typeof(TFrom), "instance");

                UnaryExpression castInstance = Expression.Convert(instance, memberInfo.DeclaringType);

                Expression body;

                switch (memberInfo)
                {
                    case PropertyInfo propertyInfo:
                        body = Expression.Call(castInstance, propertyInfo.GetGetMethod());
                        break;
                    case FieldInfo fieldInfo:
                        body = Expression.Field(castInstance, fieldInfo);
                        break;
                    default:
                        throw new ArgumentException($"Cannot handle member {memberInfo.Name}", nameof(memberInfo));
                }

                ParameterExpression[] parameters = new[] { instance };

                Expression<Func<TFrom, TReturn>> lambdaExpression = Expression.Lambda<Func<TFrom, TReturn>>(body, parameters);

                return lambdaExpression.Compile();
            });
#endif
        }
    }
}