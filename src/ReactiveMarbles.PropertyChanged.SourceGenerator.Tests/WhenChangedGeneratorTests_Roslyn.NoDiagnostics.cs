﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Xunit.Abstractions;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    /// <summary>
    /// WhenChanged tests for the Roslyn implementation.
    /// </summary>
    public class WhenChangedGeneratorTests_Roslyn : WhenChangedGeneratorTestBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WhenChangedGeneratorTests_Roslyn"/> class.
        /// </summary>
        /// <param name="testOutputHelper">The logger provided by xUnit.</param>
        public WhenChangedGeneratorTests_Roslyn(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper, true)
        {
        }
    }
}