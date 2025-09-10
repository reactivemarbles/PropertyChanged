// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

using ReactiveMarbles.PropertyChanged.Tests.Moqs;

using Xunit;

namespace ReactiveMarbles.PropertyChanged.Tests;

/// <summary>
/// Whens the WhenChanged.
/// </summary>
public class WhenChangedTests
{
    /// <summary>
    /// Checks to make sure that nested property value changes work.
    /// </summary>
    [Fact]
    public void NestedPropertyValueChangedWork()
    {
        var a = new A();
        var b = new B();
        a.B = b;
        var c = new C();
        b.C = c;

        var testValue = "ignore";
        a.WhenChanged(x => x.B.C.Test).Subscribe(x => testValue = x);
        Assert.Null(testValue);

        c.Test = "Hello World";

        Assert.Equal("Hello World", testValue);

        a.B = new() { C = new() };

        Assert.Null(testValue);
    }

    /// <summary>
    /// Checks to make sure that property value changes work.
    /// </summary>
    [Fact]
    public void PropertyValueChangedWork()
    {
        var c = new C();
        var testValue = "ignore";
        c.WhenChanged(x => x.Test).Subscribe(x => testValue = x);

        Assert.Null(testValue);
        c.Test = "test";
        Assert.Equal("test", testValue);

        c.Test = null;
        Assert.Null(testValue);
    }
}