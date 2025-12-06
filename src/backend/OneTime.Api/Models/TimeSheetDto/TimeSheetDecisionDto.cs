namespace OneTime.Api.Models.TimeSheetDto
{
	public record TimesheetDecisionDto(
		int TimesheetId,
		int LeaderId,
		int Status,
		string Comment
	);
}
