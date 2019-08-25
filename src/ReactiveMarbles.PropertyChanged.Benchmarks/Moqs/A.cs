// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.PropertyChanged.Benchmarks.Moqs
{
    internal class A : BaseTestClass
    {
        private B _b;

        public B B
        {
            get => _b;
            set => RaiseAndSetIfChanged(ref _b, value);
        }
    }
}