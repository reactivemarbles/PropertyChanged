# Reactive Marbles Property Changed

A framework for providing an observable with the latest value of a property expression.

Will use Expression trees on platforms that support it (no iOS based platforms).

```cs
this.WhenPropertyChanges(x => x.Property1.Property2.Property3);
```

The above will generate a `IObservable<T>` where T is the type of `Property3`. It will signal each time a value has changed. It is aware of all property changes in the property chain.

# Limitations compared to ReactiveUI

At the moment it only supports `INotifyPropertyChanged` properties. More property types to come such as WPF DependencyProperty.

# Benchmark Comparisons

```ini
|                 Method |    N |         Mean |      Error |       StdDev |     Gen 0 | Gen 1 | Gen 2 |  Allocated |
|----------------------- |----- |-------------:|-----------:|-------------:|----------:|------:|------:|-----------:|
| ReactiveMarblesChanges |    1 |     80.61 us |   1.542 us |     1.515 us |    8.9111 |     - |     - |   13.82 KB |
|      ReactiveUIChanges |    1 |    246.01 us |  19.087 us |    55.069 us |         - |     - |     - |   20.38 KB |
| ReactiveMarblesChanges |   10 |    258.15 us |   2.114 us |     1.977 us |   27.3438 |     - |     - |   42.32 KB |
|      ReactiveUIChanges |   10 |    581.21 us |  10.042 us |     8.902 us |   36.1328 |     - |     - |   55.75 KB |
| ReactiveMarblesChanges |  100 |  1,968.07 us |  28.810 us |    25.539 us |  212.8906 |     - |     - |  328.04 KB |
|      ReactiveUIChanges |  100 |  4,754.17 us |  49.251 us |    43.659 us |  281.2500 |     - |     - |  437.24 KB |
| ReactiveMarblesChanges | 1000 | 19,262.79 us | 275.029 us |   257.262 us | 2062.5000 |     - |     - | 3180.23 KB |
|      ReactiveUIChanges | 1000 | 45,432.12 us | 902.420 us | 1,843.404 us | 2000.0000 |     - |     - | 4232.83 KB |
```

The above benchmarks for compares ReactiveUI `WhenAnyValue` to `WhenPropertyChanges`. On every platform apart from iOS/TVOS/WatchOS the new property changes out performs ReactiveUI versions.