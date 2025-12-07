using OneTime.Core.Models.Enums;

namespace OneTime.Api.Models
{
	namespace OneTime.Api.Models
	{
		public record TimeEntryDetailsDto( int TimeEntryId, int UserId, string UserName, string email, int UserRole, int ProjectId, string ProjectName, ProjectStatus ProjectStatus, DateOnly Date, string Note, decimal Hours, TimeEntryStatus Status);
	}
}
