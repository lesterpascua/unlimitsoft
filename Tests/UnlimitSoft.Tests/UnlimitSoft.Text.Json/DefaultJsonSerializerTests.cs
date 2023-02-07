using FluentAssertions;
using UnlimitSoft.Text.Json;
using Xunit;

namespace UnlimitSoft.Tests.UnlimitSoft.Text.Json;


public sealed class DefaultJsonSerializerTests
{
    [Fact]
    public void ToKeyValueTest()
    {
        // Arrange
        var serializer = new DefaultJsonSerializer();

        // Act
        var qs = serializer.ToKeyValue(new { a = "a", b = "b", c = "c", obj = new { a = "a", b = "b" } });

        // Assert
        qs.Should().NotBeNull();
        qs.Count.Should().Be(5);
        qs["a"].Should().Be("a");
        qs["b"].Should().Be("b");
        qs["c"].Should().Be("c");
        qs["obj.a"].Should().Be("a");
        qs["obj.b"].Should().Be("b");
    }
}
