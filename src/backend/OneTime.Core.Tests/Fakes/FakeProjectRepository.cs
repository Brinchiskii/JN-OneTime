using OneTime.Core.Models;
using OneTime.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Tests.Fakes
{
    /// <summary>
    /// Provides an in-memory implementation of the IProjectRepository interface for testing.
    /// </summary>
    public class FakeProjectRepository : IProjectRepository
    {
        public List<Project> Projects { get; } = new List<Project>();

        public Task<IEnumerable<Project>> GetAll()
        {
            return Task.FromResult<IEnumerable<Project>>(Projects);
        }

        public Task<Project> GetById(int id)
        {
            var project = Projects.FirstOrDefault(p => p.ProjectId == id);
            return Task.FromResult(project);
        }
    }
}
