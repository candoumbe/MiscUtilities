using Candoumbe.MiscUtilities.Comparers;
using FluentAssertions;
using FsCheck.Xunit;
using Xunit;

namespace Candoumbe.MiscUtilities.UnitTests.Comparers
{
    public class InvariantCultureIgnoreCaseCharComparerTests
    {
        private readonly InvariantCultureIgnoreCaseCharComparer _sut = new();

        [Property]
        public void Given_a_character_When_comparing_with_itself_Then_result_should_be_true(char value)
        {
            // Act
            bool actual = _sut.Equals(value, value);

            // Assert
            actual.Should().BeTrue();
        }

        [Property]
        public void Given_two_characters_When_they_are_equal_Then_comparing_should_be_true(char x, char y)
        {
            // Arrange
            bool expected = char.ToLowerInvariant(x) == char.ToLowerInvariant(y);

            // Act
            bool actual = _sut.Equals(x, y);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData('a', 'A', true)]
        public void Given_two_characters_When_they_are_equal_Then_comparing_should_be_true(char x, char y, bool expected)
        {
            // Act
            bool actual = _sut.Equals(x, y);

            // Assert
            actual.Should().Be(expected);
        }
    }
}