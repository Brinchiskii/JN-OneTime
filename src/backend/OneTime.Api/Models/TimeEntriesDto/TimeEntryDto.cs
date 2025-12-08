using OneTime.Core.Models.Enums;

namespace OneTime.Api.Models.TimeEntriesDto
{
    /// <summary>
    /// Data transfer object for a time entry.
    /// </summary>
    public record TimeEntryDto(int TimeEntryId, int UserId, int ProjectId, DateOnly Date, string Note, decimal Hours, int TimesheetId);
}
