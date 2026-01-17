using ForecastService.Application.DTOs;
using ForecastService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ForecastService.Api.Controllers;

[Route("api/[controller]")]
public class ForecastsController(IForecastService forecastService) : BaseApiController
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

    /// <summary>
    /// Get a specific forecast by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResult<ForecastResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<ForecastResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResult<ForecastResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResult<ForecastResponse>>> GetForecast(
        [Required(ErrorMessage = "Forecast ID is required")] Guid? id)
    {
        var result = await forecastService.GetForecastAsync(id!.Value);

        if (result.IsFailure)
        {
            return HandleError(result);
        }

        return Ok(ApiResult<ForecastResponse>.Ok(result.Value));
    }

    /// <summary>
    /// Get forecasts for a specific power plant within a date range
    /// </summary>
    [HttpGet("power-plant/{powerPlantId}")]
    [ProducesResponseType(typeof(ApiResult<IEnumerable<ForecastResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<IEnumerable<ForecastResponse>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<IEnumerable<ForecastResponse>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResult<IEnumerable<ForecastResponse>>>> GetForecastsByPowerPlant(
        [Required(ErrorMessage = "Power plant ID is required")] Guid? powerPlantId,
        [FromQuery, Required(ErrorMessage = "Start date is required")] DateTime? startDate,
        [FromQuery, Required(ErrorMessage = "End date is required")] DateTime? endDate)
    {
        var result = await forecastService.GetForecastsByPowerPlantAsync(powerPlantId!.Value, startDate!.Value, endDate!.Value);

        if (result.IsFailure)
        {
            return HandleError(result);
        }

        return Ok(ApiResult<IEnumerable<ForecastResponse>>.Ok(result.Value));
    }
}
