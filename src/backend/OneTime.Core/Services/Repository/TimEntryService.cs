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

        public async Task<TimeEntry> CreateTimeEntry(TimeEntry entry)
        {
            var project = await _projectRepo.GetById(entry.ProjectId);
            if (project == null)
                throw new Exception("Projekt not found");

            entry.Status = TimeEntryStatus.Pending;
            entry.Date = entry.Date == default ? DateOnly.FromDateTime(DateTime.Now) : entry.Date;

            return await _timeEntryRepo.Add(entry);
        }

        public async Task<IEnumerable<Project>> GetAvailableProjects()
        {
            return await _projectRepo.GetAll();
        }
    }
}	
