// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using ReactiveUI;

namespace ReactiveMarbles.PropertyChanged.Benchmarks.Moqs
{
    public class TestClass : INotifyPropertyChanged, IViewFor<TestClass>
    {
        private static readonly PropertyInfo _childPropertyInfo = typeof(TestClass).GetProperty("Child");
        private static readonly PropertyInfo _valuePropertyInfo = typeof(TestClass).GetProperty("Value");

        private static readonly ConcurrentDictionary<int, Expression<Func<TestClass, int>>> _getValueExpression
            = new ConcurrentDictionary<int, Expression<Func<TestClass, int>>>();
        private TestClass _child;
        private int _value;

        public readonly int Height;

        public TestClass(int height = 1)
        {
            if (height < 1) height = 1;
            Height = height;
            if (height > 1) Child = new TestClass(height - 1);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TestClass Child
        {
            get => _child;
            set => RaiseAndSetIfChanged(ref _child, value);
        }

        public int Value
        {
            get => _value;
            set => RaiseAndSetIfChanged(ref _value, value);
        }

        public void Mutate(int depth = 0)
        {
            if (depth >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(depth));
            }

            var height = Height;
            var current = this;
            while (--height > depth)
            {
                current = current?.Child;
            }

            if (height < 1 && current != null)
            {
                // We're at the bottom, so tweak the value
                current.Value++;
                return;
            }

            // Create a new child hierarchy from this depth.
            if (current != null)
            {
                current.Child = new TestClass(height);
            }
        }

        public static Expression<Func<TestClass, int>> GetValuePropertyExpression(int depth)
            => _getValueExpression.GetOrAdd(depth, d =>
            {
                if (_childPropertyInfo is null)
                {
                    throw new InvalidOperationException(nameof(_childPropertyInfo));
                }

                if (_valuePropertyInfo is null)
                {
                    throw new InvalidOperationException(nameof(_valuePropertyInfo));
                }

                var parameter = Expression.Parameter(typeof(TestClass), "x");

                var pe = parameter;
                Expression body = pe;
                while (d-- > 1)
                {
                    body = Expression.Property(body, _childPropertyInfo);
                }

                body = Expression.Property(body, _valuePropertyInfo);

                return Expression.Lambda<Func<TestClass, int>>(body, parameter);
            });

        protected void RaiseAndSetIfChanged<T>(ref T fieldValue, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(fieldValue, value))
            {
                return;
            }

            fieldValue = value;
            OnPropertyChanged(propertyName);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <inheritdoc />
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TestClass)value;
        }

        /// <inheritdoc />
        public TestClass ViewModel { get; set; }
    }
}