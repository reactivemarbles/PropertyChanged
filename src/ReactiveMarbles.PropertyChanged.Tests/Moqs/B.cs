// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.PropertyChanged.Tests.Moqs
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