# Reactive Marbles Property Changed

## Packages

| Container | NuGet
|---------|-------|
| [PropertyChanged][PropertyChanged] | [![PropertyChangedBadge]][PropertyChanged]
| [PropertyChanged.SourceGenerator][PropertyChangedSourceGen] | [![PropertyChangedSourceGenBadge]][PropertyChangedSourceGen]

[PropertyChanged]: https://www.nuget.org/packages/ReactiveMarbles.PropertyChanged/
[PropertyChangedBadge]: https://img.shields.io/nuget/v/ReactiveMarbles.PropertyChanged.svg
[PropertyChangedSourceGen]: https://www.nuget.org/packages/ReactiveMarbles.PropertyChanged.SourceGenerator/
[PropertyChangedSourceGenBadge]: https://img.shields.io/nuget/v/ReactiveMarbles.PropertyChanged.SourceGenerator.svg

## Overview 

A framework for providing an observable with the latest value of a property expression.

The source generator version will generate raw source code for the binding. If you have private/protected classes and/or properties it may require partial classes.

The regular version will use Expression trees on platforms that support it (no iOS based platforms). On iOS it will just use reflection. This provides a roughly 2x performance boost for those platforms that can use expression trees.

```cs
this.WhenChanged(x => x.Property1.Property2.Property3);
```

The above will generate a `IObservable<T>` where T is the type of `Property3`. It will signal each time a value has changed. It is aware of all property changes in the property chain.

## Binding

There are several methods of binding.

First is two way binding. Two way binding will update either the `host` or the `target` whenever the target property has changed.

```cs
host.BindTwoWay(target, host => host.B.C, target => target.D.E);
```

One way binding will only update the `target` with changes  the `host`'s specified target property.

```cs
host.BindOneWay(target, host => host.B.C);
```

There are also overloads with lambdas that allow you to convert from the `host` to the `target`. These will allow you to convert at binding time to the specified formats.

```cs
host.BindOneWay(target, host => host.B.C, hostProp => ConvertToTargetPropType(hostProp));
host.BindTwoWay(target, host => host.B.C, target => target.D.E, hostProp => ConvertToTargetPropType(hostProp), targetProp => ConvertToHostPropType(targetProp));
```

# Limitations compared to ReactiveUI

At the moment it only supports `INotifyPropertyChanged` properties. More property types to come such as WPF DependencyProperty.

# Milestones 

* Implement initial binding and property changes.

# Benchmark Comparisons

Detailed benchmarking results can be found [here](/docs/Performance.md).
