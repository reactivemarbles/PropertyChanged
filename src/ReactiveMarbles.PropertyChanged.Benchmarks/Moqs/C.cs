// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.PropertyChanged.Benchmarks.Moqs
{
    internal class C : BaseTestClass
    {
        private string _test;

        public string Test
        {
            get => _test;
            set => RaiseAndSetIfChanged(ref _test, value);
        }
    }
}