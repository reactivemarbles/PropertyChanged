// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Sample;

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
public partial class SampleClass : INotifyPropertyChanged, INotifyPropertyChanging
{
    private OtherNamespace.SampleClass _myClass;
    private string _myString;
    private string _myString2;
    private string _myString3;

    internal SampleClass() =>
#pragma warning disable SX1101 // Do not prefix local calls with 'this.'
        this.WhenChanged(x => x.MyClass);
#pragma warning restore SX1101 // Do not prefix local calls with 'this.'

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
            PropertyChanging?.Invoke(this, new(nameof(MyString)));
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
            PropertyChanging?.Invoke(this, new(nameof(MyString2)));
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
            PropertyChanging?.Invoke(this, new(nameof(MyString3)));
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

    [SuppressMessage("Design", "CA1812: Never used class", Justification = "Used by Rx")]
    private class PrivateClass
    {
    }
}
