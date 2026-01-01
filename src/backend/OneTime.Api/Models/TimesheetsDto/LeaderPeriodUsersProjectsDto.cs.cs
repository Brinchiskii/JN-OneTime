namespace OneTime.Api.Models.TimesheetsDto
{
	public record ProjectSimpleDto(int ProjectId, string Name, int Status);

	public record ProjectHoursByDateDto(
		ProjectSimpleDto Project,
		Dictionary<string, decimal> Hours 
	);
	public record PendingTimesheetDto(
		int TimesheetId,
		int status,
		List<ProjectHoursByDateDto> Rows
	);
	public record LeaderUsersProjectsResponseDto(
		Dictionary<string, List<PendingTimesheetDto>> Users
	);

}

