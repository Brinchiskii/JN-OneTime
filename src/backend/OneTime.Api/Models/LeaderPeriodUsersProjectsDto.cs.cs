namespace OneTime.Api.Models
{
	public record ProjectSimpleDto(int Id, string Name, int Status);

	public record ProjectHoursByDateDto(
		ProjectSimpleDto Project,
		Dictionary<string, decimal> Hours 
	);

	public record LeaderUsersProjectsResponseDto(
		Dictionary<string, List<ProjectHoursByDateDto>> Users
	);
}

