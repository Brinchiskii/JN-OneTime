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

        public ProjectRepository(OneTimeContext context)
        {
            _context = context;
        }

        public async Task<Project> GetById(int id)
        {
            return await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
        }

        public async Task<IEnumerable<Project>> GetAll()
        {
            return await _context.Projects.ToListAsync();
        }
    }
}
