// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Xunit.Abstractions;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    /// <summary>
    /// Bind generator tests.
    /// </summary>
    public partial class BindGeneratorTests
    {
        private readonly ITestOutputHelper _testLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindGeneratorTests"/> class.
        /// </summary>
        /// <param name="testLogger">The logger provided by xUnit.</param>
        public BindGeneratorTests(ITestOutputHelper testLogger) => _testLogger = testLogger;
    }
}
