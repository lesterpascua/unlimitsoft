using FluentAssertions;
using System;
using System.Linq;
using UnlimitSoft.Json;
using Xunit;

namespace UnlimitSoft.Tests.UnlimitSoft.Json;


public sealed class DefaultJsonSerializerTests
{
    [Fact]
    public void ToKeyValueTest()
    {
        // Arrange
        var serializer = new Text.Json.DefaultJsonSerializer();

        // Act
        var qs = serializer.ToKeyValue(new { a = "a", b = "b", c = "c", obj = new { a = "a", b = "b" } }, null, false)!;

        // Assert
        qs.Should().NotBeNull();
        qs.Count.Should().Be(5);
        qs["a"].Should().Be("a");
        qs["b"].Should().Be("b");
        qs["c"].Should().Be("c");
        qs["obj.a"].Should().Be("a");
        qs["obj.b"].Should().Be("b");
    }
    [Theory]
    [InlineData(typeof(Text.Json.DefaultJsonSerializer))]
    [InlineData(typeof(Newtonsoft.DefaultJsonSerializer))]
    public void ToKeyValue_WithObjectWithEmptyPropertiesAndCleanSetTrue_DictionaryWillReturn(Type type)
    {
        // Arrange
        var value = new
        {
            wrapperElement = new
            {
                data = (string?)null,
                count = 2,
                elementName = new[] {
                    new { id = 3, name = "value1" },
                    new { id = 4, name  = "value2" },
                }
            }
        };
        var serializer = (IJsonSerializer)Activator.CreateInstance(type)!;


        // Act
        var data = serializer.ToKeyValue(value, clean: true)!;


        // Assert
        data.Should().HaveCount(5);
        data.ContainsKey("wrapperElement.data").Should().BeFalse();

        data["wrapperElement.count"].Should().Be("2");
        data["wrapperElement.elementName[0].id"].Should().Be("3");
        data["wrapperElement.elementName[0].name"].Should().Be("value1");
        data["wrapperElement.elementName[1].id"].Should().Be("4");
        data["wrapperElement.elementName[1].name"].Should().Be("value2");
    }
    [Theory]
    [InlineData(typeof(Text.Json.DefaultJsonSerializer))]
    [InlineData(typeof(Newtonsoft.DefaultJsonSerializer))]
    public void ToKeyValue_WithObjectWithEmptyPropertiesAndCleanSetFalse_DictionaryWillReturn(Type type)
    {
        // Arrange
        var value = new
        {
            wrapperElement = new
            {
                data = "",
                count = 2,
                elementName = new[] {
                    new { id = 3, name = "value1" },
                    new { id = 4, name  = "value2" },
                }
            }
        };
        var serializer = (IJsonSerializer)Activator.CreateInstance(type)!;


        // Act
        var data = serializer.ToKeyValue(value, clean: false)!;


        // Assert
        data.Should().HaveCount(6);

        data["wrapperElement.data"].Should().Be("");
        data["wrapperElement.count"].Should().Be("2");
        data["wrapperElement.elementName[0].id"].Should().Be("3");
        data["wrapperElement.elementName[0].name"].Should().Be("value1");
        data["wrapperElement.elementName[1].id"].Should().Be("4");
        data["wrapperElement.elementName[1].name"].Should().Be("value2");
    }

    [Theory]
    [InlineData(typeof(Text.Json.DefaultJsonSerializer))]
    [InlineData(typeof(Newtonsoft.DefaultJsonSerializer))]
    public void EnumerateJson_WithMultiplesChildLevel_SameResultInAllJsonFrameworks(Type type)
    {
        // Arrange
        var value = new
        {
            wrapperElement = new
            {
                count = 2,
                elementName = new[] {
                    new { id = 3, name = "value1" },
                    new { id = 4, name  = "value2" },
                }
            }
        };
        const string WrapperElement = nameof(value.wrapperElement), ElementName = nameof(value.wrapperElement.elementName), CountName = nameof(value.wrapperElement.count);

        var serializer = (IJsonSerializer)Activator.CreateInstance(type);
        var json = serializer.Deserialize<object>(serializer.Serialize(value));


        // Act
        var rootToken = serializer.GetEnumerable(json).First();
        var wrapperToken = serializer.GetEnumerable(rootToken).First();
        var countToken = serializer.GetEnumerable(serializer.GetEnumerable(wrapperToken).First()).First();
        var elementToken = serializer.GetEnumerable(serializer.GetEnumerable(wrapperToken).Skip(1).First()).First();


        // Assert
        #region Check First Level
        var token = serializer.GetToken(json, WrapperElement); token.Should().NotBeNull();
        var element = serializer.GetValue<object>(json, WrapperElement); element.Should().NotBeNull();
        token.Should().Be(element);

        wrapperToken.Should().Be(token);
        #endregion

        #region Check 2 Level Count
        token = serializer.GetToken(json, WrapperElement, CountName); token.Should().NotBeNull();
        var countValue = serializer.GetValue<int>(json, WrapperElement, CountName); countValue.Should().Be(value.wrapperElement.count);
        var name = serializer.GetName(serializer.GetEnumerable(wrapperToken).First()); name.Should().Be(CountName);
        serializer.GetTokenType(countToken).Should().Be(TokenType.Number);
        serializer.GetTokenValue<int>(token).Should().Be(countValue);
        serializer.GetTokenValue<int>(countToken).Should().Be(countValue);
        #endregion

        #region Check 2 Level Element
        token = serializer.GetToken(json, WrapperElement, ElementName); token.Should().NotBeNull();
        var elementValue = serializer.GetValue<object>(json, WrapperElement, ElementName);
        name = serializer.GetName(serializer.GetEnumerable(wrapperToken).Skip(1).First()); name.Should().Be(ElementName);
        serializer.GetTokenType(elementToken).Should().Be(TokenType.Array);

        elementValue.Should().Be(elementToken);
        #endregion

        #region Check Array Level Element
        var array = serializer.GetEnumerable(elementToken);
        array.Should().HaveCount(2);
        #endregion
    }


    [Theory]
    [InlineData(typeof(Text.Json.DefaultJsonSerializer))]
    [InlineData(typeof(Newtonsoft.DefaultJsonSerializer))]
    public void GetGuidValue_WithValidGuid_ShouldBeAbleToConvert(Type type)
    {
        // Arrange
        var value = new
        {
            wrapperElement = new
            {
                count = 2,
                elementName = new[] {
                    new { id = 3, name = "value1" },
                    new { id = 4, name  = "value2" },
                }
            }
        };
        const string WrapperElement = nameof(value.wrapperElement), ElementName = nameof(value.wrapperElement.elementName), CountName = nameof(value.wrapperElement.count);

        var serializer = (IJsonSerializer)Activator.CreateInstance(type);
        var json = serializer.Deserialize<object>(serializer.Serialize(value));


        // Act
        var rootToken = serializer.GetEnumerable(json).First();
        var wrapperToken = serializer.GetEnumerable(rootToken).First();
        var countToken = serializer.GetEnumerable(serializer.GetEnumerable(wrapperToken).First()).First();
        var elementToken = serializer.GetEnumerable(serializer.GetEnumerable(wrapperToken).Skip(1).First()).First();


        // Assert
        #region Check First Level
        var token = serializer.GetToken(json, WrapperElement); token.Should().NotBeNull();
        var element = serializer.GetValue<object>(json, WrapperElement); element.Should().NotBeNull();
        token.Should().Be(element);

        wrapperToken.Should().Be(token);
        #endregion

        #region Check 2 Level Count
        token = serializer.GetToken(json, WrapperElement, CountName); token.Should().NotBeNull();
        var countValue = serializer.GetValue<int>(json, WrapperElement, CountName); countValue.Should().Be(value.wrapperElement.count);
        var name = serializer.GetName(serializer.GetEnumerable(wrapperToken).First()); name.Should().Be(CountName);
        serializer.GetTokenType(countToken).Should().Be(TokenType.Number);
        serializer.GetTokenValue<int>(token).Should().Be(countValue);
        serializer.GetTokenValue<int>(countToken).Should().Be(countValue);
        #endregion

        #region Check 2 Level Element
        token = serializer.GetToken(json, WrapperElement, ElementName); token.Should().NotBeNull();
        var elementValue = serializer.GetValue<object>(json, WrapperElement, ElementName);
        name = serializer.GetName(serializer.GetEnumerable(wrapperToken).Skip(1).First()); name.Should().Be(ElementName);
        serializer.GetTokenType(elementToken).Should().Be(TokenType.Array);

        elementValue.Should().Be(elementToken);
        #endregion

        #region Check Array Level Element
        var array = serializer.GetEnumerable(elementToken);
        array.Should().HaveCount(2);
        #endregion
    }


}
