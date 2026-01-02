using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Core.Services.Implementations;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<IEnumerable<Project>> GetAll()
    {
        var projects = await _projectRepository.GetAll();
        return projects;
    }

    public Task<Project?> GetById(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Project ID must be greater than zero.");
        }
        
        return _projectRepository.GetById(id);
    }

    public async Task<Project> Create(string name, ProjectStatus status)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Project name is required.");

        if (!Enum.IsDefined(typeof(ProjectStatus), status))
            throw new InvalidOperationException("Invalid project status.");

        var entity = new Project
        {
            Name = name,
            Status = (int)status
        };

        return await _projectRepository.Add(entity);
    }

    public async Task<Project> Update(int id, string name, ProjectStatus status)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Project name is required.");

        if (!Enum.IsDefined(typeof(ProjectStatus), status))
            throw new InvalidOperationException("Invalid project status.");

        var existing = await _projectRepository.GetById(id);
        if (existing == null)
            throw new InvalidOperationException("Project not found.");

        existing.Name = name;
        existing.Status = (int)status;
        return await _projectRepository.Update(existing);
    }

    public async Task<Project> Delete(int id)
    {
        var project = await _projectRepository.GetById(id);
        if (project == null)
            throw new InvalidOperationException("Project not found.");
        
        return await _projectRepository.Delete(project);
    }
}
