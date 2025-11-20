using OneTime.Core.Models.Enums;

namespace OneTime.Api.Models
{
	public record TimeEntryDto(int TimeEntryId, int UserId, int ProjectId, DateOnly Date, string Note, decimal Hours, TimeEntryStatus Status);
}
