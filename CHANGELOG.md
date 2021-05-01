# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
- `ToQueryString` properly handles converting to string values of type that have a `TypeConverter` [#5](https://github.com/candoumbe/MiscUtilities/issues/5)
## [0.5.1] / 2021-04-30
- Fixes calling `ToQueryString` on `RouteValueDictionary` types [#3](https://github.com/candoumbe/MiscUtilities/issues/3).

## [0.5.0] / 2021-04-29
- Introduces `ToPascalCase` extension method [(feature #2)](https://github.com/candoumbe/MiscUtilities/issues/2)
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

[Unreleased]: https://github.com/candoumbe/MiscUtilities.git/compare/0.5.1...HEAD
[0.5.1]: https://github.com/candoumbe/MiscUtilities.git/compare/0.5.0...0.5.1
[0.5.0]: https://github.com/candoumbe/MiscUtilities.git/compare/0.4.0...0.5.0
[0.4.0]: https://github.com/candoumbe/MiscUtilities.git/compare/0.3.1...0.4.0
[0.3.1]: https://github.com/candoumbe/MiscUtilities.git/compare/0.3.0...0.3.1
[0.3.0]: https://github.com/candoumbe/MiscUtilities.git/compare/0.2.0...0.3.0
[0.2.0]: https://github.com/candoumbe/MiscUtilities.git/compare/0.1.0...0.2.0
[0.1.0]: https://github.com/candoumbe/MiscUtilities.git/tree/0.1.0

