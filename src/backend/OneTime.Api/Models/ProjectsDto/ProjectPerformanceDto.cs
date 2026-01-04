namespace OneTime.Api.Models.ProjectsDto
{
    public record ProjectPerformanceDto(
    int ProjectId,
    string ProjectName,
    int Status,           
    decimal TotalHours,    
    List<ProjectMemberDto> Members 
);
}
