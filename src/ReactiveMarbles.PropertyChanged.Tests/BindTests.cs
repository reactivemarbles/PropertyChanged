// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveMarbles.PropertyChanged.Tests.Moqs;

using Xunit;

namespace ReactiveMarbles.PropertyChanged.Tests;

/// <summary>
/// Tests to make sure that Bind works.
/// </summary>
public class BindTests
{
    /// <summary>
    /// Tests one way binding.
    /// </summary>
    [Fact]
    public void OneWayBindTest()
    {
        var a = new A();
        var b = new B();
        a.B = b;
        var c = new C();
        b.C = c;

        var bindToC = new C();
        c.Test = "start value";

        a.BindOneWay(bindToC, x => x.B.C.Test, x => x.Test);

        Assert.Equal("start value", bindToC.Test);

        c.Test = "Hello World";

        Assert.Equal("Hello World", bindToC.Test);

        a.B = new() { C = new() };

        Assert.Null(bindToC.Test);
    }

    /// <summary>
    /// Tests one way binding with a converter.
    /// </summary>
    [Fact]
    public void OneWayBindWithConverterTest()
    {
        var a = new A();
        var b = new B();
        a.B = b;
        var c = new C();
        b.C = c;

        var bindToC = new C();
        c.Test = "start value";

        a.BindOneWay(bindToC, x => x.B.C, x => x.Test, value => value.Test);

        Assert.Equal("start value", bindToC.Test);

        b.C = new();

        Assert.Null(bindToC.Test);

        b.C = new() { Test = "blah" };

        Assert.Equal("blah", bindToC.Test);
    }

    /// <summary>
    /// Tests two way binding.
    /// </summary>
    [Fact]
    public void TwoWayBindTest()
    {
        var a = new A();
        var b = new B();
        a.B = b;
        var c = new C
        {
            Test = "Host Value",
        };
        b.C = c;

        var bindToC = new C
        {
            Test = "Target value"
        };

        a.BindTwoWay(bindToC, x => x.B.C.Test, x => x.Test);

        Assert.Equal("Host Value", bindToC.Test);

        bindToC.Test = "Test2";

        Assert.Equal("Test2", c.Test);

        a.B = new()
        {
            C = new()
            {
                Test = "Test3",
            },
        };
        Assert.Equal("Test3", bindToC.Test);
    }

    /// <summary>
    /// Tests two way binding with a converter.
    /// </summary>
    [Fact]
    public void TwoWayBindTestConverter()
    {
        var a = new A();
        var b = new B();
        a.B = b;
        var c = new C
        {
            Test = "Host Value"
        };
        b.C = c;

        var bindToC = new C
        {
            Test = "Target value"
        };

        a.BindTwoWay(bindToC, x => x.B.C, x => x.Test, x => x.Test, y => new() { Test = y });

        Assert.Equal("Host Value", bindToC.Test);

        bindToC.Test = "Test2";

        Assert.Equal("Test2", a.B.C.Test);
    }
}