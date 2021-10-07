// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests;

internal static class CommonTest
{
    private static CompilationUtil _compilationUtil;

    private static Func<TestContext, Task> _compilationInitFunc = context =>
    {
        _compilationUtil = new(context.WriteLine);
        return _compilationUtil.Initialize();
    };

    public static CompilationUtil CompilationUtil => _compilationUtil;

    public static async Task<CompilationUtil> Initialize(TestContext testContext)
    {
        var func = Interlocked.Exchange(ref _compilationInitFunc, null);

        if (func is not null)
        {
            await func.Invoke(testContext).ConfigureAwait(false);
        }

        return _compilationUtil;
    }
}
