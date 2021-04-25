// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Sample
{
    /// <summary>
    /// Dummy.
    /// </summary>
    public partial class SampleClass
    {
        private PrivateClass GetClass() => throw new NotImplementedException();
    }

    /// <summary>
    /// Dummy.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Readability", "RCS1018:Add accessibility modifiers.", Justification = "Because")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1400:Access modifier should be declared", Justification = "Because")]
    public partial class SampleClass : INotifyPropertyChanged
    {
        private OtherNamespace.SampleClass _myClass;
        private string _myString;

        internal SampleClass()
        {
#pragma warning disable SX1101 // Do not prefix local calls with 'this.'
            this.WhenChanged(x => x.MyClass);
#pragma warning restore SX1101 // Do not prefix local calls with 'this.'
        }

        /// <summary>
        /// Dummy.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets a string.
        /// </summary>
        internal string MyString
        {
            get => _myString;

            set
            {
                _myString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MyString)));
            }
        }

        /// <summary>
        /// Gets or sets a class.
        /// </summary>
        internal OtherNamespace.SampleClass MyClass
        {
            get => _myClass;

            set
            {
                _myClass = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MyClass)));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "OK")]
        internal System.IObservable<OtherNamespace.SampleClass> WhenChanged(System.Linq.Expressions.Expression<System.Func<Sample.SampleClass, OtherNamespace.SampleClass>> propertyExpression) =>
            System.Reactive.Linq.Observable.Return(this)
                .Where(x => x != null)
                .Select(x => GenerateObservable(x, "MyClass", y => y.MyClass))
                .Switch();

        private static System.IObservable<T> GenerateObservable<TObj, T>(
        TObj parent,
        string memberName,
        System.Func<TObj, T> getter)
    where TObj : INotifyPropertyChanged =>
            Observable.Create<T>(
                    observer =>
                    {
                        PropertyChangedEventHandler handler = (object sender, PropertyChangedEventArgs e) =>
                        {
                            if (e.PropertyName == memberName)
                            {
                                observer.OnNext(getter(parent));
                            }
                        };

                        parent.PropertyChanged += handler;

                        return System.Reactive.Disposables.Disposable.Create((parent, handler), x => x.parent.PropertyChanged -= x.handler);
                    })
                .StartWith(getter(parent));

        private class PrivateClass
        {
        }
    }
}
