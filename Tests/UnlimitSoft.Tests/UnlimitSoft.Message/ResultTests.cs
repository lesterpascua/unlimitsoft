using FluentAssertions;
using System.Collections.Generic;
using UnlimitSoft.Message;
using Xunit;

namespace UnlimitSoft.Tests.UnlimitSoft.Message;


/// <summary>
/// 
/// </summary>
public sealed class ResultTests
{
    [Fact]
    public void CreateResult_WithOkValue_SuccessShouldBeTrue()
    {
        // Arrange
        var value = "some test";

        // Act
        var result = Result.Ok(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
        result.Error.Should().BeNull();
    }
    [Fact]
    public void CreateResult_WithErrorValue_SuccessShouldBeFalse()
    {
        // Arrange
        var err = new ErrorResponse(System.Net.HttpStatusCode.InternalServerError, new Dictionary<string, string[]>());

        // Act
        var result = Result.Err<string>(err);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be(err);
    }
}
