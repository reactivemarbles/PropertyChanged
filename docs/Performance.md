# Performance Notes

The following explains the performance analysis methodology, and results in detail.

## Benchmarks

The Benchmarks are generated using the [ReactiveMarbles.PropertyChanged.Benchmarks project](/src/ReactiveMarbles.PropertyChanged.Benchmarks).  They are currently 2 main benchmarks -
1. [Property Change Benchmarks](/src/ReactiveMarbles.PropertyChanged.Benchmarks/PropertyChangesBenchmarks.cs) - which test the performance of the [`WhenPropertyValueChanges`](/src/ReactiveMarbles.PropertyChanged/NotifyPropertyChangedExtensions.cs) extension methods which are equivalent (but not yet identical to) the [`WhenAnyValue`](https://github.com/reactiveui/ReactiveUI/blob/main/src/ReactiveUI/VariadicTemplates.tt) extension methods from [ReactiveUI](https://github.com/reactiveui/ReactiveUI).
2. [Binding Benchmarks](/src/ReactiveMarbles.PropertyChanged.Benchmarks/BindBenchmarks.cs) - which test the performance of the new [`Bind`](/src/ReactiveMarbles.PropertyChanged/BindExtensions.cs) extension methods which are equivalent (but not yet identical to) the [`Bind`](https://github.com/reactiveui/ReactiveUI/blob/main/src/ReactiveUI/Bindings/Property/PropertyBindingMixins.cs) extension methods from [ReactiveUI](https://github.com/reactiveui/ReactiveUI).

Both of these benchmarks are parameterised with the following:
1. `Depth` - The depth indicates how deep the reference being monitored goes.  A `Depth` of `1`, means a the property expression is equiaveltn to `x => x.Value`, whereas a `Depth` of `3`, would be equivalent to `x => x.Child.Child.Value`.  For the [Property Change Benchmarks](/src/ReactiveMarbles.PropertyChanged.Benchmarks/PropertyChangesBenchmarks.cs), it indicates the depth of property bound to, however for the [Binding Benchmarks](/src/ReactiveMarbles.PropertyChanged.Benchmarks/BindBenchmarks.cs) it indicates the depth of _both_ the source _and_ the destination of the binding, e.g. a `Depth` of `3` would be equivalent to `x => x.Child.Child.Value` being bound to `y => y.Child.Child.Value`.  As such the _complexity_ of a binding grows exponentially with depth.
2. `Changes` - This indicates the number of mutations to a property are performed sequentially for the test.
3. **Including Binding/Subscription** - (_This is technically not a parameterisation, but a 'category'_)  The tests are performed including the initial binding/subscription and the subsequent disposal, and again with only the changes.  This allows comparison between the 'setup' cost of creating the subscription/binding, and the impact on actual performance of the binding thereafter.
4. **UI vs Old vs New** - (_This is technically not a parameterisation, but actual unique sub-tests_)  The tests each have a suffix of one of the following:
  * `UI` - indicating the original [ReactiveUI](https://github.com/reactiveui/ReactiveUI) method.
  * `Old` _(Optional)_ - indicating the previous version of this code base, which is included in the [`Legacy` subfolder of the Benchmark project](/src/ReactiveMarbles.PropertyChanged.Benchmarks/Legacy).  This allows for the latest changes to be compared to previous releases. (**Note to implementors**:  It is a good idea to update the folder with the existing codebase before making changes to allow for easier comparison.)
  * `New` - indicating the latest codebase.

### Notes on property mutation
Each change is not equivalent.  In the case of the [Binding Benchmarks](/src/ReactiveMarbles.PropertyChanged.Benchmarks/BindBenchmarks.cs), changes alternate between mutating the source and the destination binding, demonstrating the 2-way nature of a bind.  The [Property Change Benchmarks](/src/ReactiveMarbles.PropertyChanged.Benchmarks/PropertyChangesBenchmarks.cs) only mutate the property that is bound to.

Both benchmarks then go from a depth of 0 to the maximum depth (-1) and repeat.  That means, on the `Depth=3` tests, they first change `x=>x.Child` and `x=>x.Child.Child`; then on the next step change `x=>x.Child.Child`; and then finally change `x=>x.Child.Child.Value` on the final step.  After which they repeat.

For this reason, the `Changes=1, Depth=3` tests always perform the most complex mutation by changing both `x=>x.Child` and `x=>x.Child.Child`.  For higher values of `Changes` and `Depth=3` each style accounts for 1/3rd of the changes.

### Notes on including Binding/Subscription
Due to the nature of the tests, and the framework, these are really only testing the speed of accessing the cache, rather than the initial binding cost.  It is not immediately obvious that that is the case but is the main reason for the inclusion of the [`_actionCache`](/src/ReactiveMarbles.PropertyChanged/ExpressionExtensions.cs#L15-L16).

Without that cache, the performance tests would not be equivalent due to the heavy amount of caching in the existing implementations.  In general the metrics that include the Binding/Subscription are therefore of limited interest, in that they allow some indication of the point at which the front-loading of cost is outweighed by the overall performance of changes; but they are not definitive whilst the caching is in place.

Instead, the tests showing the performance of the changes alone are much more informative and show the overhead of any codebase whilst it is running (after initial setup).

## Latest raw benchmark data

The latest raw data can be found at:

* [Property Change Benchmarks](ReactiveMarbles.PropertyChanged.Benchmarks.PropertyChangesBenchmarks-report-github.md)
* [Binding Benchmarks](ReactiveMarbles.PropertyChanged.Benchmarks.BindBenchmarks-report-github.md)

## Analysis


### Property Change Benchmarks Summary

The benchmarks demonstrate that the new library is quicker for both subscription and change.  In the most common use case (`Depth=1`), where the property expression is not 'nested' (i.e. `x=>x.Value`), the updates take ~13% of the time of the existing implementation (>7x faster).  As nesting increase, the gains decrease to ~69% (45% faster) at `Depth=2` and ~79% (27% faster) at `Depth=3`; however most binding doesn't occur at depth.

The latest changes are either slightly better or within the margin or error, which isn't suprising as the only change was to not do a cache lookup whenever a parent property changed (so would only effect `Depth>1` anyway).


### Binding Benchmarks Summary

This release is the first to add benchmarks for binding, which is a good addition as the existing `Old` implemntation was actually consistently slower for changes (between 2-4x slower at `Depth=1` decreasing to ~80% slower at `Depth=3`!).

The latest `New` changes, reverse this trend for a `Depth=1` where they take ~40% of the time (2.5x faster) of the existing `UI` Bind methods.  However at `Depth=2` they are ~10-20% slower, and by `Depth=3` they are ~40% slower.

### Interpretation

The [`WhenPropertyValueChanges`](/src/ReactiveMarbles.PropertyChanged/NotifyPropertyChangedExtensions.cs) extension methods represent a signficant improvement to the existing [`WhenAnyValue`](https://github.com/reactiveui/ReactiveUI/blob/main/src/ReactiveUI/VariadicTemplates.tt) extension methods.  There is reason to be optimistic that these benefits can be carried forward when support for dependency properties is added.

The latest `New` changes offer a significant improvement to `Bind` performance of the existing `Old` changes (which represent a regression compared to the `UI` methods), and an improvement to the `UI` `Bind` extension methods when the `Depth=1`.  

More work needs to be undertaken to understand why the performance lead is overturned when the depth is increased; however, it is anticipated that a `Depth=1` represents the most common scenario.  Further, the metric changes parent property values 2/3rds of the time, separate testing has confirmed that, where only the tail is changed (e.g. `x.Child.Child.Value`) the performance gains closely match the `Depth=1` result (which isn't so surprising).  As deep bindings are comparitvely rare, and changing parent values rarer still, it is likely that the real world performance will still exceed that of the existing `UI` implementation.