using OneTime.Core.Models.Enums;

namespace OneTime.Api.Models.TimesheetsDto
{
    /// <summary>
    /// Data summary for a timesheet.
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
