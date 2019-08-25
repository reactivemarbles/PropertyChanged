// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.PropertyChanged.Benchmarks.Moqs
{
    internal class B : BaseTestClass
    {
        private C _c;

        public C C
        {
            get => _c;
            set => RaiseAndSetIfChanged(ref _c, value);
        }
    }
}