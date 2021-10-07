﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Sample
{
    /// <summary>
    /// Second sample class.
    /// </summary>
    public class SampleClass2 : INotifyPropertyChanged
    {
        private OtherNamespace.SampleClass _myClass;
        private string _myString;
        private string _myString2;
        private string _myString3;

        internal SampleClass2() =>
#pragma warning disable SX1101 // Do not prefix local calls with 'this.'
            this.WhenChanged(x => x.MyClass);
#pragma warning restore SX1101 // Do not prefix local calls with 'this.'

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
                PropertyChanged?.Invoke(this, new(nameof(MyString)));
            }
        }

        /// <summary>
        /// Gets or sets a string.
        /// </summary>
        internal string MyString2
        {
            get => _myString2;

            set
            {
                _myString2 = value;
                PropertyChanged?.Invoke(this, new(nameof(MyString2)));
            }
        }

        /// <summary>
        /// Gets or sets a string.
        /// </summary>
        internal string MyString3
        {
            get => _myString3;

            set
            {
                _myString3 = value;
                PropertyChanged?.Invoke(this, new(nameof(MyString3)));
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
                PropertyChanged?.Invoke(this, new(nameof(MyClass)));
            }
        }
    }
}
