// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
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
    public class SampleClass2 : INotifyPropertyChanged, INotifyPropertyChanging
    {
        ////private SampleClass3 _myClass = new SampleClass3();
        private string? _myString = string.Empty;
        private string? _myString2 = string.Empty;
        private string? _myString3 = string.Empty;

        internal SampleClass2()
        {
           // this.WhenChanged(x => x.MyClass);
        }

        /// <summary>
        /// Dummy.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Dummy.
        /// </summary>
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>
        /// Gets or sets a string.
        /// </summary>
        internal string? MyString1
        {
            get => _myString;

            set
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(MyString1)));
                _myString = value;
                PropertyChanged?.Invoke(this, new(nameof(MyString1)));
            }
        }

        /// <summary>
        /// Gets or sets a string.
        /// </summary>
        internal string? MyString2
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
        internal string? MyString3
        {
            get => _myString3;

            set
            {
                _myString3 = value;
                PropertyChanged?.Invoke(this, new(nameof(MyString3)));
            }
        }

        // TODO: Fix this.
        /////// <summary>
        /////// Gets or sets a class.
        /////// </summary>
        ////internal SampleClass3 MyClass
        ////{
        ////    get => _myClass;

        ////    set
        ////    {
        ////        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(MyClass)));
        ////        _myClass = value;
        ////        PropertyChanged?.Invoke(this, new(nameof(MyClass)));
        ////    }
        ////}
    }
}
