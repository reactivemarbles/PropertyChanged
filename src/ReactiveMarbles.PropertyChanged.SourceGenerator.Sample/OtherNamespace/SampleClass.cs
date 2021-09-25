﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Sample.OtherNamespace
{
    /// <summary>
    /// Dummy.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Readability", "RCS1018:Add accessibility modifiers.", Justification = "Because")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1400:Access modifier should be declared", Justification = "Because")]
    public class SampleClass : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private Sample.SampleClass _myClass;
        private string _myString;

        internal SampleClass()
        {
        }

        /// <summary>
        /// Dummy.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Dummy.
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// Gets or sets a string.
        /// </summary>
        internal string MyString
        {
            get => _myString;

            set
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(MyString)));
                _myString = value;
                PropertyChanged?.Invoke(this, new(nameof(MyString)));
            }
        }

        /// <summary>
        /// Gets or sets a class.
        /// </summary>
        internal Sample.SampleClass MyClass
        {
            get => _myClass;

            set
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(MyClass)));
                _myClass = value;
                PropertyChanged?.Invoke(this, new(nameof(MyClass)));
            }
        }
    }
}
