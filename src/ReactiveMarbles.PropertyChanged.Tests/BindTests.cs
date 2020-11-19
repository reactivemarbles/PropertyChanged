// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveMarbles.PropertyChanged.Tests.Moqs;
using Xunit;

namespace ReactiveMarbles.PropertyChanged.Tests
{
    public class BindTests
    {
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

            a.OneWayBind(bindToC, x => x.B.C.Test, x => x.Test);

            Assert.Equal("start value", bindToC.Test);

            c.Test = "Hello World";

            Assert.Equal("Hello World", bindToC.Test);

            a.B = new B() { C = new C() };

            Assert.Null(bindToC.Test);
        }

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

            a.OneWayBind(bindToC, x => x.B.C, x => x.Test, value => value.Test);

            Assert.Equal("start value", bindToC.Test);

            b.C = new C();

            Assert.Null(bindToC.Test);

            b.C = new C { Test = "blah" };

            Assert.Equal("blah", bindToC.Test);
        }

        [Fact]
        public void TwoWayBindTest()
        {
            var a = new A();
            var b = new B();
            a.B = b;
            var c = new C
            {
                Test = "Host Value"
            };
            b.C = c;

            var bindToC = new C();
            bindToC.Test = "Target value";

            a.Bind(bindToC, x => x.B.C.Test, x => x.Test);

            Assert.Equal("Host Value", bindToC.Test);

            bindToC.Test = "Test2";

            Assert.Equal("Test2", c.Test);

            a.B = new B
            {
                C = new C
                {
                    Test = "Test3"
                }
            };
            Assert.Equal("Test3", bindToC.Test);
        }

        [Fact]
        public void TwoWayBindTestConverter()
        {
            var a = new A();
            var b = new B();
            a.B = b;
            var c = new C();
            c.Test = "Host Value";
            b.C = c;

            var bindToC = new C();
            bindToC.Test = "Target value";

            a.Bind(bindToC, x => x.B.C, x => x.Test, x => x.Test, y => new C() { Test = y });

            Assert.Equal("Host Value", bindToC.Test);

            bindToC.Test = "Test2";

            Assert.Equal("Test2", a.B.C.Test);
        }
    }
}