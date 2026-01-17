using ForecastService.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using System.Text.Json;

namespace ForecastService.Api.Tests.Middleware;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _logger = Substitute.For<ILogger<GlobalExceptionHandlerMiddleware>>();
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNextMiddleware()
    {
        // Arrange
        var wasCalled = false;
        RequestDelegate next = _ =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, _logger);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        wasCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Test exception");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.ShouldBe(500);
        context.Response.ContentType.ShouldBe("application/json");
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ShouldReturnJsonErrorResponse()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Test exception");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        responseText.ShouldNotBeEmpty();

        var responseJson = JsonDocument.Parse(responseText);
        responseJson.RootElement.GetProperty("success").GetBoolean().ShouldBeFalse();
        responseJson.RootElement.GetProperty("error").GetProperty("status").GetInt32().ShouldBe(500);
        responseJson.RootElement.GetProperty("error").GetProperty("title").GetString().ShouldBe("Internal Server Error");
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ShouldLogError()
    {
        // Arrange
        var exception = new Exception("Test exception");
        RequestDelegate next = _ => throw exception;

        var middleware = new GlobalExceptionHandlerMiddleware(next, _logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Method = "GET";
        context.Request.Path = "/api/test";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ShouldIncludeRequestPath()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Test exception");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/forecasts";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var responseJson = JsonDocument.Parse(responseText);
        
        responseJson.RootElement.GetProperty("error").GetProperty("instance").GetString()
            .ShouldBe("/api/forecasts");
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ShouldIncludeTimestamp()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Test exception");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var responseJson = JsonDocument.Parse(responseText);
        
        var timestamp = responseJson.RootElement.GetProperty("error").GetProperty("timestamp").GetString();
        timestamp.ShouldNotBeNullOrEmpty();
        DateTime.TryParse(timestamp, out _).ShouldBeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ShouldHideInternalErrorDetails()
    {
        // Arrange
        var sensitiveException = new Exception("Database connection string: Server=secret;Password=12345");
        RequestDelegate next = _ => throw sensitiveException;

        var middleware = new GlobalExceptionHandlerMiddleware(next, _logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        
        responseText.ShouldNotContain("Database connection string");
        responseText.ShouldNotContain("Password");
        responseText.ShouldContain("An error occurred while processing your request");
    }
}
