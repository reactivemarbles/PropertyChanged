[![NuGet](https://img.shields.io/nuget/v/ReactiveMarbles.PropertyChanged.svg?maxAge=2592000)](https://www.nuget.org/packages/ReactiveMarbles.PropertyChanged/)

# Reactive Marbles Property Changed

A framework for providing an observable with the latest value of a property expression.

Will use Expression trees on platforms that support it (no iOS based platforms). On iOS it will just use reflection. This provides a roughly 2x performance boost for those platforms that can use expression trees.

```cs
this.WhenPropertyChanges(x => x.Property1.Property2.Property3);
```

The above will generate a `IObservable<T>` where T is the type of `Property3`. It will signal each time a value has changed. It is aware of all property changes in the property chain.

# Binding

There are several methods of binding.

First is two way binding. Two way binding will update either the `host` or the `target` whenever the target property has changed.

```cs
host.Bind(target, host => host.B.C, target => target.D.E);
```

One way binding will only update the `target` with changes  the `host`'s specified target property.

```cs
host.OneWayBind(target, host => host.B.C);
```

There are also overloads with lambdas that allow you to convert from the `host` to the `target`. These will allow you to convert at binding time to the specified formats.

```cs
host.OneWayBind(target, host => host.B.C, hostProp => ConvertToTargetPropType(hostProp));
host.Bind(target, host => host.B.C, target => target.D.E, hostProp => ConvertToTargetPropType(hostProp), targetProp => ConvertToHostPropType(targetProp));
```

# Limitations compared to ReactiveUI

At the moment it only supports `INotifyPropertyChanged` properties. More property types to come such as WPF DependencyProperty.

# Milestones 

* Implement initial binding and property changes.
* Introduce AOP for allow removal of expressions.

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
