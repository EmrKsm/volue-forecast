using ForecastService.Application.DTOs;
using ForecastService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForecastService.Api.Controllers;

[Route("api/[controller]")]
public class CompanyPositionController(
    ICompanyPositionService companyPositionService,
    ILogger<CompanyPositionController> logger) : BaseApiController
{

    /// <summary>
    /// Get the aggregated position for a company across all its power plants
    /// </summary>
    [HttpGet("{companyId}")]
    [ProducesResponseType(typeof(ApiResult<CompanyPositionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<CompanyPositionResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<CompanyPositionResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResult<CompanyPositionResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResult<CompanyPositionResponse>>> GetCompanyPosition(
        Guid companyId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var result = await companyPositionService.GetCompanyPositionAsync(companyId, startDate, endDate);

            if (result.IsFailure)
            {
                return HandleError(result);
            }

            return Ok(ApiResult<CompanyPositionResponse>.Ok(result.Value));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving company position for {CompanyId}", companyId);
            return StatusCode(500, ApiResult<CompanyPositionResponse>.Fail(
                "Internal Server Error",
                "An error occurred while processing your request. Please try again later.",
                StatusCodes.Status500InternalServerError,
                HttpContext.Request.Path));
        }
    }
}
