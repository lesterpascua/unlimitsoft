using FluentAssertions;
using System;
using UnlimitSoft.Data.EntityFramework.Utility;
using Xunit;

namespace UnlimitSoft.Tests.UnlimitSoft.Data.EntityFramework.Utility;


/// <summary>
/// 
/// </summary>
public sealed class StringToArrayConverterTests
{
    [Fact]
    public void ConvertFromInt_WithEmptyArray_ReturnsEmptyString()
    {
        // Arrange
        var converter = new StringToArrayConverter<int>();

        // Act
        var result = (int[]?)converter.ConvertFromProvider("");

        // Assert
        result.Should().BeEmpty();
    }
    [Fact]
    public void ConvertFromInt_WithIntStringCommaSeparate_ReturnsIntArray()
    {
        // Arrange
        var converter = new StringToArrayConverter<int>();

        // Act
        var result = (int[]?)converter.ConvertFromProvider("33,676,784");

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public void ConvertFromEnum_WithEnumStringCommaSeparate_ReturnsEnumArray()
    {
        // Arrange
        var converter = new StringToArrayConverter<DayOfWeek>();

        // Act
        var result = (DayOfWeek[]?)converter.ConvertFromProvider($"{DayOfWeek.Monday},{DayOfWeek.Saturday}");

        // Assert
        result.Should().HaveCount(2);
    }
    [Fact]
    public void ConvertFromString_WithStringArray_ReturnsStringCommaSeparate()
    {
        // Arrange
        var converter = new StringToArrayConverter<string>();

        // Act
        var result = (string[]?)converter.ConvertFromProvider("2324, sdfsf ,4fgsfg");

        // Assert
        result.Should().HaveCount(3);
    }
}
