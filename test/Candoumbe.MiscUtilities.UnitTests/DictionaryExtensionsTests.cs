// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Candoumbe.MiscUtilities.UnitTests.Generators;
using FluentAssertions;
using FluentAssertions.Extensions;
using FsCheck.Xunit;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static System.StringSplitOptions;

namespace Candoumbe.MiscUtilities.UnitTests;

/// <summary>
/// unit tests for <see cref="DictionaryExtensions"/> methods.
/// </summary>
[UnitTest]
[Feature("IDictionary")]
public class DictionaryExtensionsTests(ITestOutputHelper outputHelper)
{
    private readonly ITestOutputHelper _outputHelper = outputHelper;

    public static TheoryData<IEnumerable<KeyValuePair<string, object>>, string> ToQueryStringCases
        => new()
        {
            { null, string.Empty },
            { new Dictionary<string, object>(), string.Empty },
            { new Dictionary<string, object> { ["limit"] = 1 }, "limit=1" },
            { [new KeyValuePair<string, object>("limit", null)], string.Empty },
            {
                [
                    new KeyValuePair<string, object>("color", 3),
                    new KeyValuePair<string, object>("color", 2)
                ],
                "color=3&color=2"
            },
            {
                new Dictionary<string, object> { ["date"] = 1.February(2010).AddMinutes(30).AsLocal() },
                "date=2010-02-01T00:30:00"
            },
            {
                new Dictionary<string, object> { ["date"] = 1.February(2010).AddMinutes(30).AsUtc() },
                "date=2010-02-01T00:30:00Z"
            },
            { new Dictionary<string, object> { ["date"] = 1.February(2010).AsLocal() }, "date=2010-02-01T00:00:00" },
            {
                new Dictionary<string, object>
                {
                    ["date-with-offset"] = new DateTimeOffset(1.February(2010).Add(11.Hours()), 1.Hours())
                },
                "date-with-offset=2010-02-01T11:00:00+01:00"
            },
            { new Dictionary<string, object> { ["offset"] = 3, ["limit"] = 3 }, "offset=3&limit=3" },
            {
                new Dictionary<string, object>
                {
                    ["search"] = new Dictionary<string,
                        object>
                    {
                        ["page"] = 1,
                        ["pageSize"] = 3,
                        ["filter"] =
                            new Dictionary<string, object>
                            {
                                ["field"] = "firstname", ["op"] = "eq", ["value"] = "Bruce"
                            }
                    },
                },
                $"{Uri.EscapeDataString("search[page]")}=1" +
                $"&{Uri.EscapeDataString("search[pageSize]")}=3" +
                $"&{Uri.EscapeDataString("search[filter][field]")}=firstname" +
                $"&{Uri.EscapeDataString("search[filter][op]")}=eq" +
                $"&{Uri.EscapeDataString("search[filter][value]")}=Bruce"
            },
            {
                [
                    new KeyValuePair<string, object>("search",
                        new[]
                        {
                            new KeyValuePair<string, object>("page", 1),
                            new KeyValuePair<string, object>("pageSize", 3),
                            new KeyValuePair<string, object>("filter",
                                (KeyValuePair<string, object>[])
                                [
                                    new KeyValuePair<string, object>("field", "firstname"),
                                    new KeyValuePair<string, object>("op", "EqualTo"),
                                    new KeyValuePair<string, object>("value", "Bruce")
                                ])
                        })
                ],
                $"{Uri.EscapeDataString("search[page]")}=1" +
                $"&{Uri.EscapeDataString("search[pageSize]")}=3" +
                $"&{Uri.EscapeDataString("search[filter][field]")}=firstname" +
                $"&{Uri.EscapeDataString("search[filter][op]")}=EqualTo" +
                $"&{Uri.EscapeDataString("search[filter][value]")}=Bruce"
            },
            {
                new Dictionary<string, object> { ["name"] = (int[]) [1, 5, 4] },
                $"{Uri.EscapeDataString("name[0]")}=1" +
                $"&{Uri.EscapeDataString("name[1]")}=5" +
                $"&{Uri.EscapeDataString("name[2]")}=4"
            }
        };

    /// <summary>
    /// Tests <see cref="System.Collections.Generic.DictionaryExtensions.ToQueryString"/>
    /// </summary>
    /// <param name="keyValues">dictionary to turn into query</param>
    /// <param name="expectedString"></param>
    [Theory]
    [MemberData(nameof(ToQueryStringCases))]
    public void ToQueryString(IEnumerable<KeyValuePair<string, object>> keyValues, string expectedString)
    {
        _outputHelper.WriteLine($"input : {keyValues.Jsonify()}");

        // Act
        string queryString = keyValues.ToQueryString();

        // Arrange
        _outputHelper.WriteLine($"Result is '{queryString}'");
        queryString?.Should().Be(expectedString);
    }

    public static TheoryData<IEnumerable<KeyValuePair<string, object>>, Func<string, object, object>, string> ToQueryStringWithTransformationCases
        => new()
        {
            {
                [new KeyValuePair<string, object>("color", 3), new KeyValuePair<string, object>("color", 2)],
                (key, value) =>
                {
                    if (key == "color" && Equals(value, 3))
                    {
                        value = "replacement";
                    }

                    return value;
                },
                "color=replacement&color=2"
            },
            {
                new Dictionary<string, object>
                {
                    ["search"] = new Dictionary<string, object>
                    {
                        ["page"] = 1,
                        ["pageSize"] = 3,
                        ["filter"] = new Dictionary<string, object>
                                {
                                    ["field"] = "firstname",
                                    ["op"] = "eq",
                                    ["value"] = "Bruce"
                                }
                    },
                },
                null,
                $"{Uri.EscapeDataString("search[page]")}=1" +
                $"&{Uri.EscapeDataString("search[pageSize]")}=3" +
                $"&{Uri.EscapeDataString("search[filter][field]")}=firstname" +
                $"&{Uri.EscapeDataString("search[filter][op]")}=eq" +
                $"&{Uri.EscapeDataString("search[filter][value]")}=Bruce"
            },
            {
                [
                    new KeyValuePair<string, object>("search",
                        new[]
                        {
                            new KeyValuePair<string, object>("page", 1),
                            new KeyValuePair<string, object>("pageSize", 3),
                            new KeyValuePair<string, object>("filter",
                                new[]
                                {
                                    new KeyValuePair<string, object>("field", "firstname"),
                                    new KeyValuePair<string, object>("op", "EqualTo"),
                                    new KeyValuePair<string, object>("value", "Bruce"),
                                })
                        })
                ],
                (key, value) =>
                {
                    if (key == "search[filter]")
                    {
                        value = "replacement";
                    }

                    return value;
                },
                $"{Uri.EscapeDataString("search[page]")}=1" +
                $"&{Uri.EscapeDataString("search[pageSize]")}=3" +
                $"&{Uri.EscapeDataString("search[filter]")}=replacement"
            },
            {
                new Dictionary<string, object> { ["name"] = (int[]) [1, 5, 4] },
                (key, value) =>
                {
                    if (key == "name")
                    {
                        value = 10;
                    }

                    return value;
                },
                "name=10"
            }
        };

    /// <summary>
    /// Tests <see cref="System.Collections.Generic.DictionaryExtensions.ToQueryString"/>
    /// </summary>
    /// <param name="keyValues">dictionary to turn into query</param>
    /// <param name="expectedString"></param>
    [Theory]
    [MemberData(nameof(ToQueryStringWithTransformationCases))]
    public void ToQueryStringWithTransformation(IEnumerable<KeyValuePair<string, object>> keyValues,
        Func<string, object, object> transformation,
        string expectedString)
    {
        _outputHelper.WriteLine($"input : {keyValues.Jsonify()}");

        // Act
        string queryString = keyValues?.ToQueryString(transformation);

        // Arrange
        _outputHelper.WriteLine($"Result is '{queryString}'");
        queryString?.Should().Be(expectedString);
    }

    [Property(Arbitrary = [typeof(ValueGenerators)])]
    public void Given_dictionary_that_contains_a_DateOnly_instance_ToQueryString_should_returns_expected_result(
        DateOnly date)
    {
        // Arrange
        IDictionary<string, object> dictionary = new Dictionary<string, object> { ["date-only"] = date };

        //  Act
        string queryString = dictionary.ToQueryString();

        // Assert
        queryString.Should()
            .Be($"date-only={date:yyyy-MM-dd}");
    }

    [Property(Arbitrary = [typeof(ValueGenerators)])]
    public void Given_dictionary_that_contains_a_TimeOnly_instance_ToQueryString_should_returns_expected_result(
        TimeOnly time)
    {
        // Arrange
        IDictionary<string, object> dictionary = new Dictionary<string, object> { ["time-only"] = time };

        //  Act
        string queryString = dictionary.ToQueryString();

        // Assert
        _ = time switch
        {
            { Millisecond: 0 } => queryString.Should().Be($"time-only={time:hh:mm:ss}"),
            _ => queryString.Should().Be($"time-only={time:hh:mm:ss.fffffff}")
        };
    }

    public static TheoryData<Dictionary<string, object>, string, object, object, Dictionary<string, object>>
        GetOrAddCases
        => new()
        {
            { [], "A", "A value", "A value", new Dictionary<string, object>() { { "A", "A value" } } },
            {
                new Dictionary<string, object>() { { "A", "A value" } }, "A",
                "Default value in case the key does not exist", "A value",
                new Dictionary<string, object>() { { "A", "A value" } }
            },
            {
                new Dictionary<string, object>() { { "A", "A value" } }, "B",
                "Default value in case the key does not exist", "Default value in case the key does not exist",
                new Dictionary<string, object>()
                {
                    { "A", "A value" }, { "B", "Default value in case the key does not exist" }
                }
            }
        };

    [Theory]
    [MemberData(nameof(GetOrAddCases))]
    public void Given_dictionary_When_calling_GetOrAdd_then_dictionary_should_have_expected_keys_and_values(
        Dictionary<string, object> dictionary, string key, object value, object expectedValue,
        Dictionary<string, object> expectedDictionary)

    {
        // Act
        object actualValue = dictionary.GetOrAdd(key, value);

        // Assert
        dictionary.Should()
            .ContainKey(key);
        actualValue.Should().Be(expectedValue);
        dictionary.Should().BeEquivalentTo(expectedDictionary);
    }
}