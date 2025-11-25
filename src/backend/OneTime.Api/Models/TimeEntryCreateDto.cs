namespace OneTime.Api.Models
{
	public record TimeEntryCreateDto(int UserId, int ProjectId, DateOnly Date, string Note, decimal Hours);
}
