# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### 🚀 New features
- Added `ReadOnlyMemoryExtensions.Occurrences(ReadOnlyMemory<T> search)` extension method.
- Added `ReadOnlyMemoryExtensions.FirstOccurrence(ReadOnlyMemory<T> search, IEqualityComparer<T>)` extension method.
- Added `ReadOnlyMemoryExtensions.FirstOccurrence<T>(ReadOnlyMemory<T> search, IEqualityComparer<T>)` extension method.
- Added `ReadOnlyMemoryExtensions.LastOccurrence<T>(ReadOnlyMemory<T> search, IEqualityComparer<T>)` extension method.
- Added `ReadOnlyMemoryExtensions.Split(ReadOnlySpan<T> search)` extension method.

### 🚨 Breaking changes
- Dropped `netstandard1.0`, `netstandard1.1` and `net6.0` support ([#272](https://github.com/candoumbe/MiscUtilities/issues/272))
  - Removed `DateOnlyJsonConverter`, `TimeOnlyJsonConverter`
  - Removed `T[] Enum.GetValues<T>()` extension method

### 🧹 Housekeeping
- Removed `Nuke.Common` dependency
- Updated `Candoumbe.Pipelines` to `0.12.1`
- Updated `GitVersion.tool` to `6.0.5`
- Updated `Nuke.GlobalTool` to `9.0.1`

## [0.13.1] / 2024-10-05
### 🔧 Fixes
- `StringExtensions.FirstOccurrence(string source, string search)` now returns `0` instead of throwing `ArgumentOutOfRangeException` when `source`
is not null and  ̀search` is empty.
- `StringExtensions.LastOccurrence(string source, string search)` now returns `0` when both `source` and  `search` are empty.
- `StringSegmentExtensions.FirstOccurrence(StringSegment source, StringSegment search)` now returns `0` 
instead of throwing `ArgumentOutOfRangeException` when both `source` and  `search` are equal to `StringSegment.Empty`
- `StringSegmentExtensions.LastOccurrence(StringSegment source, StringSegment search)` now returns `0`
instead of throwing `ArgumentOutOfRangeException` when both `source` and  `search` are equal to `StringSegment.Empty`


## [0.13.0] / 2024-07-06
### 🚨 Breaking changes
- Dropped `net7.0` support

### 🚀 New features
- Added `net8.0` support

### 🔧 Fixes
- Fixed `object.As<T>()` extension method to work the same way `as` keyword does.

### 🧹 Housekeeping
- Add Codium PR agent
- Bumped `Candoumbe.Pipelines` to 0.9.0
- Replaced constructors with primary constructor wherever applicable
- Removed `Format` step from build pipeline

## [0.11.0] / 2023-02-01
### 🚨 Breaking changes
- Removed `DateOnlyRange` type
- Removed `TimeOnlyRange` type
- Removed `DateTimeRange` type
- Removed `Range<T>` type
- Removed `MultiTimeOnlyRange<T>` type
- Removed `MultiDateOnlyRange<T>` type
- Dropped `netcoreapp3.1` support
- Dropped `net5.0` support


## [0.10.0] / 2022-10-16
### 🚀 New features
- Added [`DateOnlyRange`](./src/Candoumbe.MiscUtilities/Types/DateOnlyRange.cs) type
- Added [`TimeOnlyRange`](./src/Candoumbe.MiscUtilities/Types/TimeOnlyRange.cs) type
- Added [`DateTimeRange`](./src/Candoumbe.MiscUtilities/Types/DateTimeRange.cs) type
- Added generic [`Range<T>`](./src/Candoumbe.MiscUtilities/Types/Range.cs) type
- Added [`MultiTimeOnlyRange<T>`](./src/Candoumbe.MiscUtilities/Types/MultiTimeOnlyTimeRange.cs) type
- Added [`MultiDateOnlyRange<T>`](./src/Candoumbe.MiscUtilities/Types/MultiDateOnlyTimeRange.cs) type
- Added `StringExtensions.ToTitleCase` string extension overload to use `CultureInfo`  when runtime is not `netstandard1.0` or `netstandard1.1`.

## [0.8.2] / 2022-07-17
- Updated `Newtonsoft.Json` to 13.0.1 to avoid potential DoS attack ([more details](https://github.com/candoumbe/MiscUtilities/security/dependabot/1))

## [0.8.1] / 2022-07-16
- Fixed validating an email pattern using `Like` extension method. 

## [0.8.0] / 2022-01-26
- Added `SortBy` extension method for `IEnumerable<T>` type
- Added `IShuffler` interface and `FisherYatesShuffler` implementation

## [0.7.0] / 2022-01-11
- Added `EnumExtensions.GetValues<TEnum>()` utility method
- Added `ToArray<T>()` extension methods for `Array` type

## [0.6.3] / 2021-11-11
- Fixed `NotSupportedException` thrown when calling `Jsonify` extension method with an object that has a `DateOnly` / `TimeOnly` property ([#30](https://github.com/candoumbe/MiscUtilities/30))

## [0.6.2] / 2021-11-11
- Fixed `NotSupportedException` thrown when calling `DeepClone` extension method with an object that has a `DateOnly` / `TimeOnly` property ([#30](https://github.com/candoumbe/MiscUtilities/30))

## [0.6.1] / 2021-11-10
- Fixed minor build pipeline issues

## [0.6.0] / 2021-11-10
- Added `TimeOnly` support when calling `ToQueryString` extension methods
- Added `DateOnly` support when calling `ToQueryString` extension methods

## [0.5.4] / 2021-05-30
- Fixed `IsAssignaleToGenericType` not working with inheritance class hierarchy ([#8](https://github.com/candoumbe/MiscUtilities/issues/8))

## [0.5.3] / 2021-05-22
- Fixed `ToQueryString` extension method to convert `DateTime` instance according to value set for [`Kind`] property.
- Fixed missing documentation
- Changed license to Apache 2.0 ([#6](https://github.com/candoumbe/MiscUtilities/issues/6)

## [0.5.2] / 2021-05-01
- `ToQueryString` properly handles converting to string values of type that have a `TypeConverter` ([#5](https://github.com/candoumbe/MiscUtilities/issues/5))

## [0.5.1] / 2021-04-30
- Fixes calling `ToQueryString` on `RouteValueDictionary` types ([#3](https://github.com/candoumbe/MiscUtilities/issues/3)).

## [0.5.0] / 2021-04-29
- Introduces `ToPascalCase` extension method ([#2](https://github.com/candoumbe/MiscUtilities/issues/2))
- Introduces GPLv3 licence

## [0.4.0] / 2021-04-17
- Removes [Rune](https://docs.microsoft.com/en-us/dotnet/api/system.text.rune) tests

## [0.3.1] / 2021-04-12
- Fixes build issue

## [0.3.0] / 2021-04-12
- Added `NET5.0` support
- Improved support for non latin string when using string extension methods
- Added new overload for `ToQueryString`

## [0.2.0] / 2021-01-29
- Changed syntax to target sub-properties from `prop.subproperty` to `prop["subproperty"]`
- Added `Partition(int bucketSize)` extension method

## [0.1.0] / 2020-12-10
- Fixed `Slugify` to take into account characters like `/`
- Fixed `ToSnakeCase` to take into account characters like `-`
- Made [ReplaceVisitor](./src/Candoumbe.MiscUtilities/ReplaceVisitor.cs) public

[Unreleased]: https://github.com/candoumbe/MiscUtilities/compare/0.14.0...HEAD
[0.14.0]: https://github.com/candoumbe/MiscUtilities/compare/0.13.1...0.14.0
[0.13.1]: https://github.com/candoumbe/MiscUtilities/compare/0.13.0...0.13.1
[0.13.0]: https://github.com/candoumbe/MiscUtilities/compare/0.11.0...0.13.0
[0.11.0]: https://github.com/candoumbe/MiscUtilities/compare/0.10.0...0.11.0
[0.10.0]: https://github.com/candoumbe/MiscUtilities/compare/0.8.2...0.10.0
[0.8.2]: https://github.com/candoumbe/MiscUtilities/compare/0.8.1...0.8.2
[0.8.1]: https://github.com/candoumbe/MiscUtilities/compare/0.8.0...0.8.1
[0.8.0]: https://github.com/candoumbe/MiscUtilities/compare/0.7.0...0.8.0
[0.7.0]: https://github.com/candoumbe/MiscUtilities/compare/0.6.3...0.7.0
[0.6.3]: https://github.com/candoumbe/MiscUtilities/compare/0.6.2...0.6.3
[0.6.2]: https://github.com/candoumbe/MiscUtilities/compare/0.6.1...0.6.2
[0.6.1]: https://github.com/candoumbe/MiscUtilities/compare/0.6.0...0.6.1
[0.6.0]: https://github.com/candoumbe/MiscUtilities/compare/0.5.4...0.6.0
[0.5.4]: https://github.com/candoumbe/MiscUtilities/compare/0.5.3...0.5.4
[0.5.3]: https://github.com/candoumbe/MiscUtilities/compare/0.5.2...0.5.3
[0.5.2]: https://github.com/candoumbe/MiscUtilities/compare/0.5.1...0.5.2
[0.5.1]: https://github.com/candoumbe/MiscUtilities/compare/0.5.0...0.5.1
[0.5.0]: https://github.com/candoumbe/MiscUtilities/compare/0.4.0...0.5.0
[0.4.0]: https://github.com/candoumbe/MiscUtilities/compare/0.3.1...0.4.0
[0.3.1]: https://github.com/candoumbe/MiscUtilities/compare/0.3.0...0.3.1
[0.3.0]: https://github.com/candoumbe/MiscUtilities/compare/0.2.0...0.3.0
[0.2.0]: https://github.com/candoumbe/MiscUtilities/compare/0.1.0...0.2.0
[0.1.0]: https://github.com/candoumbe/MiscUtilities/tree/0.1.0
