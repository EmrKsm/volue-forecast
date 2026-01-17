using ForecastService.Api.Controllers;
using ForecastService.Application.DTOs;
using ForecastService.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Shouldly;

namespace ForecastService.Api.Tests.Common;

public class BaseApiControllerTests
{
    private class TestController : BaseApiController
    {
        public ActionResult<ApiResult<T>> TestHandleError<T>(Result<T> result)
        {
            return HandleError(result);
        }
    }

    [Fact]
    public void HandleError_WithNotFoundError_ShouldReturn404()
    {
        // Arrange
        var controller = new TestController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test";
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        
        var error = new Error("Entity.NotFound", "Entity not found");
        var result = Result.Failure<string>(error);

        // Act
        var actionResult = controller.TestHandleError(result);

        // Assert
        var objectResult = actionResult.Result as ObjectResult;
        objectResult.ShouldNotBeNull();
        objectResult!.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public void HandleError_WithConcurrencyConflict_ShouldReturn409()
    {
        // Arrange
        var controller = new TestController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test";
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        
        var error = new Error("Entity.ConcurrencyConflict", "Concurrency conflict");
        var result = Result.Failure<string>(error);

        // Act
        var actionResult = controller.TestHandleError(result);

        // Assert
        var objectResult = actionResult.Result as ObjectResult;
        objectResult.ShouldNotBeNull();
        objectResult!.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
    }

    [Fact]
    public void HandleError_WithValidationError_ShouldReturn400()
    {
        // Arrange
        var controller = new TestController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test";
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        
        var error = new Error("Validation.Failed", "Validation failed");
        var result = Result.Failure<string>(error);

        // Act
        var actionResult = controller.TestHandleError(result);

        // Assert
        var objectResult = actionResult.Result as ObjectResult;
        objectResult.ShouldNotBeNull();
        objectResult!.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void HandleError_ShouldCreateApiResultWithCorrectStructure()
    {
        // Arrange
        var controller = new TestController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test/path";
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        
        var error = new Error("Test.Error", "Test error message");
        var result = Result.Failure<string>(error);

        // Act
        var actionResult = controller.TestHandleError(result);

        // Assert
        var objectResult = actionResult.Result as ObjectResult;
        objectResult.ShouldNotBeNull();
        
        var apiResult = objectResult!.Value as ApiResult<string>;
        apiResult.ShouldNotBeNull();
        apiResult!.Success.ShouldBeFalse();
        apiResult.Data.ShouldBeNull();
        apiResult.Error.ShouldNotBeNull();
    }

    [Fact]
    public void HandleError_WithNotFoundError_ShouldHaveCorrectProblemDetails()
    {
        // Arrange
        var controller = new TestController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/forecasts/123";
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        
        var error = new Error("Forecast.NotFound", "Forecast not found");
        var result = Result.Failure<string>(error);

        // Act
        var actionResult = controller.TestHandleError(result);

        // Assert
        var objectResult = actionResult.Result as ObjectResult;
        var apiResult = objectResult!.Value as ApiResult<string>;
        var problemDetails = apiResult!.Error;
        
        problemDetails.ShouldNotBeNull();
        problemDetails!.Status.ShouldBe(StatusCodes.Status404NotFound);
        problemDetails.Title.ShouldBe("Not Found");
        problemDetails.Detail.ShouldBe("Forecast not found");
        problemDetails.Instance.ShouldBe("/api/forecasts/123");
    }

    [Fact]
    public void HandleError_WithDatabaseConstraintViolation_ShouldReturn409()
    {
        // Arrange
        var controller = new TestController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test";
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        
        var error = new Error("Database.UniqueConstraintViolation", "Unique constraint violation");
        var result = Result.Failure<string>(error);

        // Act
        var actionResult = controller.TestHandleError(result);

        // Assert
        var objectResult = actionResult.Result as ObjectResult;
        objectResult.ShouldNotBeNull();
        objectResult!.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
    }

    [Fact]
    public void HandleError_WithDatabaseConnectionError_ShouldReturn503()
    {
        // Arrange
        var controller = new TestController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test";
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        
        var error = new Error("Database.ConnectionError", "Database connection failed");
        var result = Result.Failure<string>(error);

        // Act
        var actionResult = controller.TestHandleError(result);

        // Assert
        var objectResult = actionResult.Result as ObjectResult;
        objectResult.ShouldNotBeNull();
        objectResult!.StatusCode.ShouldBe(StatusCodes.Status503ServiceUnavailable);
    }

    [Fact]
    public void HandleError_WithDatabaseTimeoutError_ShouldReturn504()
    {
        // Arrange
        var controller = new TestController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test";
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        
        var error = new Error("Database.TimeoutError", "Database timeout");
        var result = Result.Failure<string>(error);

        // Act
        var actionResult = controller.TestHandleError(result);

        // Assert
        var objectResult = actionResult.Result as ObjectResult;
        objectResult.ShouldNotBeNull();
        objectResult!.StatusCode.ShouldBe(StatusCodes.Status504GatewayTimeout);
    }

    [Fact]
    public void HandleError_WithGenericDatabaseError_ShouldReturn500()
    {
        // Arrange
        var controller = new TestController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test";
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        
        var error = new Error("Something.DatabaseError", "Database error occurred");
        var result = Result.Failure<string>(error);

        // Act
        var actionResult = controller.TestHandleError(result);

        // Assert
        var objectResult = actionResult.Result as ObjectResult;
        objectResult.ShouldNotBeNull();
        objectResult!.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void HandleError_WithUnknownError_ShouldReturn400WithValidationErrorTitle()
    {
        // Arrange
        var controller = new TestController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test";
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        
        var error = new Error("Unknown.Error", "Unknown error");
        var result = Result.Failure<string>(error);

        // Act
        var actionResult = controller.TestHandleError(result);

        // Assert
        var objectResult = actionResult.Result as ObjectResult;
        objectResult.ShouldNotBeNull();
        objectResult!.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        
        var apiResult = objectResult.Value as ApiResult<string>;
        apiResult!.Error!.Title.ShouldBe("Validation Error");
    }
}
