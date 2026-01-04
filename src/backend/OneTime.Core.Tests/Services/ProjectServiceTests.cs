using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Implementations;
using OneTime.Core.Services.Repository;
using OneTime.Core.Tests.TestHelpers;

namespace OneTime.Core.Tests.Services;

public class ProjectServiceTests
{
    public ProjectServiceTests()
    {
        
    }
    
    [Fact]
    public async Task GetAllProjects_Returns_ProjectsOk()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();

        var projectrepo = new ProjectRepository(context);
        var projectService = new ProjectService(projectrepo);

        context.Projects.Add(new Project
        {
            ProjectId = 10,
            Name = "Test Project",
            Status = 0
        });
        
        context.Projects.Add(new Project
        {
            ProjectId = 20,
            Name = "Test Project 2",
            Status = 1
        });
        
        await context.SaveChangesAsync();

        var projects = await projectService.GetAll();
        
        Assert.NotEmpty(projects);
    }

    [Fact]
    public async Task GetAllProjects_Returns_ProjectsEmpty()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var projectrepo = new ProjectRepository(context);
        var projectService = new ProjectService(projectrepo);
        
        var projects = await projectService.GetAll();
        
        Assert.Empty(projects);
    }

    [Fact]
    public async Task GetUserById_Returns_UserOk()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var projectRepo = new ProjectRepository(context);
        var projectService = new ProjectService(projectRepo);

        context.Projects.Add(new Project
        {
            ProjectId = 10,
            Name = "Test Project",
            Status = 0
        });
        
        await context.SaveChangesAsync();
        
        var project = await projectService.GetById(10);
        
        Assert.NotNull(project);
        Assert.Equal(10, project.ProjectId);
        Assert.Equal("Test Project", project.Name);
        Assert.Equal(0, project.Status);
    }

    [Fact]
    public async Task GetUserById_Returns_UserNotFound()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var projectRepo = new ProjectRepository(context);
        var projectService = new ProjectService(projectRepo);
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => projectService.GetById(2));
        Assert.Equal("Project not found.", ex.Message);
    }
    
    [Fact]
    public async Task GetUserById_Id_is_Zero_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var projectRepo = new ProjectRepository(context);
        var projectService = new ProjectService(projectRepo);
        
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => projectService.GetById(0));
        Assert.Equal("Project ID must be greater than zero.", ex.ParamName);
    }
    
    [Fact]
    public async Task GetUserById_Id_is_Negative_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var projectRepo = new ProjectRepository(context);
        var projectService = new ProjectService(projectRepo);
        
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => projectService.GetById(-1));
        Assert.Equal("Project ID must be greater than zero.", ex.ParamName);
    }
    
    [Fact]
    public async Task CreateProject_Succeeds()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var projectRepo = new ProjectRepository(context);
        var projectService = new ProjectService(projectRepo);
        
        string name = "Test Project";
        ProjectStatus status = ProjectStatus.Active;
        
        var projectCreated = await projectService.Create(name, status);
        
        Assert.NotNull(projectCreated);
        Assert.Equal(name, projectCreated.Name);
        Assert.Equal((int)status, projectCreated.Status);
    }

    [Fact]
    public async Task Create_Invalid_Name_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var projectRepo = new ProjectRepository(context);
        var projectService = new ProjectService(projectRepo);
        
        string name = null;
        ProjectStatus status = ProjectStatus.Active;
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => projectService.Create(name, status));
        Assert.Equal("Project name is required.", ex.Message);
    }

    [Fact]
    public async Task Create_Invalid_Status_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var projectRepo = new ProjectRepository(context);
        var projectService = new ProjectService(projectRepo);
        
        string name = "Test Project";
        int status = 6;
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => projectService.Create(name, (ProjectStatus)status));
        Assert.Equal("Invalid project status.", ex.Message);
    }

    [Fact]
    public async Task UpdateProject_Succeeds()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var projectRepo = new ProjectRepository(context);
        var projectService = new ProjectService(projectRepo);

        var project = new Project
        {
            ProjectId = 10,
            Name = "Test Project",
            Status = 0
        };

        context.Projects.Add(project);
        
        await context.SaveChangesAsync();
        
        var updatedProject = await projectService.Update(10, "Test Project 2", ProjectStatus.Active);
        
        Assert.NotNull(updatedProject);
        Assert.Equal(10, updatedProject.ProjectId);
        Assert.Equal("Test Project 2", updatedProject.Name);
        Assert.Equal((int)ProjectStatus.Active, updatedProject.Status);
    }

    [Fact]
    public async Task UpdateProject_NotFound_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var projectRepo = new ProjectRepository(context);
        var projectService = new ProjectService(projectRepo);
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => projectService.Update(10, "Test Project 2", ProjectStatus.Active));
        Assert.Equal("Project not found.", ex.Message);
    }

    [Fact]
    public async Task UpdateProject_Invalid_Name_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var projectRepo = new ProjectRepository(context);
        var projectService = new ProjectService(projectRepo);

        context.Projects.Add(new Project
        {
            ProjectId = 10,
            Name = "Test Project",
            Status = 0
        });
        
        await context.SaveChangesAsync();
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => projectService.Update(10, null, ProjectStatus.Active));
        Assert.Equal("Project name is required.", ex.Message);
    }

    [Fact]
    public async Task UpdateProject_Invalid_Status_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var projectRepo = new ProjectRepository(context);
        var projectService = new ProjectService(projectRepo);

        context.Projects.Add(new Project
        {
            ProjectId = 10,
            Name = "Test Project",
            Status = 0
        });
        
        await context.SaveChangesAsync();
        
        var ex =  await Assert.ThrowsAsync<InvalidOperationException>(() => projectService.Update(10, "Test Project 2", (ProjectStatus)6));
        Assert.Equal("Invalid project status.", ex.Message);
    }

    [Fact]
    public async Task DeleteProject_Succeeds()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var projectRepo = new ProjectRepository(context);
        var projectService = new ProjectService(projectRepo);

        var project = new Project
        {
            ProjectId = 10,
            Name = "Test Project",
            Status = 0
        };
        
        context.Projects.Add(project);
        
        await context.SaveChangesAsync();
        
        await projectService.Delete(10);
        
        Assert.Empty(context.Projects);
    }

    [Fact]
    public async Task DeleteProject_NotFound_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var projectRepo = new ProjectRepository(context);
        var projectService = new ProjectService(projectRepo);
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => projectService.Delete(10));
        Assert.Equal("Project not found.", ex.Message);
    }
}