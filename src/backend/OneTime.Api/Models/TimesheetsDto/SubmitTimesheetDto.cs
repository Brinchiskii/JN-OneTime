namespace OneTime.Api.Models.TimesheetsDto
{
    /// <summary>
	/// Data transfer object for submitting a new monthly review through the API.
	/// </summary>
    public record SubmitTimesheetDto(
        int UserId,
        DateOnly PeriodStart,
        DateOnly PeriodEnd
    );
}
