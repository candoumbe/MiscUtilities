// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Candoumbe.MiscUtilities.Comparers;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Primitives;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Candoumbe.MiscUtilities.UnitTests
{
    [UnitTest]
    public class ReadOnlyMemoryExtensionsTests(ITestOutputHelper outputHelper)
    {
        private readonly ITestOutputHelper _outputHelper = outputHelper;

        public static TheoryData<ReadOnlyMemory<char>, ReadOnlyMemory<char>, Expression<Func<IEnumerable<int>, bool>>, string> OccurrencesCases
            => new()
            {
                {
                    new StringSegment("Firstname"),
                    new StringSegment("z"),
                    occurrences => occurrences != null && !occurrences.Any(),
                    "There's no occurrence of the search element in the string"
                },
                {
                    StringSegment.Empty, new StringSegment("z"),
                    (occurrences => occurrences != null && !occurrences.Any()),
                    "The source element is empty"
                },
                {
                    new StringSegment("Firstname"),
                    new StringSegment("F"),
                    (occurrences => occurrences != null
                                    && occurrences.Exactly(1)
                                    && occurrences.Once(pos => pos == 0)
                    ),
                    "There is one occurrence of the search element in the string"
                },
                {
                    new StringSegment("zsasz"),
                    new StringSegment("z"),
                    occurrences => occurrences != null
                        && occurrences.Exactly(2)
                        && occurrences.Once(pos => pos == 0)
                        && occurrences.Once(pos => pos == 4)
                    ,
                    "There is 2 occurrences of the search element in the string"
                }
            };

        [Theory]
        [MemberData(nameof(OccurrencesCases))]
        public void Occurrences(ReadOnlyMemory<char> source, ReadOnlyMemory<char> search, Expression<Func<IEnumerable<int>, bool>> expectation, string reason)
        {
            _outputHelper.WriteLine($"Source : '{source.Span}'");
            _outputHelper.WriteLine($"Search : '{search}'");

            // Act
            IEnumerable<int> occurrences = source.Occurrences(search);

            _outputHelper.WriteLine($"Result : {occurrences.Jsonify()}");

            // Assert
            occurrences.Should()
                       .Match(expectation, reason);
        }

        public static TheoryData<ReadOnlyMemory<char>, char, Expression<Func<IEnumerable<int>, bool>>, string> OccurrencesWithCharCases
            => new()
            {
                {
                    new StringSegment("Firstname"),
                    'z',
                    occurrences => occurrences != null && occurrences.None(),
                     "There's no occurrence of the search element in the string"
                },
                {
                    StringSegment.Empty,
                    'z',
                    occurrences => occurrences != null && occurrences.None(),
                    "There source is empty"
                },
                {
                    new StringSegment("Firstname"),
                    'F',
                    occurrences => occurrences != null
                        && occurrences.Exactly(1)
                        && occurrences.Once(pos  => pos == 0),
                    "There is one occurrence of the search element in the string"
                },
                {
                    new StringSegment("zsasz"),
                    'z',
                    occurrences => occurrences != null && occurrences.Exactly(2)
                        && occurrences.Once(pos => pos == 0)
                        && occurrences.Once(pos  =>pos == 4),
                    "There is 2 occurrences of the search element in the string"
                }
        };

        [Theory]
        [MemberData(nameof(OccurrencesWithCharCases))]
        public void Occurrences_of_char(ReadOnlyMemory<char> source, char search, Expression<Func<IEnumerable<int>, bool>> expectation, string reason)
        {
            _outputHelper.WriteLine($"Source : '{source.Span}'");
            _outputHelper.WriteLine($"Search : '{search}'");

            // Act
            IEnumerable<int> occurrences = source.Occurrences(search);

            _outputHelper.WriteLine($"Result : {occurrences.Jsonify()}");

            // Assert
            occurrences.Should()
                       .Match(expectation, reason);
        }

        public static TheoryData<ReadOnlyMemory<char>, Func<char, bool>, Expression<Func<IEnumerable<int>, bool>>, string> OccurrencesWithPredicateCases
            => new()
            {
                {
                    new StringSegment("Firstname"),
                    chr => chr == 'z',
                    occurrences => occurrences != null && occurrences.None(),
                    "There's no occurrence of the search element in the string"
                },
                {
                    StringSegment.Empty,
                    chr => chr == 'z',
                    occurrences => occurrences != null && occurrences.None(),
                    "There source is empty"
                },
                {
                    new StringSegment("Firstname"),
                    chr => chr == 'F',
                    occurrences => occurrences != null
                                   && occurrences.Exactly(1)
                                   && occurrences.Once(pos  => pos == 0),
                    "There is one occurrence of the search element in the string"
                },
                {
                    new StringSegment("zsasz"),
                    chr => chr == 'z',
                    occurrences => occurrences != null && occurrences.Exactly(2)
                                                       && occurrences.Once(pos => pos == 0)
                                                       && occurrences.Once(pos  =>pos == 4),
                    "There is 2 occurrences of the search element in the string"
                }
            };

        [Theory]
        [MemberData(nameof(OccurrencesWithPredicateCases))]
        public void Occurrences_with_predicate(ReadOnlyMemory<char> source, Func<char, bool> search, Expression<Func<IEnumerable<int>, bool>> expectation, string reason)
        {
            _outputHelper.WriteLine($"Source : '{source.Span}'");
            _outputHelper.WriteLine($"Search : '{search}'");

            // Act
            IEnumerable<int> occurrences = source.Occurrences(search);

            _outputHelper.WriteLine($"Result : {occurrences.Jsonify()}");

            // Assert
            occurrences.Should()
                .Match(expectation, reason);
        }

        public static TheoryData<ReadOnlyMemory<char>, Func<char, bool>, int, string> FirstOccurrenceWithPredicateCases
            => new()
            {
                {
                    new StringSegment("Firstname"),
                    chr => chr == 'z',
                    -1,
                    "There's no occurrence of the search element in the string"
                },
                {
                    StringSegment.Empty,
                    chr => chr == 'z',
                    -1,
                    "There source is empty"
                },
                {
                    new StringSegment("Firstname"),
                    chr => chr == 'F',
                    0,
                    "There is one occurrence of the search element in the string"
                },
                {
                    new StringSegment("zsasz"),
                    chr => chr is 'z' or 'Z',
                    0,
                    "There is 2 occurrences of the search element in the string"
                }
            };

        [Theory]
        [MemberData(nameof(FirstOccurrenceWithPredicateCases))]
        public void FirstOccurrence_with_predicate(ReadOnlyMemory<char> source, Func<char, bool> search, int expected, string reason)
        {
            _outputHelper.WriteLine($"Source : '{source.Span}'");
            _outputHelper.WriteLine($"Search : '{search}'");

            // Act
            int actual = source.FirstOccurrence(search);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }

        public static TheoryData<ReadOnlyMemory<char>, ReadOnlyMemory<char>, IEqualityComparer<char>, int, string> LastOccurrenceCases
        => new()
            {
                {
                    new StringSegment("Firstname"),
                    new StringSegment("z"),
                    CharComparer.InvariantCultureIgnoreCase,
                    -1,
                    "There's no occurrence of the search element in the string"
                },
                {
                    StringSegment.Empty,
                    new StringSegment("z"),
                    CharComparer.InvariantCultureIgnoreCase,
                    -1,
                    "The source element is empty"
                },
                {
                    new StringSegment("Firstname"),
                    new StringSegment("F"),
                    CharComparer.InvariantCultureIgnoreCase,
                    0,
                    "There is one occurrence of the search element in the string"
                },
                {
                    new StringSegment("zsasz"),
                    new StringSegment("z"),
                    CharComparer.InvariantCultureIgnoreCase,
                    4,
                    "There are 2 occurrences of the search element in the string"
                },
                {
                    new StringSegment("zsasz"),
                    new StringSegment("Z"),
                    CharComparer.InvariantCultureIgnoreCase,
                    4,
                    "There are 2 occurrences of the search element in the string"
                }
            };

        [Theory]
        [MemberData(nameof(LastOccurrenceCases))]
        public void LastOccurrence(ReadOnlyMemory<char> source, ReadOnlyMemory<char> search, IEqualityComparer<char> charComparer, int expectation, string reason)
        {
            // Act
            int actual = source.LastOccurrence(search, charComparer);

            // Assert
            actual.Should()
                  .Be(expectation, reason);
        }

        [Property]
        public void Given_source_is_not_null_When_search_is_empty_Then_LastOccurrence_returns_the_length_of_the_source(NonNull<string> sourceGenerator)
        {
            // Arrange
            StringSegment source = sourceGenerator.Item;

            // Act
            int lastIndex = source.LastOccurrence(StringSegment.Empty);

            // Assert
            _ = source.Length switch
            {
                > 0 => lastIndex.Should().Be(source.Length - 1),
                _ => lastIndex.Should().Be(0),
            };
        }

        public static TheoryData<ReadOnlyMemory<char>, ReadOnlyMemory<char>, IEqualityComparer<char>, int> FirstOccurrenceCases
            => new()
            {
                {
                    new StringSegment("Firstname"),
                    new StringSegment("z"),
                    CharComparer.Ordinal,
                    -1
                },
                {
                    StringSegment.Empty,
                    new StringSegment("z"),
                    CharComparer.Ordinal,
                    -1
                },
                {
                    StringSegment.Empty,
                    StringSegment.Empty,
                    CharComparer.Ordinal,
                    0
                },
                {
                    new StringSegment("Firstname"),
                    new StringSegment("F"),
                    CharComparer.Ordinal,
                    0
                },
                {
                    new StringSegment("zsasz"),
                    new StringSegment("z"),
                    CharComparer.Ordinal,
                    0
                },
                {
                    new StringSegment("zsasz"),
                    new StringSegment("Z"),
                    CharComparer.InvariantCultureIgnoreCase,
                    0
                }
            };

        [Theory]
        [MemberData(nameof(FirstOccurrenceCases))]
        public void FirstOccurrence(ReadOnlyMemory<char> source, ReadOnlyMemory<char> search, IEqualityComparer<char> comparer, int expected)
        {
            // Act
            int actual = source.FirstOccurrence(search, comparer);

            // Assert
            actual.Should()
                  .Be(expected);
        }

        public static TheoryData<ReadOnlyMemory<char>, ReadOnlyMemory<char>, bool> StartsWithCases
            => new()
            {
                {
                    new StringSegment("Firstname"),
                    new StringSegment("z"),
                    false
                },
                {
                    StringSegment.Empty,
                    new StringSegment("z"),
                    false
                },
                {
                    StringSegment.Empty,
                    StringSegment.Empty,
                    true
                },
                {
                    new StringSegment("Firstname"),
                    new StringSegment("F"),
                    true
                },
                {
                    new StringSegment("Firstname"),
                    new StringSegment("First"),
                    true
                },
                {
                    new StringSegment("zsasz"),
                    new StringSegment("z"),
                    true
                },
                {
                    new StringSegment("Firstname"),
                    new StringSegment("Firstname"),
                    true
                },
                {
                    new StringSegment("First"),
                    new StringSegment("Firstname"),
                    false
                }
            };

        [Theory]
        [MemberData(nameof(StartsWithCases))]
        public void StartsWith(ReadOnlyMemory<char> source, ReadOnlyMemory<char> search, bool expected)
        {
            // Act
            bool actual = source.StartsWith(search);

            // Assert
            actual.Should()
                .Be(expected);
        }
    }
}