using OneTime.Api.Models.OneTime.Api.Models;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;

namespace OneTime.Api.Models.TimeEntriesDto
{
	public static class TimeEntryConverter
	{
		/// <summary>
		/// Converts a <see cref="TimeEntryCreateDto"/> instance to a new <see cref="TimeEntry"/> entity.
		/// </summary>
		/// <param name="dto">The data transfer object containing the information to populate the <see cref="TimeEntry"/> entity.</param>
		/// <returns>A new <see cref="TimeEntry"/> entity.</returns>
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

        /// <summary>
        /// Converts a <see cref="TimeEntry"/> entity to a <see cref="TimeEntryDto"/>."/>
        /// </summary>
        /// <param name="entity">The time entry object containg the values needed to create the <see cref="TimeEntryDto"/>.</param>
        /// <returns>The <see cref="TimeEntryDto"/>.</returns>
        public static TimeEntryDto ToDto(TimeEntry entity)
		{
			return new TimeEntryDto(entity.TimeEntryId, entity.UserId, entity.ProjectId, entity.Date, entity.Note, entity.Hours, entity.TimesheetId);
		}

		public static TimeEntryDetailsDto ToDetailsDto(TimeEntry entity)
		{
			return new TimeEntryDetailsDto(
				entity.TimeEntryId,
				entity.UserId,
				entity.User?.Name ?? string.Empty,
				entity.User?.Email ?? string.Empty,
				(int)entity.User?.Role,
				entity.ProjectId,
				entity.Project?.Name ?? string.Empty,
				entity.Project != null ? (ProjectStatus)entity.Project.Status : ProjectStatus.Active,
				entity.Date,
				entity.Note,
				entity.Hours,
				entity.TimesheetId
				);
			//(TimeEntryStatus)entity.Status);
		}
	}
}