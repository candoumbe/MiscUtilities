using System;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Xunit.Categories;

namespace Candoumbe.MiscUtilities.UnitTests;

[UnitTest]
[Feature(nameof(GuidExtensions))]
public class GuidExtensionsTests
{
    [Property]
    public Property NotEmpty(Guid input)
    {
        // Act
        string encodedString = input.Encode();
        bool isNotEmpty = encodedString != default;

        // Assert
        return isNotEmpty.ToProperty();
    }

    [Property()]
    public Property NotNull(Guid input)
    {
        // Act
        string encodedString = input.Encode();
        bool isNotNull = encodedString is not null;

        //Assert
        return isNotNull.ToProperty();
    }

    [Property]
    public Property Encode_is_pure()
    {
        return Prop.ForAll<Guid, Guid>((first, second) =>
                                       {
                                           string firstEncoded = first.Encode();
                                           string secondEncoded = second.Encode();

                                           return (firstEncoded.Length == secondEncoded.Length)
                                               .And(
                                                    (firstEncoded == secondEncoded).When(first == second)
                                                    .Or(firstEncoded != secondEncoded).When(first != second)
                                                   );
                                       });
    }

    [Property]
    public Property EncodeDecode_returns_original() => Prop.ForAll<Guid>(guid => guid.Encode().Decode() == guid);
}