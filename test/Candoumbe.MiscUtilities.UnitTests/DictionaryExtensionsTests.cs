﻿using FluentAssertions;
using FluentAssertions.Extensions;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

using static System.StringSplitOptions;

namespace Utilities.UnitTests
{
    /// <summary>
    /// unit tests for <see cref="DictionaryExtensions"/> methods.
    /// </summary>
    [UnitTest]
    [Feature("IDictionary")]
    public class DictionaryExtensionsTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public DictionaryExtensionsTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        public static IEnumerable<object[]> ToQueryStringCases
        {
            get
            {
                yield return new object[]
                {
                    null,
                    (Expression<Func<string, bool>>)(x => x == string.Empty)
                };

                yield return new object[]
                {
                    new Dictionary<string, object>(),
                    (Expression<Func<string, bool>>)(x => x == string.Empty),
                };
                yield return new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["limit"] = 1
                    },
                    (Expression<Func<string, bool>>)(x => "limit=1".Equals(x))
                };

                yield return new object[]
                {
                    new []
                    {
                        new KeyValuePair<string, object>("limit", null)
                    },
                    (Expression<Func<string, bool>>)(x => x == string.Empty)
                };

                yield return new object[]
                {
                    new []
                    {
                        new KeyValuePair<string, object>("color", 3),
                        new KeyValuePair<string, object>("color", 2),
                    },
                    (Expression<Func<string, bool>>)(x => x == "color=2&color=3")
                };

                yield return new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["date"] = 1.February(2010).AddMinutes(30)
                    },
                    (Expression<Func<string, bool>>)(x => "date=2010-02-01T00:30:00".Equals(x))
                };

                yield return new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["date"] = 1.February(2010)
                    },
                    (Expression<Func<string, bool>>)(x => "date=2010-02-01T00:00:00".Equals(x))
                };

                yield return new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["date-with-offset"] = new DateTimeOffset(1.February(2010).Add(11.Hours()), 1.Hours())
                    },
                    (Expression<Func<string, bool>>)(x => "date-with-offset=2010-02-01T11:00:00".Equals(x))
                };

                yield return new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["offset"] = 3,
                        ["limit"] = 3
                    },
                    (Expression<Func<string, bool>>)(x => "limit=3&offset=3".Equals(x))
                };

                yield return new object[]
                {
                    new Dictionary<string, object>{
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
                    (Expression<Func<string, bool>>)( queryString =>
                        queryString != null
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Length == 5
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[page]")}=1")
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[pageSize]")}=3")
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter][field]")}=firstname")
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter][op]")}=eq")
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter][value]")}=Bruce")
                )
                    };

                yield return new object[]
                {
                    new[]
                    {
                        new KeyValuePair<string, object>("search", new []
                        {
                            new KeyValuePair<string, object>("page", 1),
                            new KeyValuePair<string, object>("pageSize", 3),
                            new KeyValuePair<string, object>("filter", new []
                            {
                                new KeyValuePair<string, object>("field", "firstname"),
                                new KeyValuePair<string, object>("op", "EqualTo"),
                                new KeyValuePair<string, object>("value", "Bruce"),
                            })
                        })
                    },
                    (Expression<Func<string, bool>>)( queryString =>
                        queryString != null
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Length == 5
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[page]")}=1")
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[pageSize]")}=3")
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter][field]")}=firstname")
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter][op]")}=EqualTo")
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter][value]")}=Bruce")
                    )
                };

                yield return new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["name"] = new []{ 1, 5, 4 }
                    },
                    (Expression<Func<string, bool>>)( queryString =>
                        queryString != null
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Length == 3
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("name[0]")}=1")
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("name[1]")}=5")
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("name[2]")}=4")
                    )
                };
            }
        }

        /// <summary>
        /// Tests <see cref="System.Collections.Generic.DictionaryExtensions.ToQueryString(IEnumerable{KeyValuePair{string, object}})"/>
        /// </summary>
        /// <param name="keyValues">dictionary to turn into query</param>
        /// <param name="expectedString"></param>
        [Theory]
        [MemberData(nameof(ToQueryStringCases))]
        public void ToQueryString(IEnumerable<KeyValuePair<string, object>> keyValues, Expression<Func<string, bool>> expectedString)
        {
            _outputHelper.WriteLine($"input : {keyValues.Jsonify()}");

            // Act
            string queryString = DictionaryExtensions.ToQueryString(keyValues, null);

            // Arrange
            _outputHelper.WriteLine($"Result is '{queryString}'");
            queryString?.Should()
                        .Match(expectedString);
        }

        public static IEnumerable<object[]> ToQueryStringWithTransformationCases
        {
            get
            {
                yield return new object[]
                {
                    new []
                    {
                        new KeyValuePair<string, object>("color", 3),
                        new KeyValuePair<string, object>("color", 2),
                    },
                    (Func<string, object, object>)((key, value) =>
                    {
                        if (key == "color" && Equals(value,3))
                        {
                            value = "replacement";
	                    }

                        return value;
                    }),
                    (Expression<Func<string, bool>>) (queryString => queryString != null
                                                                     && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Length == 2
                                                                     && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == "color=replacement")
                                                                     && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == "color=2"))
                };

                yield return new object[]
                {
                    new Dictionary<string, object>{
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
                    (Func<string, object, object>)null,
                    (Expression<Func<string, bool>>)( queryString => queryString != null
                                                                     && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Length == 5
                                                                     && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[page]")}=1")
                                                                     && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[pageSize]")}=3")
                                                                     && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter][field]")}=firstname")
                                                                     && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter][op]")}=eq")
                                                                     && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter][value]")}=Bruce"))
                    };

                yield return new object[]
                {
                    new[]
                    {
                        new KeyValuePair<string, object>("search", new []
                        {
                            new KeyValuePair<string, object>("page", 1),
                            new KeyValuePair<string, object>("pageSize", 3),
                            new KeyValuePair<string, object>("filter", new []
                            {
                                new KeyValuePair<string, object>("field", "firstname"),
                                new KeyValuePair<string, object>("op", "EqualTo"),
                                new KeyValuePair<string, object>("value", "Bruce"),
                            })
                        })
                    },
                    (Func<string, object, object>)((key, value) =>
                    {
                        if (key == "search[filter]")
                        {
                            value = "replacement";
                        }

                        return value;
                    }),
                    (Expression<Func<string, bool>>)( queryString => queryString != null
                                                                     && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Length == 3
                                                                     && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[page]")}=1")
                                                                     && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[pageSize]")}=3")
                                                                     && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter]")}=replacement")
                    )
                };

                yield return new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["name"] = new []{ 1, 5, 4 }
                    },
                    (Func<string, object, object>)((key, value) =>
                    {
                        if (key == "name")
                        {
                            value = 10;
                        }

                        return value;
                    }),
                    (Expression<Func<string, bool>>)( queryString =>
                        queryString != null
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Length == 1
                        && queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("name")}=10")
                    )
                };
            }
        }

        /// <summary>
        /// Tests <see cref="System.Collections.Generic.DictionaryExtensions.ToQueryString(IEnumerable{KeyValuePair{string, object}})"/>
        /// </summary>
        /// <param name="keyValues">dictionary to turn into query</param>
        /// <param name="expectedString"></param>
        [Theory]
        [MemberData(nameof(ToQueryStringWithTransformationCases))]
        public void ToQueryStringWithTransformation(IEnumerable<KeyValuePair<string, object>> keyValues,
                                                    Func<string, object, object> transformation,
                                                    Expression<Func<string, bool>> expectedString)
        {
            _outputHelper.WriteLine($"input : {keyValues.Jsonify()}");

            // Act
            string queryString = keyValues?.ToQueryString(transformation);

            // Arrange
            _outputHelper.WriteLine($"Result is '{queryString}'");
            queryString?.Should()
                        .Match(expectedString);
        }
    }
}
