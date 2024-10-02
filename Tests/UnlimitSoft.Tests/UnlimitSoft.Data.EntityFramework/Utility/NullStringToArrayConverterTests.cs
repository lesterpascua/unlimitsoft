using FluentAssertions;
using UnlimitSoft.Data.EntityFramework.Utility;
using Xunit;

namespace UnlimitSoft.Tests.UnlimitSoft.Data.EntityFramework.Utility;


/// <summary>
/// 
/// </summary>
public sealed class NullStringToArrayConverterTests
{
    [Fact]
    public void ConvertFromInt_WithEmptyArray_ReturnsNull()
    {
        // Arrange
        var converter = new NullStringToArrayConverter<int>();

        // Act
        var result = (int[]?)converter.ConvertFromProvider("");

        // Assert
        result.Should().BeNull();
    }
}
