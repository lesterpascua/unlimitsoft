using Bogus;
using FluentAssertions;
using System;
using System.Linq;
using UnlimitSoft.Extensions;
using Xunit;

namespace UnlimitSoft.Tests.UnlimitSoft.Extensions;


public sealed class StringExtensionsTests
{
    [Fact]
    public void CallToEnumArray_WithEnumAsInt_ConvertSuccess()
    {
        // Arrange
        var faker = new Faker();
        var array = faker.System.Random.ArrayElements(Enum.GetValues<Names>(), 3);
        var str = array.Select(s => Convert.ToString((int)s)).Aggregate((a, b) => $"{a},{b}");

        // Act
        var result = str.ToEnumArray<Names>();

        // Assert
        result.Should().BeEquivalentTo(array);
    }
    [Fact]
    public void CallToEnumArray_WithEnumAsString_ConvertSuccess()
    {
        // Arrange
        var faker = new Faker();
        var array = faker.System.Random.ArrayElements(Enum.GetValues<Names>(), 3);
        var str = array.Select(s => Convert.ToString(s)).Aggregate((a, b) => $"{a},{b}");

        // Act
        var result = str.ToEnumArray<Names>();

        // Assert
        result.Should().BeEquivalentTo(array);
    }

    private enum Names
    {
        Jhon = 1,
        Michael = 2,
        Sheppart = 3,
        Juan = 4,
        Elena = 5
    }
}
