// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Sample
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var myClass = new SampleClass();
            var myClass2 = new OtherNamespace.SampleClass();
            var myClass3 = new SampleClass2();

            myClass.WhenChanged(x => x.MyString).Where(x => x == "Hello World").Select(x => x[10..]).Subscribe(Console.WriteLine);
            myClass2.WhenChanged(x => x.MyString).Subscribe(Console.WriteLine);

            Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Take(5)
                .Subscribe(x => myClass.MyString = x.ToString());

            Console.ReadLine();

            myClass.Bind(myClass3, x => x.MyString, x => x.MyString);
        }

        private static Expression<Func<SampleClass, string>> GetExpression() => x => x.MyString;
    }
}
