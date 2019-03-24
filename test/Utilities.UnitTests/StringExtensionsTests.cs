using System;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using Xunit.Categories;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Utilities.UnitTests
{
    [UnitTest]
    [Feature("String")]
    public class StringExtensionsTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        internal class SuperHero
        {
            public string Firstname { get; set; }

            public string Lastname { get; set; }
        }

        public StringExtensionsTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        public void Dispose() => _outputHelper = null;

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
        [InlineData("Zsasz","Zs[A-Z]sz", false, false)]
        [InlineData("Zsasz","Zs[a-z]sz", false, true)]
        [InlineData("Zsasz","Zs[A-Za-z]sz", false, true)]
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

        [Fact]
        public void ToLowerKebabCase_Throws_ArgumentNullException()
        {
            // Act
            Action act = () => StringExtensions.ToLowerKebabCase(null);

            // Assert
            act.Should().Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .BeEquivalentTo("input");
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

        [Theory]
        [InlineData("firstname", "firstname")]
        [InlineData("firstName", "first-name")]
        [InlineData("FirstName", "first-name")]
        public void ToLowerKebabCase(string input, string expectedOutput)
        {
            _outputHelper.WriteLine($"input : '{input}'");
            input.ToLowerKebabCase().Should().Be(expectedOutput);
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
        [InlineData(" firstname", "x.Firstname")]
        public void ToLambda(string property, string expectedLambda)
        {
            // Arrange
            LambdaExpression lambda = StringExtensions.ToLambda<SuperHero>(property);
            string actual = lambda.Body.ToString();

            // Assert
            actual.Should()
                .BeEquivalentTo(expectedLambda);
        }
    }
}
