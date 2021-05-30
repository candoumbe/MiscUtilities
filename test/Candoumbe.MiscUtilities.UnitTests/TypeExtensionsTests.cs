using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Categories;

namespace Utilities.UnitTests
{
    [UnitTest]
    [Feature("Type")]
    public class TypeExtensionsTests
    {
        [Fact]
        public void Should_be_assignable_from_open_generic_type_to_concrete_open_generic_type()
        {
            typeof(Foo<>).IsAssignableToGenericType(typeof(IFoo<>)).Should().BeTrue();
        }

        [Fact]
        public void Should_be_assignable_from_open_generic_type_to_generic_interface_type()
        {
            typeof(IFoo<int>).IsAssignableToGenericType(typeof(IFoo<>)).Should().BeTrue();
        }

        [Fact]
        public void Should_be_assignable_from_open_generic_type_to_itself()
        {
            typeof(IFoo<>).IsAssignableToGenericType(typeof(IFoo<>)).Should().BeTrue();
        }

        [Fact]
        public void Should_be_assignable_from_open_generic_type_to_concrete_generic_type()
        {
            typeof(Foo<int>).IsAssignableToGenericType(typeof(IFoo<>)).Should().BeTrue();
        }

        [Fact]
        public void Should_be_assignable_from_open_generic_type_to_nongeneric_concrete_type()
        {
            typeof(Bar).IsAssignableToGenericType(typeof(IFoo<>)).Should().BeTrue();
        }

        [Fact]
        public void Given_Should_be_assignable_from_open_generic_type_to_nongeneric_concrete_type()
        {
            typeof(Baz).IsAssignableToGenericType(typeof(IFoo<>)).Should().BeTrue();
        }

        [Theory]
        [InlineData(typeof(Bat), typeof(IFoo<>), true, "Bat inherits from a type that implements IFoo<>")]
        [InlineData(typeof(Bat), typeof(Foo<>), true, "Bat inheriance hierarchy as Foo<>")]
        [InlineData(typeof(Foo<int>), typeof(IFoo<Guid>), false, "Foo<int> does not implements Foo<Guid>")]
        public void Given_type_IsAssignableToGenericType_should_return_expected_result(Type givenType,
                                                                                       Type genericType,
                                                                                       bool expected,
                                                                                       string reason)
        {
            // Act
            bool actual = givenType.IsAssignableToGenericType(genericType);

            // Assert
            actual.Should()
                  .Be(expected, reason);
        }

        public interface IFoo<T> { }

        public class Foo<T> : IFoo<T> { }

        public class Bar : IFoo<int> { }

        public class Baz : Bar { }

        public class Bat : Foo<Guid> {}

        public static IEnumerable<object[]> IsAnonymousCases
        {
            get
            {
                yield return new object[]
                {
                    new {}.GetType(),
                    true
                };

                yield return new object[]
                {
                    typeof(Bar),
                    false
                };

                yield return new object[]
                {
                    typeof(Foo<>),
                    false
                };
            }
        }

        [Theory]
        [MemberData(nameof(IsAnonymousCases))]
        public void IsAnonymous(Type t, bool expectedResult)
            => t.IsAnonymousType().Should().Be(expectedResult);
    }
}
