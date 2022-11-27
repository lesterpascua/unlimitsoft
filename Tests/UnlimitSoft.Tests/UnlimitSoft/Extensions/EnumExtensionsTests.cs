using Bogus;
using FluentAssertions;
using System;
using System.Linq;
using UnlimitSoft.Extensions;
using Xunit;

namespace UnlimitSoft.Tests.UnlimitSoft.Extensions;

public class EnumExtensionsTests
{
    [Fact]
    public void CallToStringSeparator_WithEnumAsInt_ConvertSuccess()
    {
        // Arrange
        var faker = new Faker();
        var array = faker.System.Random.ArrayElements(Enum.GetValues<Names>(), 3);

        // Act
        var str = array.ToStringSeparator();

        // Assert
        str.Should().Be(array.Select(s => Convert.ToString((int)s)).Aggregate((a, b) => $"{a},{b}"));
    }
    [Fact]
    public void CallToStringSeparator_WithEnumAsString_ConvertSuccess()
    {
        // Arrange
        var faker = new Faker();
        var array = faker.System.Random.ArrayElements(Enum.GetValues<Names>(), 3);

        // Act
        var str = array.ToStringSeparator(useInt: false);

        // Assert
        str.Should().Be(array.Select(s => Convert.ToString(s)).Aggregate((a, b) => $"{a},{b}"));
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
