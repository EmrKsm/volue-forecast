using ForecastService.Domain.Errors;
using Shouldly;

namespace ForecastService.Application.Tests.Errors;

public class DomainErrorsTests
{
    [Fact]
    public void ForecastErrors_NotFound_ShouldIncludeId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var error = ForecastErrors.NotFound(id);

        // Assert
        error.Code.ShouldBe("Forecast.NotFound");
        error.Message.ShouldContain(id.ToString());
    }

    [Fact]
    public void ForecastErrors_NegativeProduction_ShouldHaveCorrectCodeAndMessage()
    {
        // Act
        var error = ForecastErrors.NegativeProduction;

        // Assert
        error.Code.ShouldBe("Forecast.NegativeProduction");
        error.Message.ShouldBe("Production value cannot be negative");
    }

    [Fact]
    public void ForecastErrors_InvalidDateRange_ShouldHaveCorrectCodeAndMessage()
    {
        // Act
        var error = ForecastErrors.InvalidDateRange;

        // Assert
        error.Code.ShouldBe("Forecast.InvalidDateRange");
        error.Message.ShouldContain("Start date");
    }

    [Fact]
    public void ForecastErrors_ConcurrencyConflict_ShouldHaveCorrectCodeAndMessage()
    {
        // Act
        var error = ForecastErrors.ConcurrencyConflict;

        // Assert
        error.Code.ShouldBe("Forecast.ConcurrencyConflict");
        error.Message.ShouldContain("modified by another user");
    }

    [Fact]
    public void ForecastErrors_DatabaseError_ShouldHaveCorrectCodeAndMessage()
    {
        // Act
        var error = ForecastErrors.DatabaseError;

        // Assert
        error.Code.ShouldBe("Forecast.DatabaseError");
        error.Message.ShouldContain("database error");
    }

    [Fact]
    public void PowerPlantErrors_NotFound_ShouldIncludeId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var error = PowerPlantErrors.NotFound(id);

        // Assert
        error.Code.ShouldBe("PowerPlant.NotFound");
        error.Message.ShouldContain(id.ToString());
    }

    [Fact]
    public void PowerPlantErrors_ConcurrencyConflict_ShouldHaveCorrectCodeAndMessage()
    {
        // Act
        var error = PowerPlantErrors.ConcurrencyConflict;

        // Assert
        error.Code.ShouldBe("PowerPlant.ConcurrencyConflict");
        error.Message.ShouldContain("power plant");
    }

    [Fact]
    public void CompanyErrors_NotFound_ShouldIncludeId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var error = CompanyErrors.NotFound(id);

        // Assert
        error.Code.ShouldBe("Company.NotFound");
        error.Message.ShouldContain(id.ToString());
    }

    [Fact]
    public void CompanyErrors_ConcurrencyConflict_ShouldHaveCorrectCodeAndMessage()
    {
        // Act
        var error = CompanyErrors.ConcurrencyConflict;

        // Assert
        error.Code.ShouldBe("Company.ConcurrencyConflict");
        error.Message.ShouldContain("company");
    }

    [Fact]
    public void DatabaseErrors_ConnectionError_ShouldHaveCorrectCodeAndMessage()
    {
        // Act
        var error = DatabaseErrors.ConnectionError;

        // Assert
        error.Code.ShouldBe("Database.ConnectionError");
        error.Message.ShouldContain("connect to the database");
    }

    [Fact]
    public void DatabaseErrors_TimeoutError_ShouldHaveCorrectCodeAndMessage()
    {
        // Act
        var error = DatabaseErrors.TimeoutError;

        // Assert
        error.Code.ShouldBe("Database.TimeoutError");
        error.Message.ShouldContain("timed out");
    }

    [Fact]
    public void DatabaseErrors_ConstraintViolation_ShouldIncludeDetails()
    {
        // Arrange
        var details = "Unique key constraint failed";

        // Act
        var error = DatabaseErrors.ConstraintViolation(details);

        // Assert
        error.Code.ShouldBe("Database.ConstraintViolation");
        error.Message.ShouldContain(details);
    }

    [Fact]
    public void DatabaseErrors_UniqueConstraintViolation_ShouldHaveCorrectCodeAndMessage()
    {
        // Act
        var error = DatabaseErrors.UniqueConstraintViolation;

        // Assert
        error.Code.ShouldBe("Database.UniqueConstraintViolation");
        error.Message.ShouldContain("unique value already exists");
    }

    [Fact]
    public void DatabaseErrors_ForeignKeyViolation_ShouldHaveCorrectCodeAndMessage()
    {
        // Act
        var error = DatabaseErrors.ForeignKeyViolation;

        // Assert
        error.Code.ShouldBe("Database.ForeignKeyViolation");
        error.Message.ShouldContain("foreign key constraint");
    }

    [Fact]
    public void ForecastErrors_NotFound_WhenCalledMultipleTimes_ShouldReturnNewInstances()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        // Act
        var error1 = ForecastErrors.NotFound(id1);
        var error2 = ForecastErrors.NotFound(id2);

        // Assert
        error1.ShouldNotBe(error2);
        error1.Message.ShouldNotBe(error2.Message);
    }

    [Fact]
    public void AllErrorCodes_ShouldFollowNamingConvention()
    {
        // Arrange & Act & Assert
        ForecastErrors.NotFound(Guid.NewGuid()).Code.ShouldStartWith("Forecast.");
        ForecastErrors.NegativeProduction.Code.ShouldStartWith("Forecast.");
        ForecastErrors.InvalidDateRange.Code.ShouldStartWith("Forecast.");
        
        PowerPlantErrors.NotFound(Guid.NewGuid()).Code.ShouldStartWith("PowerPlant.");
        
        CompanyErrors.NotFound(Guid.NewGuid()).Code.ShouldStartWith("Company.");
        
        DatabaseErrors.ConnectionError.Code.ShouldStartWith("Database.");
        DatabaseErrors.TimeoutError.Code.ShouldStartWith("Database.");
    }
}
