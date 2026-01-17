using ForecastService.Application.DTOs;
using ForecastService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ForecastService.Api.Controllers;

[Route("api/[controller]")]
public class CompanyPositionController(ICompanyPositionService companyPositionService) : BaseApiController
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
        [Required(ErrorMessage = "Company ID is required")] Guid? companyId,
        [FromQuery, Required(ErrorMessage = "Start date is required")] DateTime? startDate,
        [FromQuery, Required(ErrorMessage = "End date is required")] DateTime? endDate)
    {
        var result = await companyPositionService.GetCompanyPositionAsync(companyId!.Value, startDate!.Value, endDate!.Value);

        if (result.IsFailure)
        {
            return HandleError(result);
        }

        return Ok(ApiResult<CompanyPositionResponse>.Ok(result.Value));
    }
}
