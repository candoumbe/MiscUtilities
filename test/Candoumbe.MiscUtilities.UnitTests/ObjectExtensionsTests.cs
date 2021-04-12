﻿using System.Collections.Generic;
using Xunit;
using System;
using FluentAssertions;
using System.Linq.Expressions;
using Xunit.Abstractions;
using static Newtonsoft.Json.JsonConvert;
using FluentAssertions.Extensions;
using Xunit.Categories;

namespace Utilities.UnitTests
{
    /// <summary>
    /// Extensions methods for <see cref="object"/> type.
    /// </summary>
    [UnitTest]
    [Feature(nameof(ObjectExtensions))]
    public class ObjectExtensionsTests
    {
        private readonly ITestOutputHelper _outputHelper;

        /// <summary>
        /// Builds a new <see cref="ObjectExtensionsTests"/> instance.
        /// </summary
        /// <param name="outputHelper"></param>
        public ObjectExtensionsTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        public static IEnumerable<object[]> ToQueryStringCases
        {
            get
            {
                yield return new object[]
                {
                    null,
                    string.Empty
                };

                yield return new object[]
                {
                    new {},
                    string.Empty,
                };
                yield return new object[]
                {
                    new { limit = 1 },
                    "limit=1"
                };

                yield return new object[]
                 {
                    new { limit = 1, offset=3 },
                    "limit=1&offset=3"
                 };

                yield return new object[]
                 {
                    new { c = 1, b=3 , a = 2},
                    "a=2&b=3&c=1"
                 };

                yield return new object[]
                {
                    new {limit = new [] {0, 1, 2, 3}},
                    $"{Uri.EscapeDataString("limit[0]")}=0" +
                    $"&{Uri.EscapeDataString("limit[1]")}=1" +
                    $"&{Uri.EscapeDataString("limit[2]")}=2" +
                    $"&{Uri.EscapeDataString("limit[3]")}=3"
                };

                yield return new object[]
                {
                    new {
                        search = new
                        {
                            filter = new { field = "Firstname", op = "eq", value = "Bruce" }
                        }
                    },
                    $"{Uri.EscapeDataString("search[filter][field]")}=Firstname" +
                    $"&{Uri.EscapeDataString("search[filter][op]")}=eq" +
                    $"&{Uri.EscapeDataString("search[filter][value]")}=Bruce"
                };

                yield return new object[]
                {
                    new
                    {
                        search = new
                        {
                            filter = new { field = "Firstname", op = "eq", value = "Bru&ce" }
                        }
                    },
                    $"{Uri.EscapeDataString("search[filter][field]")}=Firstname" +
                    $"&{Uri.EscapeDataString("search[filter][op]")}=eq" +
                    $"&{Uri.EscapeDataString("search[filter][value]")}={Uri.EscapeDataString("Bru&ce")}"
                };

                yield return new object[]
                {
                    new {
                        search = new
                        {
                            filter = new { field = "Firstname", op = "eq", value = "Bru&ce" }
                        }
                    },
                    $"{Uri.EscapeDataString("search[filter][field]")}=Firstname" +
                    $"&{Uri.EscapeDataString("search[filter][op]")}=eq" +
                    $"&{Uri.EscapeDataString("search[filter][value]")}={Uri.EscapeDataString("Bru&ce")}"
                };
            }
        }

        [Theory]
        [MemberData(nameof(ToQueryStringCases))]
        public void ToQueryString(object input, string expectedString)
        {
            _outputHelper.WriteLine($"input : {input}");

            // Act
            string actual = input.ToQueryString();

            // Assert
            actual.Should().Be(expectedString);
        }

        public static IEnumerable<object[]> ToQueryStringWithTransformCases
        {
            get
            {
                yield return new object[]
                {
                    new { limit = 1 },
                    null,
                    "limit=1"
                };

                yield return new object[]
                 {
                    new { limit = 1, offset=3 },
                    (Func<string, object, object>)((key, value) => key == "limit" ? 2 : value),
                    "limit=2&offset=3"
                 };
            }
        }

        [Theory]
        [MemberData(nameof(ToQueryStringWithTransformCases))]
        public void ToQueryStringWithTransform(object input, Func<string, object, object> transform, string expectedString)
        {
            _outputHelper.WriteLine($"input : {input}");

            // Act
            string actual = input.ToQueryString(transform);

            // Assert
            actual.Should().Be(expectedString);
        }

        public static IEnumerable<object[]> ParseAnonymousObjectCases
        {
            get
            {
                yield return new object[]
                {
                    new {limit = new [] {0, 2, 4}},
                    (Expression<Func<IDictionary<string, object>, bool>>)(x =>
                        x != null
                        && x.Keys.Count == 1
                        && x.ContainsKey("limit")
                        && x["limit"] is IDictionary<string, object>
                        && ((IDictionary<string, object>) x["limit"]).Keys.Count == 3
                        && ((IDictionary<string, object>) x["limit"]).ContainsKey("0")
                        && Equals(((IDictionary<string, object>) x["limit"])["0"], 0)
                        && ((IDictionary<string, object>) x["limit"]).ContainsKey("1")
                        && Equals(((IDictionary<string, object>) x["limit"])["1"], 2)
                        && ((IDictionary<string, object>) x["limit"]).ContainsKey("2")
                        && Equals(((IDictionary<string, object>) x["limit"])["2"], 4)
                    )
                };

                yield return new object[]
                {
                    new { propName = "value" },
                    (Expression<Func<IDictionary<string, object>, bool>>)(x =>
                        x != null
                        && x.Keys.Count == 1
                        && x.ContainsKey("propName") && Equals(x["propName"], "value"))
                };
                yield return new object[]
                {
                    new {
                        prop1 = "value",
                        prop2 = new
                        {
                            subProp1 = "subPropValue"
                        }
                    },
                    (Expression<Func<IDictionary<string, object>, bool>>)(x =>
                        x != null
                        && x.Keys.Count == 2
                        && x.ContainsKey("prop1") && Equals(x["prop1"], "value")

                        && x.ContainsKey("prop2")
                        && x["prop2"] is IDictionary<string, object>
                        && ((IDictionary<string, object>) x["prop2"]).Keys.Count == 1
                        && ((IDictionary<string, object>) x["prop2"]).ContainsKey("subProp1")
                        && Equals(((IDictionary<string, object>) x["prop2"])["subProp1"], "subPropValue"))
                };
                yield return new object[]
                {
                    new
                    {
                        Page = 1,
                        PageSize = 30,
                        Filter = new { field  = "Firstname", @operator = "EqualTo", value = "Bruce" }
                    },
                    (Expression<Func<IDictionary<string, object>, bool>>)(x =>
                        x != null
                        && x.Keys.Count == 3
                        && x.ContainsKey("Page") && Equals(x["Page"], 1)

                        && x.ContainsKey("Filter")
                        && x["Filter"] is IDictionary<string, object>
                        && ((IDictionary<string, object>) x["Filter"]).Keys.Count == 3
                        && ((IDictionary<string, object>) x["Filter"]).ContainsKey("field")
                        && Equals(((IDictionary<string, object>) x["Filter"])["field"], "Firstname")

                        && ((IDictionary<string, object>) x["Filter"]).ContainsKey("operator")
                        && Equals(((IDictionary<string, object>) x["Filter"])["operator"], "EqualTo")

                        && ((IDictionary<string, object>) x["Filter"]).ContainsKey("value")
                        && Equals(((IDictionary<string, object>) x["Filter"])["value"], "Bruce")
                    )
                };

                {
                    DateTimeOffset? from = null;
                    DateTimeOffset? to = 1.February(2001);

                    yield return new object[]
                    {
                        new
                        {
                            from,
                            to
                        },
                        (Expression<Func<IDictionary<string, object>, bool>>)(x =>
                            x != null
                            && x.Keys.Count == 1
                            && x.ContainsKey("to") && Equals(x["to"], to)
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ParseAnonymousObjectCases))]
        public void ParseAnonymousObject(object input, Expression<Func<IDictionary<string, object>, bool>> resultExpectation)
        {
            _outputHelper.WriteLine($"input : {input}");

            //Act
            IDictionary<string, object> result = input?.ParseAnonymousObject();

            // Assert
            _outputHelper.WriteLine($"output : {SerializeObject(result)}");
            result.Should().Match(resultExpectation);
        }

        public static IEnumerable<object[]> AsCases
        {
            get
            {
                yield return new object[]
                {
                    null,
                    typeof(string),
                    (Expression<Func<object, bool>>)(result => result == null),
                    $"{nameof(ObjectExtensions.As)} is null safe"
                };
            }
        }

        [Theory]
        [MemberData(nameof(AsCases))]
        public void As_should_behave_as_expected(object source, Type targetType, Expression<Func<object, bool>> expectation, string reason)
        {
            // Act
            object result = source.As(targetType);

            // Assert
            result.Should()
                  .Match(expectation, reason);
        }
    }
}