using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Services.Repository
{
    public class TimeEntryService : ITimeEntryService
    {
        private readonly ITimeEntryRepository _timeEntryRepo;
        private readonly IProjectRepository _projectRepo;

        public TimeEntryService(ITimeEntryRepository timeEntryRepo, IProjectRepository projectRepo)
        {
            _timeEntryRepo = timeEntryRepo;
            _projectRepo = projectRepo;
        }

        /// <summary>
        /// Creates a new time entry for a given user.
        /// Validates that the project exists before creating the entry.
        /// </summary>
        /// <param name="entry">The time entry that needs to be saved.</param>
        /// <returns>The saved <see cref="TimeEntry"/> with updated values.</returns>
        /// <exception cref="Exception">
        /// Will be thrown if the project with the projectId does not exist.
        /// </exception>
        public async Task<TimeEntry> CreateTimeEntry(TimeEntry entry)
        {
            var project = await _projectRepo.GetById(entry.ProjectId);
            if (project == null)
                throw new Exception("Projekt not found");

            if(entry.Hours <= 0 || entry.Hours > 24)
                throw new Exception("Hours must be greater than zero and less than 24");

            entry.Status = TimeEntryStatus.Pending;
            entry.Date = entry.Date == default ? DateOnly.FromDateTime(DateTime.Now) : entry.Date;

            return await _timeEntryRepo.Add(entry);
        }

        /// <summary>
        /// Retrieves a collection of all available projects.
        /// </summary>
        /// <returns>The collection of all available projects.</returns>
        public async Task<IEnumerable<Project>> GetAvailableProjects()
        {
            return await _projectRepo.GetAll();
        }

		public async Task<IEnumerable<TimeEntry>> GetTimeEntriesForUser(int userId)
		{
			return await _timeEntryRepo.GetByUserWithDetails(userId);
		}

	}
}	
