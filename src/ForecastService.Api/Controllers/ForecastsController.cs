using ForecastService.Application.DTOs;
using ForecastService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForecastService.Api.Controllers;

[Route("api/[controller]")]
public class ForecastsController(
    IForecastService forecastService,
    ILogger<ForecastsController> logger) : BaseApiController
{

    /// <summary>
    /// Create or update a forecast for a power plant
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<ForecastResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<ForecastResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResult<ForecastResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<ForecastResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResult<ForecastResponse>>> CreateOrUpdateForecast(
        [FromBody] CreateOrUpdateForecastRequest request)
    {
        try
        {
            var result = await forecastService.CreateOrUpdateForecastAsync(request);

            if (result.IsFailure)
            {
                return HandleError(result);
            }

            var forecast = result.Value;
            // Check if it's a new forecast or updated one based on timestamps
            var isNew = forecast.CreatedAt == forecast.UpdatedAt;
            var apiResult = ApiResult<ForecastResponse>.Ok(forecast);

            if (isNew)
            {
                return CreatedAtAction(
                    nameof(GetForecast),
                    new { id = forecast.Id },
                    apiResult);
            }

            return Ok(apiResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating/updating forecast");
            return StatusCode(500, ApiResult<ForecastResponse>.Fail(
                "Internal Server Error",
                "An error occurred while processing your request. Please try again later.",
                StatusCodes.Status500InternalServerError,
                HttpContext.Request.Path));
        }
    }

    /// <summary>
    /// Get a specific forecast by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResult<ForecastResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<ForecastResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResult<ForecastResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResult<ForecastResponse>>> GetForecast(Guid id)
    {
        try
        {
            var result = await forecastService.GetForecastAsync(id);

            if (result.IsFailure)
            {
                return HandleError(result);
            }

            return Ok(ApiResult<ForecastResponse>.Ok(result.Value));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving forecast {ForecastId}", id);
            return StatusCode(500, ApiResult<ForecastResponse>.Fail(
                "Internal Server Error",
                "An error occurred while processing your request. Please try again later.",
                StatusCodes.Status500InternalServerError,
                HttpContext.Request.Path));
        }
    }

    /// <summary>
    /// Get forecasts for a specific power plant within a date range
    /// </summary>
    [HttpGet("power-plant/{powerPlantId}")]
    [ProducesResponseType(typeof(ApiResult<IEnumerable<ForecastResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<IEnumerable<ForecastResponse>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<IEnumerable<ForecastResponse>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResult<IEnumerable<ForecastResponse>>>> GetForecastsByPowerPlant(
        Guid powerPlantId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var result = await forecastService.GetForecastsByPowerPlantAsync(powerPlantId, startDate, endDate);

            if (result.IsFailure)
            {
                return HandleError(result);
            }

            return Ok(ApiResult<IEnumerable<ForecastResponse>>.Ok(result.Value));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving forecasts for power plant {PowerPlantId}", powerPlantId);
            return StatusCode(500, ApiResult<IEnumerable<ForecastResponse>>.Fail(
                "Internal Server Error",
                "An error occurred while processing your request. Please try again later.",
                StatusCodes.Status500InternalServerError,
                HttpContext.Request.Path));
        }
    }
}
