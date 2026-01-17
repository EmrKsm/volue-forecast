using ForecastService.Application.DTOs;
using ForecastService.Application.Interfaces;
using ForecastService.Domain.Common;
using ForecastService.Domain.Entities;
using ForecastService.Domain.Errors;
using ForecastService.Domain.Events;
using ForecastService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForecastService.Application.Services;

public class ForecastService(
    IForecastRepository forecastRepository,
    IPowerPlantRepository powerPlantRepository,
    IEventPublisher eventPublisher) : IForecastService
{

    public async Task<Result<ForecastResponse>> CreateOrUpdateForecastAsync(CreateOrUpdateForecastRequest request)
    {
        // Validate power plant exists
        var powerPlant = await powerPlantRepository.GetByIdAsync(request.PowerPlantId);
        if (powerPlant == null)
        {
            return Result.Failure<ForecastResponse>(PowerPlantErrors.NotFound(request.PowerPlantId));
        }

        // Validate production value
        if (request.ProductionMWh < 0)
        {
            return Result.Failure<ForecastResponse>(ForecastErrors.NegativeProduction);
        }

        // Check if forecast already exists for this power plant and datetime
        var existingForecast = await forecastRepository.GetByPowerPlantAndDateTimeAsync(
            request.PowerPlantId,
            request.ForecastDateTime);

        Forecast forecast;
        bool isUpdate = false;

        try
        {
            if (existingForecast != null)
            {
                // Update existing forecast
                existingForecast.ProductionMWh = request.ProductionMWh;
                existingForecast.UpdatedAt = DateTime.UtcNow;
                forecast = await forecastRepository.UpdateAsync(existingForecast);
                isUpdate = true;
            }
            else
            {
                // Create new forecast
                forecast = new Forecast
                {
                    Id = Guid.NewGuid(),
                    PowerPlantId = request.PowerPlantId,
                    ForecastDateTime = request.ForecastDateTime,
                    ProductionMWh = request.ProductionMWh,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                forecast = await forecastRepository.CreateAsync(forecast);
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure<ForecastResponse>(ForecastErrors.ConcurrencyConflict);
        }
        catch (DbUpdateException ex)
        {
            // Handle specific constraint violations
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true ||
                ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Result.Failure<ForecastResponse>(DatabaseErrors.UniqueConstraintViolation);
            }
            
            if (ex.InnerException?.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase) == true ||
                ex.InnerException?.Message.Contains("violates foreign key", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Result.Failure<ForecastResponse>(DatabaseErrors.ForeignKeyViolation);
            }
            
            return Result.Failure<ForecastResponse>(ForecastErrors.DatabaseError);
        }
        catch (TimeoutException)
        {
            return Result.Failure<ForecastResponse>(DatabaseErrors.TimeoutError);
        }

        // Emit position changed event
        await EmitPositionChangedEventAsync(powerPlant.CompanyId, forecast.ForecastDateTime, isUpdate);

        return Result.Success(MapToResponse(forecast, powerPlant));
    }

    public async Task<Result<ForecastResponse>> GetForecastAsync(Guid id)
    {
        var forecast = await forecastRepository.GetByIdAsync(id);
        if (forecast == null)
        {
            return Result.Failure<ForecastResponse>(ForecastErrors.NotFound(id));
        }

        var powerPlant = await powerPlantRepository.GetByIdAsync(forecast.PowerPlantId);
        return Result.Success(MapToResponse(forecast, powerPlant!));
    }

    public async Task<Result<IEnumerable<ForecastResponse>>> GetForecastsByPowerPlantAsync(
        Guid powerPlantId,
        DateTime startDate,
        DateTime endDate)
    {
        if (startDate > endDate)
        {
            return Result.Failure<IEnumerable<ForecastResponse>>(ForecastErrors.InvalidDateRange);
        }

        var powerPlant = await powerPlantRepository.GetByIdAsync(powerPlantId);
        if (powerPlant == null)
        {
            return Result.Failure<IEnumerable<ForecastResponse>>(PowerPlantErrors.NotFound(powerPlantId));
        }

        var forecasts = await forecastRepository.GetActiveByPowerPlantAsync(powerPlantId, startDate, endDate);
        var responses = forecasts.Select(f => MapToResponse(f, powerPlant)).ToList();

        return Result.Success<IEnumerable<ForecastResponse>>(responses);
    }

    private async Task EmitPositionChangedEventAsync(Guid companyId, DateTime forecastDateTime, bool isUpdate)
    {
        // Calculate position for the day of the forecast
        var startDate = forecastDateTime.Date;
        var endDate = startDate.AddDays(1);

        var forecasts = await forecastRepository.GetAllActiveForCompanyAsync(companyId, startDate, endDate);
        var totalPosition = forecasts.Sum(f => f.ProductionMWh);

        var positionEvent = new PositionChangedEvent
        {
            CompanyId = companyId,
            StartDate = startDate,
            EndDate = endDate,
            TotalPositionMWh = totalPosition,
            EventTimestamp = DateTime.UtcNow,
            Reason = isUpdate ? "Forecast Updated" : "Forecast Created"
        };

        await eventPublisher.PublishPositionChangedEventAsync(positionEvent);
    }

    private static ForecastResponse MapToResponse(Forecast forecast, PowerPlant powerPlant)
    {
        return new ForecastResponse
        {
            Id = forecast.Id,
            PowerPlantId = forecast.PowerPlantId,
            PowerPlantName = powerPlant.Name,
            Country = powerPlant.Country,
            ForecastDateTime = forecast.ForecastDateTime,
            ProductionMWh = forecast.ProductionMWh,
            CreatedAt = forecast.CreatedAt,
            UpdatedAt = forecast.UpdatedAt
        };
    }
}
