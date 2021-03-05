// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveMarbles.PropertyChanged.Tests.Moqs;
using Xunit;

namespace ReactiveMarbles.PropertyChanged.Tests
{
    public class WhenPropertyChangesTest
    {
        [Fact]
        public void NestedPropertyValueChangedWork()
        {
            var a = new A();
            var b = new B();
            a.B = b;
            var c = new C();
            b.C = c;

            string testValue = "ignore";
            a.WhenChanged(x => x.B.C.Test).Subscribe(x => testValue = x);
            Assert.Null(testValue);

            c.Test = "Hello World";

            Assert.Equal("Hello World", testValue);

            a.B = new B() { C = new C() };

            Assert.Null(testValue);
        }

        [Fact]
        public void PropertyValueChangedWork()
        {
            var c = new C();
            string testValue = "ignore";
            c.WhenChanged(x => x.Test).Subscribe(x => testValue = x);

            Assert.Null(testValue);
            c.Test = "test";
            Assert.Equal("test", testValue);

            c.Test = null;
            Assert.Null(testValue);
        }
    }
}