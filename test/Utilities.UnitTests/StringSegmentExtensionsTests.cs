using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using Xunit.Categories;
using Bogus;

namespace Utilities.UnitTests
{
    [UnitTest]
    public class StringSegmentExtensionsTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public StringSegmentExtensionsTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

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
            Faker faker = new Faker();

            // Act
            Action lastOccurrenceWithSearchEmpty = () => new StringSegment(faker.Lorem.Word()).LastOccurrence(StringSegment.Empty);

            // Assert
            lastOccurrenceWithSearchEmpty.Should()
                                         .Throw<ArgumentException>();
        }
    }
}
