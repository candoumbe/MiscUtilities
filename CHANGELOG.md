# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
- Changed syntax to target subproperties from `prop.subproperty` to `prop["subproperty"]`
- Added `Partition(int bucketSize)` extension method

## [0.1.0] / 2020-12-10
- Fixed `Slugify` to take into account characters like `/`
- Fixed `ToSnakeCase` to take into account characters like `-`
- Made [ReplaceVisitor](./src/Candoumbe.MiscUtilities/ReplaceVisitor.cs) public

[Unreleased]: https://github.com/candoumbe/MiscUtilities/compare/0.7.0...HEAD
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

