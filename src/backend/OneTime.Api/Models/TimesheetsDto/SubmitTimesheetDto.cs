namespace OneTime.Api.Models.TimesheetsDto
{
    /// <summary>
	/// Data transfer object for submitting a new timesheet through the API.
	/// </summary>
    public record SubmitTimesheetDto(
        int UserId,
        DateOnly PeriodStart,
        DateOnly PeriodEnd
    );
}
