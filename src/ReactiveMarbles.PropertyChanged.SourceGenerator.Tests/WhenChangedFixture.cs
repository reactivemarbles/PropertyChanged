// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests;

internal sealed class WhenChangedFixture : IDisposable
{
    private readonly WhenChangedHostBuilder _hostTypeInfo;
    private readonly WhenChangedHostBuilder _receiverTypeInfo;
    private readonly Action<string> _testLogger;
    private readonly CompilationUtil _compilation;
    private readonly (string FileName, string Source)[] _extraSources;
    private Type _hostType;
    private Type _receiverType;
    private Type _valuePropertyType;

    private WhenChangedFixture(WhenChangedHostBuilder hostTypeInfo, WhenChangedHostBuilder receiverTypeInfo, Action<string> testLogger, (string FileName, string Source)[] extraSources)
    {
        _hostTypeInfo = hostTypeInfo;
        _receiverTypeInfo = receiverTypeInfo;
        _testLogger = testLogger;
        _compilation = new CompilationUtil(x => testLogger?.Invoke(x));
        _extraSources = extraSources;
    }

    public string Sources { get; private set; }

    public static Task<WhenChangedFixture> Create(WhenChangedHostBuilder hostTypeInfo, Action<string> testOutputHelper, params (string FileName, string Source)[] extraSources) =>
        Create(hostTypeInfo, hostTypeInfo, testOutputHelper, extraSources);

    public static async Task<WhenChangedFixture> Create(WhenChangedHostBuilder hostTypeInfo, WhenChangedHostBuilder receiverTypeInfo, Action<string> testOutputHelper, params (string FileName, string Source)[] extraSources)
    {
        var fixture = new WhenChangedFixture(hostTypeInfo, receiverTypeInfo, testOutputHelper, extraSources);
        await fixture.Initialize().ConfigureAwait(false);
        return fixture;
    }

    public Task Initialize() => _compilation.Initialize();

    public void Dispose() => _compilation?.Dispose();

    public void RunGenerator(out ImmutableArray<Diagnostic> compilationDiagnostics, out ImmutableArray<Diagnostic> generatorDiagnostics, bool writeOutput = false, [CallerMemberName] string callerMemberName = null)
    {
        var sources = _extraSources.Concat(new[] { (FileName: "receiver.cs", Source: _receiverTypeInfo.BuildRoot()) });
        if (_receiverTypeInfo != _hostTypeInfo)
        {
            sources = sources.Concat(new[] { (FileName: "hosttype.cs", _hostTypeInfo.BuildRoot()) });
        }

        Compilation afterCompilation = null;
        compilationDiagnostics = default;
        generatorDiagnostics = default;
        try
        {
            _compilation.RunGenerators(out compilationDiagnostics, out generatorDiagnostics, out var beforeCompilation, out afterCompilation, sources.ToArray());
            var assembly = GetAssembly(afterCompilation);
            _hostType = assembly.GetType(_hostTypeInfo.GetTypeName());
            _receiverType = assembly.GetType(_receiverTypeInfo.GetTypeName());
            _valuePropertyType = assembly.GetType(_receiverTypeInfo.ValuePropertyTypeName);
        }
        catch (InvalidOperationException)
        {
            var compilationErrors = compilationDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => x.GetMessage()).ToList();
            Sources = string.Join(Environment.NewLine, afterCompilation.SyntaxTrees.Select(x => x.ToString()).Where(x => !x.Contains("The implementation should have been generated.")));
        }

        if (afterCompilation is not null && writeOutput)
        {
            var directory = Directory.CreateDirectory(Path.Combine("./erroroutput/whenchanged", callerMemberName));
            foreach (var source in afterCompilation.SyntaxTrees)
            {
                var fileName = Path.Combine(directory.FullName, Path.GetFileName(source.FilePath));
                File.WriteAllText(fileName, source.ToString());
            }
        }
    }

    public WhenChangedHostProxy NewHostInstance() => new(CreateInstance(_hostType));

    public WhenChangedHostProxy NewReceiverInstance() => new(CreateInstance(_receiverType));

    public object NewValuePropertyInstance() => CreateInstance(_valuePropertyType);

    private static object CreateInstance(Type type) => Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, null, null) ?? throw new InvalidOperationException("The value of the type cannot be null");

    private static Assembly GetAssembly(Compilation compilation)
    {
        using var ms = new MemoryStream();
        compilation.Emit(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(ms.ToArray());
    }
}
