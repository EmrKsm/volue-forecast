using ForecastService.Domain.Common;
using Shouldly;

namespace ForecastService.Application.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResultWithoutValue()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBe(Error.None);
    }

    [Fact]
    public void Failure_WhenProvidedError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(error);
    }

    [Fact]
    public void Success_WithValue_ShouldCreateSuccessfulResultWithValue()
    {
        // Arrange
        var value = "Test Value";

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldBe(value);
        result.Error.ShouldBe(Error.None);
    }

    [Fact]
    public void Failure_WithGenericType_ShouldCreateFailedResultWithoutValue()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error message");

        // Act
        var result = Result.Failure<string>(error);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(error);
    }

    [Fact]
    public void Value_WhenResultIsFailure_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error message");
        var result = Result.Failure<string>(error);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => result.Value);
        exception.Message.ShouldBe("Cannot access value of a failed result");
    }

    [Fact]
    public void Success_WithComplexObject_ShouldStoreObjectCorrectly()
    {
        // Arrange
        var complexObject = new { Id = 1, Name = "Test", Active = true };

        // Act
        var result = Result.Success(complexObject);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(1);
        result.Value.Name.ShouldBe("Test");
        result.Value.Active.ShouldBeTrue();
    }

    [Fact]
    public void Success_WithNull_ShouldAllowNullValue()
    {
        // Act
        var result = Result.Success<string?>(null);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    [Fact]
    public void Failure_WhenCalledMultipleTimes_ShouldCreateIndependentResults()
    {
        // Arrange
        var error1 = new Error("ERROR_1", "First error");
        var error2 = new Error("ERROR_2", "Second error");

        // Act
        var result1 = Result.Failure(error1);
        var result2 = Result.Failure(error2);

        // Assert
        result1.Error.ShouldBe(error1);
        result2.Error.ShouldBe(error2);
        result1.Error.ShouldNotBe(result2.Error);
    }
}
