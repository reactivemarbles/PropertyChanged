// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

using Xunit;
using Xunit.Abstractions;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    /// <summary>
    /// Bind generator tests.
    /// </summary>
    public partial class BindGeneratorTests : IAsyncLifetime
    {
        private readonly CompilationUtil _compilationUtil;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindGeneratorTests"/> class.
        /// </summary>
        /// <param name="testContext">The test context.</param>
        public BindGeneratorTests(ITestOutputHelper testContext)
        {
            TestContext = testContext;
            _compilationUtil = new CompilationUtil(x => testContext.WriteLine(x));
        }

        /// <summary>
        /// Gets the test context.
        /// </summary>
        public ITestOutputHelper TestContext { get; }

        /// <inheritdoc/>
        public Task DisposeAsync() => Task.CompletedTask;

        /// <inheritdoc/>
        public Task InitializeAsync() => _compilationUtil.Initialize();
    }
}
