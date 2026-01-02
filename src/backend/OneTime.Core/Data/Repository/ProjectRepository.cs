using Microsoft.EntityFrameworkCore;
using OneTime.Core.Models;
using OneTime.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Services.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly OneTimeContext _context;

        /// <summary>
        /// Initializes a new instance of the ProjectRepository class using the specified database context.
        /// </summary>
        /// <param name="context">The database context.</param>
        public ProjectRepository(OneTimeContext context)
        {
            _context = context;
        }

		public async Task<Project> Add(Project project)
		{
			if (project == null)
				throw new ArgumentNullException(nameof(project));
			
			_context.Projects.Add(project);
			await _context.SaveChangesAsync();

			return project;
		}

		public async Task<Project> Delete(Project project)
		{
			var hasEntries = await _context.TimeEntries.AnyAsync(t => t.ProjectId == project.ProjectId);
			if (hasEntries)
				throw new InvalidOperationException("Cannot delete project with time entries.");

			_context.Projects.Remove(project);
			await _context.SaveChangesAsync();
			return project;
		}

		public async Task<Project> Update(Project project)
		{
			_context.Projects.Update(project);
			await _context.SaveChangesAsync();
			return project;
		}

		/// <summary>
		/// Retrieves a project by its unique identifier.
		/// </summary>
		/// <param name="id">The unique identifier for retrievel of the project.</param>
		/// <returns>The project, which matches the unique identifier.</returns>
		public async Task<Project> GetById(int id)
        {
            return await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
        }

        /// <summary>
        /// Retrives all projects from the database.
        /// </summary>
        /// <returns>A collection of all the projects.</returns>
        public async Task<IEnumerable<Project>> GetAll()
        {
            return await _context.Projects.ToListAsync();
        }
    }
}
