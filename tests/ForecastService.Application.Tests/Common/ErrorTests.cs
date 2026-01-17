using ForecastService.Domain.Common;
using Shouldly;

namespace ForecastService.Application.Tests.Common;

public class ErrorTests
{
    [Fact]
    public void Constructor_ShouldSetCodeAndMessage()
    {
        // Arrange
        var code = "TEST.ERROR";
        var message = "Test error message";

        // Act
        var error = new Error(code, message);

        // Assert
        error.Code.ShouldBe(code);
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void None_ShouldReturnErrorWithEmptyCodeAndMessage()
    {
        // Act
        var error = Error.None;

        // Assert
        error.Code.ShouldBe(string.Empty);
        error.Message.ShouldBe(string.Empty);
    }

    [Fact]
    public void Equals_WhenSameCodeAndMessage_ShouldReturnTrue()
    {
        // Arrange
        var error1 = new Error("TEST.ERROR", "Message");
        var error2 = new Error("TEST.ERROR", "Message");

        // Act
        var result = error1.Equals(error2);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Equals_WhenDifferentCode_ShouldReturnFalse()
    {
        // Arrange
        var error1 = new Error("TEST.ERROR_1", "Message");
        var error2 = new Error("TEST.ERROR_2", "Message");

        // Act
        var result = error1.Equals(error2);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WhenDifferentMessage_ShouldReturnFalse()
    {
        // Arrange
        var error1 = new Error("TEST.ERROR", "Message 1");
        var error2 = new Error("TEST.ERROR", "Message 2");

        // Act
        var result = error1.Equals(error2);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_WhenSameCodeAndMessage_ShouldReturnSameHashCode()
    {
        // Arrange
        var error1 = new Error("TEST.ERROR", "Message");
        var error2 = new Error("TEST.ERROR", "Message");

        // Act
        var hash1 = error1.GetHashCode();
        var hash2 = error2.GetHashCode();

        // Assert
        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void GetHashCode_WhenDifferentErrors_ShouldReturnDifferentHashCodes()
    {
        // Arrange
        var error1 = new Error("TEST.ERROR_1", "Message 1");
        var error2 = new Error("TEST.ERROR_2", "Message 2");

        // Act
        var hash1 = error1.GetHashCode();
        var hash2 = error2.GetHashCode();

        // Assert
        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void EqualityOperator_WhenSameCodeAndMessage_ShouldReturnTrue()
    {
        // Arrange
        var error1 = new Error("TEST.ERROR", "Message");
        var error2 = new Error("TEST.ERROR", "Message");

        // Act
        var result = error1 == error2;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WhenDifferentErrors_ShouldReturnTrue()
    {
        // Arrange
        var error1 = new Error("TEST.ERROR_1", "Message");
        var error2 = new Error("TEST.ERROR_2", "Message");

        // Act
        var result = error1 != error2;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ToString_ShouldContainCodeAndMessage()
    {
        // Arrange
        var error = new Error("TEST.ERROR", "Test message");

        // Act
        var result = error.ToString();

        // Assert
        result.ShouldContain("TEST.ERROR");
        result.ShouldContain("Test message");
    }

    [Fact]
    public void None_WhenCalledMultipleTimes_ShouldReturnSameInstance()
    {
        // Act
        var error1 = Error.None;
        var error2 = Error.None;

        // Assert
        error1.ShouldBe(error2);
        ReferenceEquals(error1, error2).ShouldBeTrue();
    }
}
