using OneTime.Core.Models;

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
			return new TimeEntryDto(
				entity.TimeEntryId,
				entity.UserId,
				entity.ProjectId,
				entity.Date,
				entity.Note,
				entity.Hours,
				entity.Status
			);
		}
	}
}
