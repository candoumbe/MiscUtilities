using Bogus;
using FluentAssertions;
using FluentAssertions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Newtonsoft.Json.JsonConvert;

namespace Utilities.UnitTests
{
    [UnitTest]
    [Feature("Enumerable")]
    public class EnumerableExtensionsTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public EnumerableExtensionsTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        /// <summary>
        /// <see cref="Once(IEnumerable{int}, Expression{Func{int, bool}}, bool)"/> tests cases
        /// </summary>
        public static IEnumerable<object[]> OnceCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<int>(),
                    (Expression<Func<int, bool>>) (x => x == 1),
                    false,
                    "Collection is empty"
                };

                yield return new object[]
                {
                    new []{ 1, 3 },
                    (Expression<Func<int, bool>>) (x => x == 1),
                    true,
                    "Element is exactly one time in the collection"
                };

                yield return new object[]
                {
                    new []{ 3, 1 },
                    (Expression<Func<int, bool>>) (x => x == 1),
                    true,
                    "Element is exactly one time in the collection"
                };

                yield return new object[]
                {
                    new []{ 1, 3, 1 },
                    (Expression<Func<int, bool>>) (x => x == 3),
                    true,
                    "Element is exactly one time in the collection"
                };

                yield return new object[]
                {
                    new []{ 1, 3 },
                    (Expression<Func<int, bool>>) (x => x == 5),
                    false,
                    "Element is not present in the collection"
                };

                yield return new object[]
                {
                    new []{ 1, 1 },
                    (Expression<Func<int, bool>>) (x => x == 1),
                    false,
                    "Element is present more than one time in collection"
                };

                yield return new object[]
                    {
                    new []{ 1 },
                    (Expression<Func<int, bool>>) (x => x == 1),
                    true,
                    "Element is present exactly one time in collection"
                    };
            }
        }

        /// <summary>
        /// Unit tests for <see cref="EnumerableExtensions.Once{T}(IEnumerable{T})"/>
        /// </summary>
        /// <param name="source">collection to apply <see cref="EnumerableExtensions.Once{T}(IEnumerable{T})"/> onto.</param>
        /// <param name="predicate">predicate</param>
        /// <param name="expectedResult">expected result</param>
        /// <param name="reason">Message to display when assertion fails</param>
        [Theory]
        [MemberData(nameof(OnceCases))]
        public void Once(IEnumerable<int> source, Expression<Func<int, bool>> predicate, bool expectedResult, string reason)
        {
            _outputHelper.WriteLine($"{nameof(source)} : {SerializeObject(source)}");
            _outputHelper.WriteLine($"{nameof(predicate)} : {predicate}");

            // Act
            bool actualResult = source.Once(predicate);

            // Assert
            actualResult.Should()
                .Be(expectedResult, reason);
        }

        public static IEnumerable<object[]> OnceWithPredicateThrowsArgumentNullExceptionCases
        {
            get
            {
                yield return new[] { null, (Expression<Func<int, bool>>)(x => x == 1) };
                yield return new[] { Enumerable.Empty<int>(), null };
            }
        }

        /// <summary>
        /// Tests <see cref="EnumerableExtensions.Once{T}(IEnumerable{T}, Expression{Func{T, bool}})"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        [Theory]
        [MemberData(nameof(OnceWithPredicateThrowsArgumentNullExceptionCases))]
        public void OnceWithPredicateShouldThrowsArgumentNullException(IEnumerable<int> source, Expression<Func<int, bool>> predicate)
        {
            // Act
            Action action = () => source.Once(predicate);

            // Assert
            action.Should().Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        public static IEnumerable<object[]> AtLeastOnceWithPredicateThrowsArgumentNullExceptionCases
        {
            get
            {
                yield return new[] { null, (Expression<Func<int, bool>>)(x => x == 1) };
                yield return new[] { Enumerable.Empty<int>(), null };
            }
        }

        [Fact]
        public void AtLeast_with_predicate()
        {
            // Arrange
            IEnumerable<int> source = Enumerable.Empty<int>();

            // Act
            source.AtLeast(x => x == -1, 0).Should().BeTrue("a empty collection always has at least 0 items.");
        }

        [Fact]
        public void AtLeast_without_predicate()
        {
            // Arrange
            IEnumerable<int> source = Enumerable.Empty<int>();

            // Act
            source.AtLeast(1).Should()
                .BeFalse("an empty collection always has at least 0 items.");
        }

        [Fact]
        public void AtMost()
        {
            // Arrange
            IEnumerable<int> source = Enumerable.Empty<int>();

            // Act
            source.AtMost(x => x == -1, 0).Should().BeTrue("an empty collection always has at most 0 items.");
        }

        [Theory]
        [MemberData(nameof(AtLeastOnceWithPredicateThrowsArgumentNullExceptionCases))]
        public void AtLeastOnce_With_Predicate_Should_Throws_ArgumentNullException(IEnumerable<int> source, Expression<Func<int, bool>> predicate)
        {
            _outputHelper.WriteLine($"source is null : {source is null}");
            _outputHelper.WriteLine($"predicate is null : {predicate is null}");

            // Act
            Action action = () => source.AtLeastOnce(predicate);

            // Assert
            action.Should().Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// <see cref="AtLeastOnce(IEnumerable{int}, Expression{Func{int, bool}}, bool)"/> tests cases
        /// </summary>
        public static IEnumerable<object[]> AtLeastOnceWithPredicateCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<int>(),
                    (Expression<Func<int, bool>>) (x => x == 1),
                    false
                };

                yield return new object[]
                {
                    new []{ 1, 3 },
                    (Expression<Func<int, bool>>) (x => x == 1),
                    true
                };

                yield return new object[]
                {
                    new []{ 1, 3 },
                    (Expression<Func<int, bool>>) (x => x == 5),
                    false
                };

                yield return new object[]
                {
                    new []{ 1, 3, 3 },
                    (Expression<Func<int, bool>>) (x => x ==3),
                    true
                };

                yield return new object[]
                {
                    new [] {1},
                    (Expression<Func<int, bool>>)(x => x == 1),
                    true
                };
            }
        }

        /// <summary>
        /// Unit tests for <see cref="EnumerableExtensions.AtLeastOnce{T}(IEnumerable{T})"/>
        /// </summary>
        /// <param name="source">collection to apply <see cref="EnumerableExtensions.AtLeastOnce{T}(IEnumerable{T})"/> onto.</param>
        /// <param name="predicate">predicate</param>
        /// <param name="expectedResult">expected result</param>
        [Theory]
        [MemberData(nameof(AtLeastOnceWithPredicateCases))]
        public void AtLeastOnceWithPredicate(IEnumerable<int> source, Expression<Func<int, bool>> predicate, bool expectedResult)
        {
            _outputHelper.WriteLine($"{nameof(source)} : {SerializeObject(source)}");
            _outputHelper.WriteLine($"{nameof(predicate)} : {predicate}");

            // Act
            bool actualResult = source.AtLeastOnce(predicate);

            // Assert
            actualResult.Should().Be(expectedResult);
        }

        /// <summary>
        /// <see cref="AtLeastOnce(IEnumerable{int}, bool)"/> tests cases
        /// </summary>
        public static IEnumerable<object[]> AtLeastOnceCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<int>(),
                    false
                };

                yield return new object[]
                {
                    new []{ 1, 3 },
                    true
                };

                yield return new object[]
                {
                    new [] {1},
                    true
                };
            }
        }

        /// <summary>
        /// Unit tests for <see cref="EnumerableExtensions.AtLeastOnce{T}(IEnumerable{T})"/>
        /// </summary>
        /// <param name="source">collection to apply <see cref="EnumerableExtensions.AtLeastOnce{T}(IEnumerable{T})"/> onto.</param>
        /// <param name="predicate">predicate</param>
        /// <param name="expectedResult">expected result</param>
        [Theory]
        [MemberData(nameof(AtLeastOnceCases))]
        public void AtLeastOnce(IEnumerable<int> source, bool expectedResult)
        {
            _outputHelper.WriteLine($"{nameof(source)} : {SerializeObject(source)}");

            // Act
            bool actualResult = source.AtLeastOnce();

            // Assert
            actualResult.Should().Be(expectedResult);
        }

        public static IEnumerable<object[]> AtLeastOnceThrowsArgumentNullExceptionCases
        {
            get
            {
                yield return new[] { null, (Expression<Func<int, bool>>)(x => x == 1) };
                yield return new[] { Enumerable.Empty<int>(), null };
            }
        }

        [Theory]
        [MemberData(nameof(AtLeastOnceThrowsArgumentNullExceptionCases))]
        public void AtLeastOnceShouldThrowsArgumentNullException(IEnumerable<int> source, Expression<Func<int, bool>> predicate)
            => AtLeastShouldThrowArgumentNullException(source, 1, predicate);

        public static IEnumerable<object[]> AtLeastThrowsArgumentNullExceptionCases
        {
            get
            {
                yield return new object[] { null, 4, (Expression<Func<int, bool>>)(x => x == 1) };
                yield return new object[] { Enumerable.Empty<int>(), 5, null };
            }
        }

        [Theory]
        [MemberData(nameof(AtLeastThrowsArgumentNullExceptionCases))]
        public void AtLeastShouldThrowArgumentNullException(IEnumerable<int> source, int count, Expression<Func<int, bool>> predicate)
        {
            // Act
            Action action = () => source.AtLeast(predicate, count);

            // Assert
            action.Should().Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void AtLeastShouldThrowArgumentOutOfRangeExceptionWhenParameterCountIsNegative(int count)
        {
            // Act
            Action action = () => new[] { 1, 3, 5 }.AtLeast(x => x == 2, count);

            // Assert
            action.Should().Throw<ArgumentOutOfRangeException>($"{count} is not a valid value").Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        public static IEnumerable<object[]> AtMostWithPredicateThrowsArgumentNullExceptionCases
        {
            get
            {
                yield return new object[] { null, 4, (Expression<Func<int, bool>>)(x => x == 1) };
                yield return new object[] { Enumerable.Empty<int>(), 5, null };
            }
        }

        [Theory]
        [MemberData(nameof(AtMostWithPredicateThrowsArgumentNullExceptionCases))]
        public void AtMostWithPredicateShouldThrowArgumentNullException(IEnumerable<int> source, int count, Expression<Func<int, bool>> predicate)
        {
            // Act
            Action action = () => source.AtMost(predicate, count);

            // Assert
            action.Should().Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void AtMostShouldThrowArgumentOutOfRangeExceptionWhenParameterCountIsNegative(int count)
        {
            // Act
            Action action = () => new[] { 1, 3, 5 }.AtMost(x => x == 2, count);

            // Assert
            action.Should().Throw<ArgumentOutOfRangeException>($"{count} is not a valid value").Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        public static IEnumerable<object[]> ExactlyWithPredicateThrowsArgumentNullExceptionCases
        {
            get
            {
                yield return new object[] { null, 4, (Expression<Func<int, bool>>)(x => x == 1) };
                yield return new object[] { Enumerable.Empty<int>(), 5, null };
            }
        }

        [Theory]
        [MemberData(nameof(ExactlyWithPredicateThrowsArgumentNullExceptionCases))]
        public void ExactlyWithPredicateShouldThrowArgumentNullException(IEnumerable<int> source, int count, Expression<Func<int, bool>> predicate)
        {
            // Act
            Action action = () => source.Exactly(predicate, count);

            // Assert
            action.Should().Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ExactlyShouldThrowArgumentNullExceptionWhenCollectionIsNull()
        {
            // Act
            IEnumerable<int> collection = null;

            Action action = () => collection.Exactly(10);

            // Assert
            action.Should().Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ExactlyShouldThrowArgumentOutOfRangeCountIsNegative()
        {
            // Act
            IEnumerable<int> collection = Enumerable.Empty<int>();

            Action action = () => collection.Exactly(-10);

            // Assert
            action.Should().Throw<ArgumentOutOfRangeException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void ExactlyShouldThrowArgumentOutOfRangeExceptionWhenParameterCountIsNegative(int count)
        {
            // Act
            Action action = () => new[] { 1, 3, 5 }.Exactly(x => x == 2, count);

            // Assert
            action.Should().Throw<ArgumentOutOfRangeException>($"{count} is not a valid value").Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// <see cref="CrossJoin(IEnumerable{int}, IEnumerable{int})"/> tests cases
        /// </summary>
        public static IEnumerable<object[]> CrossJoinTwoCollectionCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<int>(),
                    Enumerable.Empty<int>(),
                    (Expression<Func<IEnumerable<(int, int)>, bool>>)(items => !items.Any())
                };

                yield return new object[]
                {
                    new [] { 1, 3 },
                    Enumerable.Empty<int>(),
                    (Expression<Func<IEnumerable<(int, int)>, bool>>)(items => !items.Any())
                };

                yield return new object[]
                {
                    Enumerable.Empty<int>(),
                    new [] { 1, 3 },
                    (Expression<Func<IEnumerable<(int, int)>, bool>>)(items => !items.Any())
                };

                yield return new object[]
                {
                    new [] { 1, 3 },
                    new [] { 2 },
                    (Expression<Func<IEnumerable<(int X, int Y)>, bool>>)(items =>
                        items.Count() == 2
                        && items.Once(tuple => tuple.X == 1 && tuple.Y == 2)
                        && items.Once(tuple => tuple.X == 3 && tuple.Y == 2)
                    )
                };

                yield return new object[]
                {
                    new [] { 2 },
                    new [] { 1, 3 },
                    (Expression<Func<IEnumerable<(int X, int Y)>, bool>>)(items =>
                        items.Count() == 2
                        && items.Once(tuple => tuple.X == 2 && tuple.Y == 1)
                        && items.Once(tuple => tuple.X == 2 && tuple.Y == 3)
                    )
                };
            }
        }

        /// <summary>
        /// Unit tests <see cref="EnumerableExtensions.CrossJoin{TFirst, TSecond}(IEnumerable{TFirst}, IEnumerable{TSecond})"/>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="crossJoinResultExpectation"></param>
        [Theory]
        [MemberData(nameof(CrossJoinTwoCollectionCases))]
        public void CrossJoinTwoCollections(IEnumerable<int> first, IEnumerable<int> second, Expression<Func<IEnumerable<(int, int)>, bool>> crossJoinResultExpectation)
        {
            _outputHelper.WriteLine($"{nameof(first)} : {SerializeObject(first)}");
            _outputHelper.WriteLine($"{nameof(second)} : {SerializeObject(second)}");

            // Act
            IEnumerable<(int, int)> result = first.CrossJoin(second);

            // Assert
            result.Should()
                .Match(crossJoinResultExpectation);
        }

        /// <summary>
        /// <see cref="CrossJoin(IEnumerable{int}, IEnumerable{int}, IEnumerable{string})"/> tests cases
        /// </summary>
        public static IEnumerable<object[]> CrossJoinThreeCollectionCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<int>(),
                    Enumerable.Empty<int>(),
                    Enumerable.Empty<string>(),
                    (Expression<Func<IEnumerable<(int, int, string)>, bool>>)(items => !items.Any())
                };

                yield return new object[]
                {
                    new [] { 1, 3 },
                    Enumerable.Empty<int>(),
                    Enumerable.Empty<string>(),
                    (Expression<Func<IEnumerable<(int, int, string)>, bool>>)(items => !items.Any())
                };

                yield return new object[]
                {
                    Enumerable.Empty<int>(),
                    new [] { 1, 3 },
                    Enumerable.Empty<string>(),
                    (Expression<Func<IEnumerable<(int, int, string)>, bool>>)(items => !items.Any())
                };

                yield return new object[]
                {
                    new [] { 1, 3 },
                    new [] { 2 },
                    new [] { "a"},
                    (Expression<Func<IEnumerable<(int X, int Y, string Letter)>, bool>>)(items =>
                        items.Count() == 2
                        && items.Once(tuple => tuple.X == 1 && tuple.Y == 2 && tuple.Letter == "a" )
                        && items.Once(tuple => tuple.X == 3 && tuple.Y == 2 && tuple.Letter == "a" )
                    )
                };

                yield return new object[]
                {
                    new [] { 2 },
                    new [] { 1, 3 },
                    new [] { "a"},
                    (Expression<Func<IEnumerable<(int X, int Y, string Letter)>, bool>>)(items =>
                        items.Count() == 2
                        && items.Once(tuple => tuple.X == 2 && tuple.Y == 1 && tuple.Letter == "a")
                        && items.Once(tuple => tuple.X == 2 && tuple.Y == 3 && tuple.Letter == "a")
                    )
                };
            }
        }

        /// <summary>
        /// Unit tests <see cref="EnumerableExtensions.CrossJoin{TFirst, TSecond}(IEnumerable{TFirst}, IEnumerable{TSecond})"/>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="third"></param>
        /// <param name="crossJoinResultExpectation"></param>
        [Theory]
        [MemberData(nameof(CrossJoinThreeCollectionCases))]
        public void CrossJoinThreeCollections(IEnumerable<int> first, IEnumerable<int> second, IEnumerable<string> third, Expression<Func<IEnumerable<(int, int, string)>, bool>> crossJoinResultExpectation)
        {
            _outputHelper.WriteLine($"{nameof(first)} : {SerializeObject(first)}");
            _outputHelper.WriteLine($"{nameof(second)} : {SerializeObject(second)}");
            _outputHelper.WriteLine($"{nameof(third)} : {SerializeObject(third)}");

            // Act
            IEnumerable<(int, int, string)> result = first.CrossJoin(second, third);

            // Assert
            result.Should()
                .Match(crossJoinResultExpectation);
        }

        /// <summary>
        /// <see cref="None(IEnumerable{int}, Expression{Func{int, bool}}, bool)"/> tests cases
        /// </summary>
        public static IEnumerable<object[]> NoneCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<int>(),
                    (Expression<Func<int, bool>>) (x => x == 1),
                    true
                };

                yield return new object[]
                {
                    new []{ 1, 3 },
                    (Expression<Func<int, bool>>) (x => x == 1),
                    false
                };

                yield return new object[]
                {
                    new []{ 1, 3 },
                    (Expression<Func<int, bool>>) (x => x == 5),
                    true
                };
            }
        }

        /// <summary>
        /// Unit tests for <see cref="EnumerableExtensions.None{T}(IEnumerable{T})"/>
        /// </summary>
        /// <param name="source">collection to apply <see cref="EnumerableExtensions.None{T}(IEnumerable{T})"/> onto.</param>
        /// <param name="predicate">predicate</param>
        /// <param name="expectedResult">expected result</param>
        [Theory]
        [MemberData(nameof(NoneCases))]
        public void None(IEnumerable<int> source, Expression<Func<int, bool>> predicate, bool expectedResult)
        {
            _outputHelper.WriteLine($"{nameof(source)} : {SerializeObject(source)}");
            _outputHelper.WriteLine($"{nameof(predicate)} : {predicate}");

            // Act and assert
            source.None(predicate).Should().Be(expectedResult);
        }

#if ! (NETCOREAPP2_0 || NETCOREAPP2_1) 
        [Fact]
        public void AsAsyncEnumerable_Throws_ArgumentNullException_When_Source_Is_Null()
        {
            // Act
            Action asAsyncEnumerableWithNullSource = () => EnumerableExtensions.AsAsyncEnumerable<int>(null);

            // Assert
            asAsyncEnumerableWithNullSource.Should()
                .ThrowExactly<ArgumentNullException>($"{nameof(EnumerableExtensions.AsAsyncEnumerable)} does not allow to pass null");
        }

        public static IEnumerable<object[]> AsAsyncEnumerableIncorrectMillisecondsDelayCases
        {
            get
            {
                Faker faker = new Faker();
                int casesCount = faker.Random.Int(min: 1, max: 20);
                int millisecondsDelay = faker.Random.Int(max: -1);

                for (int i = 0; i < casesCount; i++)
                {
                    yield return new object[]
                    {
                        millisecondsDelay
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(AsAsyncEnumerableIncorrectMillisecondsDelayCases))]
        public void AsAsyncEnumerable_Throws_ArgumentOutOfRangeException_When_MillisecondsDelay_Is_Negative(int millisecondsDelay)
        {
            // Arrange
            Faker faker = new Faker();
            int[] source = faker.Random.Digits(count: 10);

            _outputHelper.WriteLine($"Collection : {source.Jsonify()}");

            // Act
            Action asAsyncEnumerableWithInvalidDelay = () => source.AsAsyncEnumerable(millisecondsDelay);

            // Assert
            asAsyncEnumerableWithInvalidDelay.Should()
                .ThrowExactly<ArgumentOutOfRangeException>($"{nameof(EnumerableExtensions.AsAsyncEnumerable)} does not allow passing negative or null delay");
        }

        public static IEnumerable<object[]> AsAsyncEnumerableRespectInputDataCases
        {
            get
            {
                Faker faker = new Faker();
                int casesCount = faker.Random.Number(min: 1, max: 20);

                for (int i = 0; i < casesCount; i++)
                {
                    int elementCount = faker.Random.Number(min: 10, max: 30);
                    int[] inputs = faker.Random.Digits(elementCount);
                    yield return new object[]
                    {
                        inputs
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(AsAsyncEnumerableRespectInputDataCases))]
        public async Task AsAsyncEnumerableRespectInputData(int[] inputs)
        {
            // Arrange
            IList<int> output = new List<int>(inputs.Length);

            // Act
            await foreach (int element in inputs.AsAsyncEnumerable())
            {
                output.Add(element);
            }

            // Assert
            output.Should()
                .BeEquivalentTo(inputs);
        }
#endif
    }
}
