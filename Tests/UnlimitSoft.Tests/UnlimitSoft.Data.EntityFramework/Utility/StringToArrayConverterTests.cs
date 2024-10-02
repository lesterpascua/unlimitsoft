using FluentAssertions;
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
}
