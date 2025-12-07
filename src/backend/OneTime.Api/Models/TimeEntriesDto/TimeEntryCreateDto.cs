namespace OneTime.Api.Models.TimeEntriesDto
{
	/// <summary>
	/// Data transfer object for creating a new time entry through the API.
	/// </summary>
	public record TimeEntryCreateDto(int UserId, int ProjectId, DateOnly Date, string Note, decimal Hours);
}
