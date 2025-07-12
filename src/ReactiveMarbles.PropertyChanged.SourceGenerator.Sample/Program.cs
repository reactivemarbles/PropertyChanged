// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
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
            var myClass1 = new SampleClass1();
            var myClass2 = new OtherNamespace.SampleClass2();
            var myClass3 = new SampleClass3();

            myClass1.WhenChanged(x => x.MyString1).Where(x => x?.Length > 0).Subscribe(s => Console.WriteLine(s));
            myClass1.WhenChanged(x => x.MyString2).Where(x => x?.Length > 0).Subscribe(s => Console.WriteLine(s));
            myClass1.WhenChanged(x => x.MyString3).Where(x => x?.Length > 0).Subscribe(s => Console.WriteLine(s));

            myClass2.WhenChanging(x => x.MyString1).Subscribe(s => Console.WriteLine(s));
            myClass2.WhenChanging(x => x.MyString2).Subscribe(s => Console.WriteLine(s));
            myClass2.WhenChanging(x => x.MyString3).Subscribe(s => Console.WriteLine(s));

            // TODO: This is not working yet.
            ////myClass1.BindTwoWay(myClass3, x => x.MyString1, x => x.MyString1);
            ////myClass1.BindTwoWay(myClass3, x => x.MyString2, x => x.MyString2);
            ////myClass1.BindTwoWay(myClass3, x => x.MyString3, x => x.MyString3);

            myClass1.BindOneWay(myClass2, x => x.MyString1, x => x.MyString1);
            myClass1.BindOneWay(myClass2, x => x.MyString2, x => x.MyString2);
            myClass1.BindOneWay(myClass2, x => x.MyString3, x => x.MyString3);

            Observable
                .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                .Take(5)
                .Subscribe(x => myClass1.MyString1 = x.ToString());

            Console.ReadLine();
        }

        private static Expression<Func<SampleClass1, string>> GetExpression() => x => x.MyString1;
    }
}
