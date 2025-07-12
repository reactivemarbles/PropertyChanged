// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
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

internal class BindFixture : IDisposable
{
    private readonly BindHostBuilder _hostTypeInfo;
    private readonly CompilationUtil _compilation;
    private readonly Action<string> _testOutputHelper;
    private Type _hostType;
    private Type _viewModelPropertyType;
    private Type _valuePropertyType;
    private bool _disposedValue;

    private BindFixture(BindHostBuilder hostTypeInfo, Action<string> testOutputHelper)
    {
        _hostTypeInfo = hostTypeInfo;
        _testOutputHelper = testOutputHelper;
        _compilation = new(x => _testOutputHelper(x));
    }

    public string Sources { get; private set; } = string.Empty;

    public static async Task<BindFixture> Create(BindHostBuilder hostTypeInfo, Action<string> testOutputHelper)
    {
        var bindFixture = new BindFixture(hostTypeInfo, testOutputHelper);
        await bindFixture.Initialize().ConfigureAwait(false);
        return bindFixture;
    }

    public Task Initialize() => _compilation.Initialize();

    public void RunGenerator(BindHostBuilder hostTypeInfo, out ImmutableArray<Diagnostic> compilationDiagnostics, out ImmutableArray<Diagnostic> generatorDiagnostics, bool writeOutput = false, [CallerMemberName] string callerMemberName = null)
    {
        var sources = new[] { ("HostBuilder.cs", hostTypeInfo.BuildRoot()) };

        Compilation afterCompilation = null;
        compilationDiagnostics = default;
        generatorDiagnostics = default;
        try
        {
            _compilation.RunGenerators(out compilationDiagnostics, out generatorDiagnostics, out var beforeCompilation, out afterCompilation, sources);
            var assembly = GetAssembly(afterCompilation);
            _hostType = assembly.GetType(_hostTypeInfo.GetTypeName());
            _viewModelPropertyType = assembly.GetType(_hostTypeInfo.ViewModelPropertyTypeName);
            _valuePropertyType = assembly.GetType(_hostTypeInfo.PropertyTypeName);
        }
        catch (InvalidOperationException)
        {
            var compilationErrors = compilationDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => x.GetMessage()).ToList();
            Sources = string.Join(Environment.NewLine, afterCompilation.SyntaxTrees.Select(x => x.ToString()).Where(x => !x.Contains("The implementation should have been generated.")));
            if (compilationErrors.Count > 0)
            {
                throw;
            }
        }
        finally
        {
            if (afterCompilation is not null && writeOutput)
            {
                var directory = Directory.CreateDirectory(Path.Combine("./erroroutput/bind", callerMemberName));
                foreach (var source in afterCompilation.SyntaxTrees)
                {
                    var fileName = Path.Combine(directory.FullName, Path.GetFileName(source.FilePath));
                    File.WriteAllText(fileName, source.ToString());
                }
            }
        }
    }

    public BindHostProxy NewHostInstance() => new(CreateInstance(_hostType));

    public WhenChangedHostProxy NewViewModelPropertyInstance() => new(CreateInstance(_viewModelPropertyType));

    public object NewValuePropertyInstance() => CreateInstance(_valuePropertyType);

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _compilation.Dispose();
            }

            _disposedValue = true;
        }
    }

    private static object CreateInstance(Type type) => Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, null, null) ?? throw new InvalidOperationException("The value of the type cannot be null");

    private static Assembly GetAssembly(Compilation compilation)
    {
        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(ms.ToArray());
    }
}
