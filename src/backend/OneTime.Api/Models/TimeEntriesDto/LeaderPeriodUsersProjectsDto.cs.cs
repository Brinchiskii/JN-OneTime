namespace OneTime.Api.Models.TimeEntriesDto
{
    public record ProjectSimpleDto(int ProjectId, string Name, int Status);

    public record ProjectHoursByDateDto(
        ProjectSimpleDto Project,
        Dictionary<string, decimal> Hours
    );

    public record LeaderUsersProjectsResponseDto(
        Dictionary<string, List<ProjectHoursByDateDto>> Users
    );
}
