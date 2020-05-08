#if !(NETCOREAPP2_0 || NETCOREAPP2_1)
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Categories;

namespace Utilities.UnitTests
{
    [UnitTest]
    public class AsyncEnumerableTests
    {
        [Fact]
        public async Task Empty_AsyncEnumerable_has_no_element()
        {
            // Arrange
            IAsyncEnumerable<int> enumerable = AsyncEnumerable.Empty<int>();
            int count = 0;
            // Act
            await foreach (int element in enumerable)
            {
                count++;
            }

            // Assert
            count.Should()
                 .Be(0);
            
        }
    }
}

#endif