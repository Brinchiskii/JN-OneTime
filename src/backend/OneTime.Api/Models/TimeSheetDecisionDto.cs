namespace OneTime.Api.Models
{
	public record TimesheetDecisionDto(
		int TimesheetId,
		int LeaderId,
		int Status,
		string Comment
	);
}
