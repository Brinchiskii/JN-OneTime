namespace OneTime.Api.Models.TimesheetsDto
{
	public record TimesheetDecisionDto(
		int TimesheetId,
		int LeaderId,
		int Status,
		string Comment
	);
}
