// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    public class StringSegmentExtensionsTests(ITestOutputHelper outputHelper)
    {
        private readonly ITestOutputHelper _outputHelper = outputHelper;

        public static TheoryData<StringSegment, StringSegment, Expression<Func<IEnumerable<int>, bool>>, string> OccurrencesCases
            => new()
            {
                {
                    new StringSegment("Firstname"),
                    new StringSegment("z"),
                    (occurrences => occurrences != null && !occurrences.Any()), "There's no occurrence of the search element in the string"
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
                    (occurrences =>
                        occurrences != null
                        && occurrences.Exactly(2)
                        && occurrences.Once(pos => pos == 0)
                        && occurrences.Once(pos => pos == 4)
                    ),
                    "There is 2 occurrences of the search element in the string"
                }
            };

        [Theory]
        [MemberData(nameof(OccurrencesCases))]
        public void Occurrences(StringSegment source, StringSegment search, Expression<Func<IEnumerable<int>, bool>> expectation, string reason)
        {
            _outputHelper.WriteLine($"Source : '{source.Value}'");
            _outputHelper.WriteLine($"Search : '{search}'");

            // Act
            IEnumerable<int> occurrences = source.Occurrences(search);

            _outputHelper.WriteLine($"Result : {occurrences.Jsonify()}");

            // Assert
            occurrences.Should()
                       .Match(expectation, reason);
        }

        public static TheoryData<StringSegment, char, Expression<Func<IEnumerable<int>, bool>>, string> OccurrencesWithCharCases
            => new()
            {
                {
                    new StringSegment("Firstname"), 'z',
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
                    new StringSegment("Firstname"), 'F', occurrences =>
                        occurrences != null
                        && occurrences.Exactly(1)
                        && occurrences.Once(pos => pos == 0),
                    "There is one occurrence of the search element in the string"
                },
                {
                    new StringSegment("zsasz"), 'z', occurrences =>
                        occurrences != null
                        && occurrences.Exactly(2)
                        && occurrences.Once(pos => pos == 0)
                        && occurrences.Once(pos => pos == 4),
                    "There is 2 occurrences of the search element in the string"
                }
            };

        [Theory]
        [MemberData(nameof(OccurrencesWithCharCases))]
        public void Occurrences_of_char(StringSegment source, char search, Expression<Func<IEnumerable<int>, bool>> expectation, string reason)
        {
            _outputHelper.WriteLine($"Source : '{source.Value}'");
            _outputHelper.WriteLine($"Search : '{search}'");

            // Act
            IEnumerable<int> occurrences = source.Occurrences(search);

            _outputHelper.WriteLine($"Result : {occurrences.Jsonify()}");

            // Assert
            occurrences.Should()
                       .Match(expectation, reason);
        }

        public static IEnumerable<object[]> LastOccurrenceCases
        {
            get
            {
                yield return new object[]
                {
                    new StringSegment("Firstname"),
                    new StringSegment("z"),
                    StringComparison.OrdinalIgnoreCase,
                    -1,
                    "There's no occurrence of the search element in the string"
                };

                yield return new object[]
                {
                    StringSegment.Empty,
                    new StringSegment("z"),
                    StringComparison.OrdinalIgnoreCase,
                    -1,
                    "The source element is empty"
                };

                yield return new object[]
                {
                    new StringSegment("Firstname"),
                    new StringSegment("F"),
                    StringComparison.OrdinalIgnoreCase,
                    0,
                    "There is one occurrence of the search element in the string"
                };

                yield return new object[]
                {
                    new StringSegment("zsasz"),
                    new StringSegment("z"),
                    StringComparison.OrdinalIgnoreCase,
                    4,
                    "There is 2 occurrences of the search element in the string"
                };
            }
        }

        [Theory]
        [MemberData(nameof(LastOccurrenceCases))]
        public void LastOccurrence(StringSegment source, StringSegment search, StringComparison stringComparison, int expectation, string reason)
        {
            // Act
            int actual = source.LastOccurrence(search, stringComparison);

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
            int lastIndex =  source.LastOccurrence(StringSegment.Empty);

            // Assert
            _ = source.Length switch
            {
                > 0 => lastIndex.Should().Be(source.Length - 1),
                _ => lastIndex.Should().Be(0),
            };
        }

        [Property]
        public void Given_source_does_not_have_a_value_When_search_has_a_value_Then_LastOccurrence_should_throw_ArgumentOutOfRangeException(NonNull<string> searchGenerator)
        {
            // Arrange
            StringSegment source = (string) null;
            StringSegment search = searchGenerator.Item;

            // Act
            Action callingLastOccurrenceWhenSourceDoesNotHaveAValue = () =>  source.LastOccurrence(search);

            // Assert
            
            callingLastOccurrenceWhenSourceDoesNotHaveAValue.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName("source");
        }

        
        [Property]
        public void Given_source_has_a_value_When_search_does_not_have_a_value_Then_LastOccurrence_should_throw_ArgumentOutOfRangeException(NonNull<string> sourceGenerator)
        {
            // Arrange
            StringSegment source = sourceGenerator.Item;
            StringSegment search = (string) null;

            // Act
            Action callingLastOccurrenceWhenSourceDoesNotHaveAValue = () =>  source.LastOccurrence(search);

            // Assert
            callingLastOccurrenceWhenSourceDoesNotHaveAValue.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName("search");
        }

        [Property]
        public void Given_source_does_not_have_a_value_When_search_has_a_value_Then_FirstOccurrence_should_throw_ArgumentOutOfRangeException(NonNull<string> searchGenerator)
        {
            // Arrange
            StringSegment source = null;
            StringSegment search = searchGenerator.Item;

            // Act
            Action callingFirstOccurrenceWhenSourceDoesNotHaveAValue = () =>  source.FirstOccurrence(search);

            // Assert
            
            callingFirstOccurrenceWhenSourceDoesNotHaveAValue.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName("source");
        }

        
        [Property]
        public void Given_source_has_a_value_When_search_does_not_have_a_value_Then_FirstOccurrence_should_throw_ArgumentOutOfRangeException(NonNull<string> sourceGenerator)
        {
            // Arrange
            StringSegment source = sourceGenerator.Item;
            StringSegment search = (string) null;

            // Act
            Action callingFirstOccurrenceWhenSourceDoesNotHaveAValue = () =>  source.FirstOccurrence(search);

            // Assert
            callingFirstOccurrenceWhenSourceDoesNotHaveAValue.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName("search");
        }

        
        public static TheoryData<StringSegment, StringSegment, StringComparison, int> FirstOccurrenceCases
            => new()
            {
                {
                    new StringSegment("Firstname"),
                    new StringSegment("z"),
                    StringComparison.OrdinalIgnoreCase,
                    -1
                },
                {
                    StringSegment.Empty,
                    new StringSegment("z"),
                    StringComparison.OrdinalIgnoreCase,
                    -1
                },
                {
                    StringSegment.Empty,
                    StringSegment.Empty,
                    StringComparison.OrdinalIgnoreCase,
                    0
                },
                {
                    new StringSegment("Firstname"),
                    new StringSegment("F"),
                    StringComparison.OrdinalIgnoreCase,
                    0
                },
                {
                    new StringSegment("zsasz"),
                    new StringSegment("z"),
                    StringComparison.OrdinalIgnoreCase,
                    0
                }
            };

        [Theory]
        [MemberData(nameof(FirstOccurrenceCases))]
        public void FirstOccurrence(StringSegment source, StringSegment search, StringComparison stringComparison, int expected)
        {
            // Act
            int actual = source.FirstOccurrence(search, stringComparison);

            // Assert
            actual.Should()
                  .Be(expected);
        }
    }
}
