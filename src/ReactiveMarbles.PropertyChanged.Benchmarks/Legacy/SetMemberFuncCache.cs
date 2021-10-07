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
    internal static class SetMemberFuncCache<TValue>
    {
#if !UIKIT
        private static readonly
            ConcurrentDictionary<(Type ParentType, string PropertyName), Action<object, TValue>> Cache
                = new ConcurrentDictionary<(Type, string), Action<object, TValue>>();
#endif

        [SuppressMessage("Design", "CA1801: Parameter not used", Justification = "Used on some platforms")]
        public static Action<object, TValue> GenerateSetCache(MemberInfo memberInfo)
        {
#if UIKIT
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    return (input, value) => propertyInfo.SetValue(input, value);
                case FieldInfo fieldInfo:
                    return (input, value) => fieldInfo.SetValue(input, value);
                default:
                    throw new ArgumentException($"Cannot handle member {memberInfo.Name}", nameof(memberInfo));
            }
#else
            return Cache.GetOrAdd((memberInfo.DeclaringType, memberInfo.Name), _ =>
            {
                ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
                ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "property");

                Expression body;

                switch (memberInfo)
                {
                    case PropertyInfo propertyInfo:
                        UnaryExpression convertProp = Expression.Convert(valueParam, propertyInfo.PropertyType);
                        UnaryExpression convertInstanceProp = Expression.Convert(instance, propertyInfo.DeclaringType);
                        body = Expression.Call(convertInstanceProp, propertyInfo.GetSetMethod(), convertProp);
                        break;
                    case FieldInfo fieldInfo:
                        UnaryExpression convertInstanceField = Expression.Convert(instance, fieldInfo.DeclaringType);
                        MemberExpression field = Expression.Field(convertInstanceField, fieldInfo);
                        UnaryExpression convertField = Expression.Convert(valueParam, fieldInfo.FieldType);
                        body = Expression.Assign(field, convertField);
                        break;
                    default:
                        throw new ArgumentException($"Cannot handle member {memberInfo.Name}", nameof(memberInfo));
                }

                ParameterExpression[] parameters = new[] { instance, valueParam };

                Expression<Action<object, TValue>> lambdaExpression = Expression.Lambda<Action<object, TValue>>(body, parameters);

                return lambdaExpression.Compile();
            });
#endif
        }
    }
}