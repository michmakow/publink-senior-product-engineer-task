using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Publink.AuditTimeline.Api.Security;
using Publink.AuditTimeline.Application.ContractsAudit;
using Publink.AuditTimeline.Domain.Audit;

namespace Publink.AuditTimeline.Api.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
[EnableRateLimiting("AuditRead")]
public sealed class ContractsAuditController : ControllerBase
{
    private readonly GetContractAuditHandler _handler;
    private readonly SearchContractAuditHandler _searchHandler;

    public ContractsAuditController(
        GetContractAuditHandler handler,
        SearchContractAuditHandler searchHandler)
    {
        _handler = handler;
        _searchHandler = searchHandler;
    }

    [HttpGet("audit-search")]
    [ProducesResponseType(typeof(ContractAuditSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContractAuditSearchResponse>> SearchContractAudits(
        [FromQuery] string? contractId,
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? changeType,
        [FromQuery] string? entityType,
        [FromQuery] string? user,
        CancellationToken cancellationToken)
    {
        var filters = TryBuildFilters(from, to, changeType, entityType, user);
        if (!ModelState.IsValid || filters is null)
        {
            return ValidationErrorResult();
        }

        try
        {
            var response = await _searchHandler.HandleAsync(
                new SearchContractAuditQuery(contractId, filters),
                cancellationToken);

            return Ok(response);
        }
        catch (ContractAuditValidationException exception)
        {
            AddValidationErrors(exception);

            return ValidationErrorResult();
        }
    }

    [HttpGet("{contractId}/audit")]
    [ProducesResponseType(typeof(AuditTimelineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuditTimelineResponse>> GetContractAudit(
        string contractId,
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? changeType,
        [FromQuery] string? entityType,
        [FromQuery] string? user,
        CancellationToken cancellationToken)
    {
        var filters = TryBuildFilters(from, to, changeType, entityType, user);
        if (!ModelState.IsValid || filters is null)
        {
            return ValidationErrorResult();
        }

        try
        {
            var response = await _handler.HandleAsync(
                new GetContractAuditQuery(contractId, filters),
                cancellationToken);

            if (response is null)
            {
                return NotFound(new
                {
                    message = "Nie znaleziono umowy lub historii audytu dla podanego identyfikatora."
                });
            }

            return Ok(response);
        }
        catch (ContractAuditValidationException exception)
        {
            AddValidationErrors(exception);

            return ValidationErrorResult();
        }
    }

    private void AddValidationErrors(ContractAuditValidationException exception)
    {
        foreach (var error in exception.Errors)
        {
            foreach (var message in error.Value)
            {
                ModelState.AddModelError(error.Key, message);
            }
        }
    }

    private BadRequestObjectResult ValidationErrorResult()
    {
        return BadRequest(new ValidationProblemDetails(ModelState)
        {
            Title = "Żądanie zawiera nieprawidłowe dane.",
            Status = StatusCodes.Status400BadRequest
        });
    }

    private ContractAuditFilters? TryBuildFilters(
        string? from,
        string? to,
        string? changeType,
        string? entityType,
        string? user)
    {
        var fromDate = TryParseDate(from, nameof(from), inclusiveEndOfDay: false);
        var toDate = TryParseDate(to, nameof(to), inclusiveEndOfDay: true);
        var parsedChangeType = TryParseEnum<AuditChangeType>(changeType, nameof(changeType));
        var parsedEntityType = TryParseEnum<AuditEntityType>(entityType, nameof(entityType));

        if (!ModelState.IsValid)
        {
            return null;
        }

        return new ContractAuditFilters(fromDate, toDate, parsedChangeType, parsedEntityType, user);
    }

    private DateTimeOffset? TryParseDate(
        string? value,
        string fieldName,
        bool inclusiveEndOfDay)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateOnly.TryParseExact(
            value,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dateOnly))
        {
            var time = inclusiveEndOfDay ? TimeOnly.MaxValue : TimeOnly.MinValue;
            return new DateTimeOffset(dateOnly.ToDateTime(time), TimeSpan.Zero);
        }

        if (DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out var dateTime))
        {
            return dateTime;
        }

        ModelState.AddModelError(fieldName, "Data musi mieć format yyyy-MM-dd albo poprawny format ISO.");
        return null;
    }

    private TEnum? TryParseEnum<TEnum>(string? value, string fieldName)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        ModelState.AddModelError(fieldName, $"Nieobsługiwana wartość pola {fieldName}.");
        return null;
    }
}
