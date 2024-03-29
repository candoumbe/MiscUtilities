﻿// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Utilities.UnitTests
{
    [UnitTest]
    public class StringSegmentExtensionsTests(ITestOutputHelper outputHelper)
    {
        private readonly ITestOutputHelper _outputHelper = outputHelper;

        public static IEnumerable<object[]> OccurrencesCases
        {
            get
            {
                yield return new object[]
                {
                    new StringSegment("Firstname"),
                    new StringSegment("z"),
                    (Expression<Func<IEnumerable<int>, bool>>)(occurrences => occurrences != null && !occurrences.Any()),
                    "There's no occurrence of the search element in the string"
                };

                yield return new object[]
                {
                    StringSegment.Empty,
                    new StringSegment("z"),
                    (Expression<Func<IEnumerable<int>, bool>>)(occurrences => occurrences != null && !occurrences.Any()),
                    "The source element is empty"
                };

                yield return new object[]
                {
                    new StringSegment("Firstname"),
                    new StringSegment("F"),
                    (Expression<Func<IEnumerable<int>, bool>>)(occurrences =>
                        occurrences != null
                        && occurrences.Exactly(1)
                        && occurrences.Once(pos  => pos == 0)
                    ),
                    "There is one occurrence of the search element in the string"
                };

                yield return new object[]
                {
                    new StringSegment("zsasz"),
                    new StringSegment("z"),
                    (Expression<Func<IEnumerable<int>, bool>>)(occurrences =>
                        occurrences != null
                        && occurrences.Exactly(2)
                        && occurrences.Once(pos => pos == 0)
                        && occurrences.Once(pos  =>pos == 4)
                    ),
                    "There is 2 occurrences of the search element in the string"
                };
            }
        }

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

        public static IEnumerable<object[]> OccurrencesWithCharCases
        {
            get
            {
                yield return new object[]
                {
                    new StringSegment("Firstname"),
                    'z',
                    (Expression<Func<IEnumerable<int>, bool>>)(occurrences => occurrences != null && occurrences.None()),
                     "There's no occurrence of the search element in the string"
                };

                yield return new object[]
                {
                    StringSegment.Empty,
                    'z',
                    (Expression<Func<IEnumerable<int>, bool>>)(occurrences => occurrences != null && occurrences.None()),
                     "There source is empty"
                };

                yield return new object[]
                {
                    new StringSegment("Firstname"),
                    'F',
                    (Expression<Func<IEnumerable<int>, bool>>)(occurrences =>
                        occurrences != null
                        && occurrences.Exactly(1)
                        && occurrences.Once(pos  => pos == 0)
                    ),
                    "There is one occurrence of the search element in the string"
                };

                yield return new object[]
                {
                    new StringSegment("zsasz"),
                    'z',
                    (Expression<Func<IEnumerable<int>, bool>>)(occurrences =>
                        occurrences != null
                        && occurrences.Exactly(2)
                        && occurrences.Once(pos => pos == 0)
                        && occurrences.Once(pos  =>pos == 4)
                    ),
                    "There is 2 occurrences of the search element in the string"
                };
            }
        }

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

        [Fact]
        public void LastOccurence_throws_ArgumentException_is_search_is_empty()
        {
            // Arrange
            Faker faker = new();

            // Act
            Action lastOccurrenceWithSearchEmpty = () => new StringSegment(faker.Lorem.Word()).LastOccurrence(StringSegment.Empty);

            // Assert
            lastOccurrenceWithSearchEmpty.Should()
                                         .Throw<ArgumentException>();
        }

        public static IEnumerable<object[]> FirstOccurrenceCases
        {
            get
            {
                yield return new object[]
                {
                    new StringSegment("Firstname"),
                    new StringSegment("z"),
                    StringComparison.OrdinalIgnoreCase,
                    -1
                };

                yield return new object[]
                {
                    StringSegment.Empty,
                    new StringSegment("z"),
                    StringComparison.OrdinalIgnoreCase,
                    -1
                };

                yield return new object[]
                {
                    new StringSegment("Firstname"),
                    new StringSegment("F"),
                    StringComparison.OrdinalIgnoreCase,
                    0
                };

                yield return new object[]
                {
                    new StringSegment("zsasz"),
                    new StringSegment("z"),
                    StringComparison.OrdinalIgnoreCase,
                    0
                };
            }
        }

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
