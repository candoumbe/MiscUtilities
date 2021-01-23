using System;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using Xunit.Categories;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace Utilities.UnitTests
{
    [UnitTest]
    [Feature(nameof(StringExtensions))]
    public class StringExtensionsTests
    {
        private readonly ITestOutputHelper _outputHelper;

        internal class SuperHero
        {
            public string Firstname { get; set; }

            public string Lastname { get; set; }

            public SuperHero Acolyte { get; set; }
        }

        public StringExtensionsTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Theory]
        [InlineData(null, null)]
        [InlineData("bruce", "Bruce")]
        [InlineData("bruce wayne", "Bruce Wayne")]
        [InlineData("cyrille-alexandre", "Cyrille-Alexandre")]
        public void ToTitleCase(string input, string expectedString)
            => input?.ToTitleCase()?.Should().Be(expectedString);

        [Theory]
        [InlineData(null, null)]
        [InlineData("startDate", "startDate")]
        [InlineData("StartDate", "startDate")]
        [InlineData("Start Date", "startDate")]
        [InlineData("start-date", "startDate")]
        public void ToCamelCase(string input, string expectedString)
            => input?.ToCamelCase()?.Should().Be(expectedString);

        [Theory]
        [InlineData("bruce", "Bruce", true, true)]
        [InlineData("bruce", "Bruce", false, false)]
        [InlineData("bruce", "br*ce", true, true)]
        [InlineData("bruce", "br?ce", true, true)]
        [InlineData("bruce", "?r?ce", true, true)]
        [InlineData("Bruce", "?r?ce", false, true)]
        [InlineData("Bruce", "Carl", false, false)]
        [InlineData("Bruce", "Carl", true, false)]
        [InlineData("Bruce", "B*e", false, true)]
        [InlineData("Bruce", "B?e", false, false)]
        [InlineData("Bruce", "B?e", true, false)]
        [InlineData("Bruce", "*,*", true, false)]
        [InlineData("Bruce", "*,*", false, false)]
        [InlineData("Bruce,Dick", "*,*", true, true)]
        [InlineData("Bruce,Dick", "*,*", false, true)]
        [InlineData("100-", "*-", false, true)]
        [InlineData("100-", "*-*", false, true)]
        [InlineData("100-200", "*-*", false, true)]
        [InlineData("100-200", "*-", false, false)]
        [InlineData("Bruce|Dick", "*|*", false, true)]
        [InlineData("Bruce|Dick", "*?|?*", false, true)]
        [InlineData("Bruce", "*?|?*", false, false)]
        [InlineData("Batman", "Bat*man", false, true)]
        [InlineData("Batman", "B[Aa]t*man", false, true)]
        [InlineData("BAtman", "B[Aa]t*man", false, true)]
        [InlineData("B[a]tman", @"B\[a\]t*man", false, true)]
        [InlineData("Zsasz", "Zs[A-Z]sz", false, false)]
        [InlineData("Zsasz", "Zs[a-z]sz", false, true)]
        [InlineData("Zsasz", "Zs[A-Za-z]sz", false, true)]
        [InlineData("first/", "first/*", false, true)]
        public void StringLike(string input, string pattern, bool ignoreCase, bool expectedResult)
        {
            _outputHelper.WriteLine($"input : '{input}'");
            _outputHelper.WriteLine($"pattern : '{pattern}'");
            _outputHelper.WriteLine($"Ignore case : '{ignoreCase}'");

            // Act
            bool result = input.Like(pattern, ignoreCase);

            // Assert
            result.Should().Be(expectedResult);
        }

        public static IEnumerable<object[]> StringSegmentLikeCases
        {
            get
            {
                {
                    StringSegment segment = new StringSegment("Bruce");

                    yield return new object[]
                    {
                        (
                            input : segment,
                            pattern : "Bruce",
                            ignoreCase : false,
                            expected : true
                        )
                    };

                    yield return new object[]
                    {
                        (
                            input : segment,
                            pattern : "bruce",
                            ignoreCase : true,
                            expected : true
                        )
                    };

                    yield return new object[]
                    {
                        (
                            input : segment,
                            pattern : "bruce",
                            ignoreCase : false,
                            expected : false
                        )
                    };

                    yield return new object[]
                    {
                        (
                            input : segment,
                            pattern : "*bruce",
                            ignoreCase : true,
                            expected : true
                        )
                    };

                    yield return new object[]
                    {
                        (
                            input : segment.Subsegment(0, 3),
                            pattern : "*bru",
                            ignoreCase : true,
                            expected : true
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(StringSegmentLikeCases))]
        public void StringSegmentLike((StringSegment input, string pattern, bool ignoreCase, bool expectedResult) data)
        {
            _outputHelper.WriteLine($"input : '{data.input}'");
            _outputHelper.WriteLine($"pattern : '{data.pattern}'");
            _outputHelper.WriteLine($"Ignore case : '{data.ignoreCase}'");

            // Act
            bool result = data.input.Like(data.pattern, data.ignoreCase);

            // Assert
            result.Should().Be(data.expectedResult);
        }

        [Theory]
        [InlineData(null, "test")]
        [InlineData("test", null)]
        public void LikeThrowsArgumentNullException(string input, string pattern)
        {
            // Act
            Action action = () => input.Like(pattern);

            // Assert
            action.Should().Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Slugify_throws_ArgumentNullException_when_input_is_null()
        {
            // Act
            Action slugifyWithNull = () => StringExtensions.Slugify(null);

            // Assert
            slugifyWithNull.Should()
                           .ThrowExactly<ArgumentNullException>()
                           .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName));
        }

        [Theory]
        [InlineData("firstname", "firstname")]
        [InlineData("firstName", "first-name")]
        [InlineData("FirstName", "first-name")]
        [InlineData("first name", "first-name")]
        [InlineData("first  name", "first-name")]
        [InlineData(" first name", "first-name")]
        [InlineData("first name ", "first-name")]
        [InlineData("first/name", "first-name")]
        [InlineData("o'neal", "o-neal")]
        public void Slugify(string input, string expectedOutput)
        {
            _outputHelper.WriteLine($"input : '{input}'");
            input.Slugify().Should().Be(expectedOutput);
        }

        [Theory]
        [InlineData("firstname", "firstname")]
        [InlineData("firstName", "first_name")]
        [InlineData("FirstName", "first_name")]
        [InlineData("first_name", "first_name")]
        [InlineData("first name", "first_name")]
        [InlineData("first  name", "first_name")]
        [InlineData("first-name", "first_name")]
        public void ToSnakeCase(string input, string expectedOutput)
        {
            _outputHelper.WriteLine($"input : '{input}'");
            input.ToSnakeCase().Should().Be(expectedOutput);
        }

        [Fact]
        public void ToSnakeCase_throws_ArgumentNullException_when_input_is_null()
        {
            // Act
            Action toSnakeCaseWithNull = () => StringExtensions.ToSnakeCase(null);

            // Assert
            toSnakeCaseWithNull.Should()
                               .ThrowExactly<ArgumentNullException>()
                               .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName));
        }

        [Fact]
        public void Decode()
        {
            Guid guid = Guid.NewGuid();
            guid.Encode().Decode().Should().Be(guid);
        }

        [Fact]
        public void ToLambdaThrowsArgumentNullExceptionWhenSourceIsNull()
        {
            // Act
            Action action = () => StringExtensions.ToLambda<SuperHero>(null);

            // Assert
            action.Should().Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData("Firstname", "x.Firstname")]
        [InlineData("firstname", "x.Firstname")]
        [InlineData(@"acolyte[""firstname""]", "x.Acolyte.Firstname")]
        [InlineData(@"acolyte[""acolyte""][""acolyte""]", "x.Acolyte.Acolyte.Acolyte")]
        public void ToLambda(string property, string expectedLambda)
        {
            // Act
            LambdaExpression lambda = property.ToLambda<SuperHero>();
            string actual = lambda.Body.ToString();

            // Assert
            actual.Should()
                .BeEquivalentTo(expectedLambda);
        }

        public static IEnumerable<object[]> OccurrencesCases
        {
            get
            {
                yield return new object[]
                {
                    "Firstname",
                    "z",
                    StringComparison.CurrentCulture,
                    (Expression<Func<IEnumerable<int>, bool>>)(occurrences => occurrences != null && !occurrences.Any()),
                    "There's no occurrence of the search element in the string"
                };

                yield return new object[]
                {
                    string.Empty,
                    "z",
                    StringComparison.CurrentCulture,
                    (Expression<Func<IEnumerable<int>, bool>>)(occurrences => occurrences != null && !occurrences.Any()),
                    "The source element is empty"
                };

                yield return new object[]
                {
                    "Firstname",
                    "F",
                    StringComparison.CurrentCulture,
                    (Expression<Func<IEnumerable<int>, bool>>)(occurrences =>
                        occurrences != null && occurrences.Once()
                        && occurrences.Once(pos  => pos == 0)
                    ),
                    "There is one occurrence of the search element in the string"
                };

                yield return new object[]
                {
                    "zsasz",
                    "z",
                    StringComparison.CurrentCulture,
                    (Expression<Func<IEnumerable<int>, bool>>)(occurrences =>
                        occurrences != null && occurrences.Count() == 2
                        && occurrences.Once(pos => pos == 0)
                        && occurrences.Once(pos  =>pos == 4)
                    ),
                    "There are 2 occurrences of the search element in the string"
                };
            }
        }

        [Theory]
        [MemberData(nameof(OccurrencesCases))]
        public void Occurrences(string source, string search, StringComparison stringComparison, Expression<Func<IEnumerable<int>, bool>> expectation, string reason)
        {
            _outputHelper.WriteLine($"Source : '{source}'");
            _outputHelper.WriteLine($"Search : '{search}'");
            _outputHelper.WriteLine($"StringComparison : '{stringComparison}'");

            // Act
            IEnumerable<int> occurrences = source.Occurrences(search, stringComparison);

            _outputHelper.WriteLine($"Result : {occurrences.Jsonify()}");

            // Assert
            occurrences.Should()
                .Match(expectation, reason);
        }

        [Theory]
        [InlineData("", "z", StringComparison.InvariantCulture, -1)]
        [InlineData("zsasz", "z", StringComparison.InvariantCulture, 0)]
        public void FirstOccurrence(string source, string search, StringComparison stringComparison, int expected)
        {
            _outputHelper.WriteLine($"Source : '{source}'");
            _outputHelper.WriteLine($"Search : '{search}'");
            _outputHelper.WriteLine($"StringComparison : '{stringComparison}'");

            // Act
            int occurrence = source.FirstOccurrence(search, stringComparison);

            // Assert
            occurrence.Should()
                      .Be(expected);
        }

        [Fact]
        public void LastOccurrence_should_throws_ArgumentOutOfRangeException_When_Search_Is_Empty()
        {
            // Arrange
            const string source = "source";

            // Act
            Action action = () => source.LastOccurrence(string.Empty);

            // Assert
            action.Should()
                .Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(null, "search", "Source is null")]
        [InlineData("source", null, "Search is null")]
        public void LastOccurrence_should_throws_ArgumentNullRangeException_When_Search_Is_Empty(string source, string search, string reason)
        {
            // Act
            Action action = () => source.LastOccurrence(search);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>(reason);
        }

        [Theory]
        [InlineData("", "a", StringComparison.OrdinalIgnoreCase, -1)]
        [InlineData("a", "a", StringComparison.OrdinalIgnoreCase, 0)]
        [InlineData("zsasz", "z", StringComparison.OrdinalIgnoreCase, 4)]
        public void LastOccurrence_should_returns_expected_index(string source, string search, StringComparison stringComparison, int expected)
        {
            // Act
            int actual = source.LastOccurrence(search, stringComparison);

            // Assert
            actual.Should()
                  .Be(expected);
        }

        [Fact]
        public void FirstOccurrence_should_throws_ArgumentOutOfRangeException_When_Search_Is_Empty()
        {
            // Arrange
            const string source = "source";

            // Act
            Action action = () => source.FirstOccurrence(string.Empty);

            // Assert
            action.Should()
                .Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(null, "search", "Source is null")]
        [InlineData("source", null, "Search is null")]
        public void FirstOccurrence_should_throws_ArgumentNullRangeException_When_Search_Is_Empty(string source, string search, string reason)
        {
            // Act
            Action action = () => source.FirstOccurrence(search);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>(reason);
        }

        [Theory]
        [InlineData("àâäéèêëïîôöùûüÿçÀÂÄÉÈÊËÏÎÔÖÙÛÜŸÇ",
                    "aaaeeeeiioouuuycAAAEEEEIIOOUUUYC")]
        public void RemoveDiacritics(string input, string expected)
        {
            // Act
            string actual = input.RemoveDiacritics();

            // Assert
            actual.Should()
                  .Be(expected);
        }
    }
}