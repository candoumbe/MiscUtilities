using FluentAssertions;
using System;
using Xunit;
using Xunit.Categories;

namespace Utilities.UnitTests
{
    [UnitTest]
    [Feature("Guid")]
    public class GuidExtensionsTests
    {
        [Fact]
        public void Encode()
        {
            Guid guid = Guid.NewGuid();
            string encodedString = guid.Encode();

            encodedString.Should().HaveLength(22);
            encodedString.Decode().Should().Be(guid);
        }
    }
}
