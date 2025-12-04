using OneTime.Core.Models.Enums;

namespace OneTime.Api.Models
{
    /// <summary>
    /// Data transfer object for a monthly review.
    /// </summary>
    public record TimesheetDto(
        int TimesheetId,
        int UserId,
        DateOnly PeriodStart,
        DateOnly PeriodEnd,
        TimesheetStatus Status,
        DateTime? DecidedAt,
        string Comment
    );
}
