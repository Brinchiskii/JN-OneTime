namespace OneTime.Api.Models.ProjectsDto
{
    public record ProjectMemberDto(
    int UserId,
    string Name,
    decimal HoursContributor
);
}
