// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using Bogus;

using Candoumbe.MiscUtilities.Collections;

using FluentAssertions;

using FsCheck;
using FsCheck.Xunit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Candoumbe.MiscUtilities.UnitTests.Collections
{
    [UnitTest]
    public class FisherYatesShufflerTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly FisherYatesShuffler<int> _sut;
        private static readonly Faker _faker = new();

        public FisherYatesShufflerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _sut = new();
        }

        [Property]
        public async Task Given_a_collection_of_items_Shuffle_should_shuffle_the_collection(NonEmptySet<int> original)
        {
            // Arrange
            List<int> input = new(original.Item.Count);
            original.Item.ForEach(val => input.Add(val));

            _outputHelper.WriteLine($"Input : {input.Jsonify()}");

            int runCount = _faker.Random.Int(10, 100);

            // Act
            IEnumerable<Task<IEnumerable<int>>> tasks = Enumerable.Range(0, runCount)
                                                                  .Select(_ => Task.Run(() => _sut.Shuffle(input)));

            await Task.WhenAll(tasks);

            (IEnumerable<IEnumerable<int>> Thruthy, IEnumerable<IEnumerable<int>> Falsy) = tasks.Select(tsk => tsk.Result)
                                                                                                .SortBy(items => items.SequenceEqual(input));

            // Assert                   
            _ = input.Count switch
            {
                < 2 => Falsy.Should().BeEmpty("Fisher-Yates algorithm cannot shuffle empty or random imput"),
                _ => Falsy.Should().NotBeEmpty("Fisher-Yates algorithm should produces at least one output that is not in same order as the input").And
                          .OnlyContain(output => !output.SequenceEqual(input))
            };
        }

        [Fact]
        public void Given_an_empty_collection_Shuffle_should_returns_an_empty_output()
        {
            // Arrange
            IEnumerable<int> input = Enumerable.Empty<int>();
            FisherYatesShuffler<int> shuffler = new();

            // Act
            IEnumerable<int> output = shuffler.Shuffle(input);

            // Assert
            output.Should()
                  .BeEmpty("there is no item to shuffle");
        }

        [Property]
        public void Given_an_collection_with_two_element_Shuffle_should_returns_same_result_as_reverse()
        {
            // Arrange
            IEnumerable<int> input = new[] { 1, 2 };
            FisherYatesShuffler<int> shuffler = new();

            // Act
            IEnumerable<int> output = shuffler.Shuffle(input);

            // Assert
            output.Should()
                  .BeEquivalentTo(input.Reverse());
        }
    }
}
