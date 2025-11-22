using OneTime.Core.Models;

namespace OneTime.Api.Models
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
