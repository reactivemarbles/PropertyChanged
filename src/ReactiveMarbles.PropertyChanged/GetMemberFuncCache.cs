// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactiveMarbles.PropertyChanged
{
    internal static class GetMemberFuncCache<TReturn>
    {
#if !UIKIT
        private static readonly
            ConcurrentDictionary<(Type fromType, string memberName), Func<INotifyPropertyChanged, TReturn>> Cache
                = new ConcurrentDictionary<(Type fromType, string memberName), Func<INotifyPropertyChanged, TReturn>>();
#endif

        [SuppressMessage("Design", "CA1801: Parameter not used", Justification = "Used on some platforms")]
        public static Func<INotifyPropertyChanged, TReturn> GetCache(Type fromType, MemberInfo memberInfo)
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
            return Cache.GetOrAdd((fromType, memberInfo.Name), _ =>
            {
                var instance = Expression.Parameter(typeof(INotifyPropertyChanged), "instance");

                var castInstance = Expression.Convert(instance, fromType);

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

                var parameters = new[] { instance };

                var lambdaExpression = Expression.Lambda<Func<INotifyPropertyChanged, TReturn>>(body, parameters);

                return lambdaExpression.Compile();
            });
#endif
        }
    }
}