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
        /// <summary>
        /// Initializes a new instance of the <see cref="BindGeneratorTests"/> class.
        /// </summary>
        /// <param name="testContext">The test context.</param>
        public BindGeneratorTests(ITestOutputHelper testContext) => TestContext = testContext;

        /// <summary>
        /// Gets the test context.
        /// </summary>
        public ITestOutputHelper TestContext { get; }
    }
}
