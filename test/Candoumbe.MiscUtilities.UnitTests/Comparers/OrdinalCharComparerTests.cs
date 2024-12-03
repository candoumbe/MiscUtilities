using Candoumbe.MiscUtilities.Comparers;
using FluentAssertions;
using FsCheck.Xunit;

namespace Candoumbe.MiscUtilities.UnitTests.Comparers
{
    public class OrdinalCharComparerTests
    {
        private readonly OrdinalCharComparer _sut = new();

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
            bool expected = x == y;

            // Act
            bool actual = _sut.Equals(x, y);

            // Assert
            actual.Should().Be(expected);
        }

        [Property]
        public void Given_any_character_Then_GetHashcode_returns_the_hashcode_of_the_character(char value)
        {
            // Arrange
            int expected = value.GetHashCode();

            // Act
            int actual = _sut.GetHashCode(value);

            // Assert
            actual.Should().Be(expected);
        }
    }
}