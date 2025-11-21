using OneTime.Api.Models.OneTime.Api.Models;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;

namespace OneTime.Api.Models
{
	public static class TimeEntryConverter
	{
		public static TimeEntry ToEntity(TimeEntryCreateDto dto)
		{
			return new TimeEntry
			{
				UserId = dto.UserId,
				ProjectId = dto.ProjectId,
				Date = dto.Date,
				Note = dto.Note,
				Hours = dto.Hours,
			};
		}
		public static TimeEntryDto ToDto(TimeEntry entity)
		{
			return new TimeEntryDto(entity.TimeEntryId, entity.UserId, entity.ProjectId, entity.Date, entity.Note, entity.Hours, entity.Status);
		}

		public static TimeEntryDetailsDto ToDetailsDto(TimeEntry entity)
		{
			return new TimeEntryDetailsDto(
				entity.TimeEntryId,
				entity.UserId,
				entity.User?.Name ?? string.Empty,
				entity.User?.Email ?? string.Empty,
				entity.ProjectId,
				entity.Project?.Name ?? string.Empty,
				entity.Project?.Status ?? ProjectStatus.Active,
				entity.Date,
				entity.Note,
				entity.Hours,
				entity.Status);
		}
	}
}
