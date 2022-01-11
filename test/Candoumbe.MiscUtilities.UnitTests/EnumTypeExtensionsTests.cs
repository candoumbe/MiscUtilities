#if !NET5_0_OR_GREATER
// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using FluentAssertions;

using System;
using System.Collections.Generic;

using Xunit;
using Xunit.Categories;

namespace Candoumbe.MiscUtilities.UnitTests
{
    [UnitTest]
    public class EnumTypeExtensionsTests
    {
        [Fact]
        public void Given_type_is_not_an_enum_GetValues_should_throw_exception()
        {
            // Act
            Action act = () => EnumExtensions.GetValues<object>();

            // Assert
            act.Should()
               .Throw<ArgumentException>();
        }

        [Fact]
        public void Given_type_is_an_enum_GetValues_should_return_a_collection_containing_all_values()
        {
            // Act
            IEnumerable<EnumType> values = EnumExtensions.GetValues<EnumType>();

            // Assert
            IEnumerable<EnumType> references = Enum.GetValues(typeof(EnumType)).ToArray<EnumType>();
            values.Should()
                  .HaveCount(7).And
                  .BeEquivalentTo(references);
        }
    }

    internal enum EnumType
    {
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven
    }
}

#endif