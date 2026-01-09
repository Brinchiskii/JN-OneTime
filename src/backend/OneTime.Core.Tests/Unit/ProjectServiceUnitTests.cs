using Moq;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Implementations;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Core.Tests.Unit;

public class ProjectServiceUnitTests
{
    [Fact]
    public async Task GetAll_ReturnsProjects()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();

        projectRepoMock
            .Setup(r => r.GetAll())
            .ReturnsAsync(new List<Project>
            {
                new() { ProjectId = 1, Name = "P1", Status = 0 },
                new() { ProjectId = 2, Name = "P2", Status = 1 }
            });

        var service = new ProjectService(projectRepoMock.Object);

        // Act
        var result = await service.GetAll();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count());

        projectRepoMock.Verify(r => r.GetAll(), Times.Once);
    }
    
    [Fact]
    public async Task GetAll_ReturnsEmpty()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();

        projectRepoMock
            .Setup(r => r.GetAll())
            .ReturnsAsync(new List<Project>());

        var service = new ProjectService(projectRepoMock.Object);

        // Act
        var result = await service.GetAll();

        // Assert
        Assert.Empty(result);

        projectRepoMock.Verify(r => r.GetAll(), Times.Once);
    }
    
    [Fact]
    public async Task GetById_ValidId_ReturnsProject()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();

        var project = new Project
        {
            ProjectId = 10,
            Name = "Test Project",
            Status = (int)ProjectStatus.Active
        };

        projectRepoMock
            .Setup(r => r.GetById(10))
            .ReturnsAsync(project);

        var service = new ProjectService(projectRepoMock.Object);

        // Act
        var result = await service.GetById(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.ProjectId);
        Assert.Equal("Test Project", result.Name);
        Assert.Equal((int)ProjectStatus.Active, result.Status);

        projectRepoMock.Verify(r => r.GetById(10), Times.Once);
    }
    
    [Fact]
    public async Task GetById_IdIsZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();
        var service = new ProjectService(projectRepoMock.Object);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.GetById(0));

        Assert.Equal("Project ID must be greater than zero.", ex.ParamName);

        projectRepoMock.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }
    
    [Fact]
    public async Task GetById_IdIsNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();
        var service = new ProjectService(projectRepoMock.Object);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.GetById(-1));

        Assert.Equal("Project ID must be greater than zero.", ex.ParamName);
        
        projectRepoMock.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetById_NotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();

        projectRepoMock
            .Setup(r => r.GetById(10))!
            .ReturnsAsync((Project?)null);

        var service = new ProjectService(projectRepoMock.Object);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetById(10));

        Assert.Equal("Project not found.", ex.Message);

        projectRepoMock.Verify(r => r.GetById(10), Times.Once);
    }

    [Fact]
    public async Task Create_ValidInput_CallsRepositoryAndReturnsProject()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();

        projectRepoMock
            .Setup(r => r.Add(It.IsAny<Project>()))
            .ReturnsAsync((Project p) =>
            {
                p.ProjectId = 1;
                return p;
            });

        var service = new ProjectService(projectRepoMock.Object);

        // Act
        var result = await service.Create("New Project", ProjectStatus.Active);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ProjectId);
        Assert.Equal("New Project", result.Name);
        Assert.Equal((int)ProjectStatus.Active, result.Status);

        projectRepoMock.Verify(r => r.Add(It.Is<Project>(p =>
            p.Name == "New Project" &&
            p.Status == (int)ProjectStatus.Active
        )), Times.Once);
    }

    [Fact]
    public async Task Create_InvalidName_ThrowsInvalidOperationException()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();
        var service = new ProjectService(projectRepoMock.Object);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Create(null!, ProjectStatus.Active));

        Assert.Equal("Project name is required.", ex.Message);
        
        projectRepoMock.Verify(r => r.Add(It.IsAny<Project>()), Times.Never);
    }
    
    [Fact]
    public async Task Create_InvalidStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();
        var service = new ProjectService(projectRepoMock.Object);

        var invalidStatus = (ProjectStatus)999;

        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Create("Test Project", invalidStatus));

        Assert.Equal("Invalid project status.", ex.Message);

        projectRepoMock.Verify(r => r.Add(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async Task Update_ValidInput_UpdatesAndReturnsProject()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();

        var existingProject = new Project
        {
            ProjectId = 10,
            Name = "Old Name",
            Status = (int)ProjectStatus.Paused
        };

        projectRepoMock
            .Setup(r => r.GetById(10))
            .ReturnsAsync(existingProject);

        projectRepoMock
            .Setup(r => r.Update(existingProject))
            .ReturnsAsync(existingProject);

        var service = new ProjectService(projectRepoMock.Object);

        // Act
        var result = await service.Update(10, "Updated Name", ProjectStatus.Active);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.ProjectId);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal((int)ProjectStatus.Active, result.Status);

        projectRepoMock.Verify(r => r.GetById(10), Times.Once);
        projectRepoMock.Verify(r => r.Update(existingProject), Times.Once);
    }
    
    [Fact]
    public async Task Update_ProjectNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();

        projectRepoMock
            .Setup(r => r.GetById(10))!
            .ReturnsAsync((Project?)null);

        var service = new ProjectService(projectRepoMock.Object);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Update(10, "New Name", ProjectStatus.Active));

        Assert.Equal("Project not found.", ex.Message);

        projectRepoMock.Verify(r => r.GetById(10), Times.Once);
        projectRepoMock.Verify(r => r.Update(It.IsAny<Project>()), Times.Never);
    }
    
    [Fact]
    public async Task Update_InvalidName_ThrowsInvalidOperationException()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();

        var existingProject = new Project
        {
            ProjectId = 10,
            Name = "Old Name",
            Status = (int)ProjectStatus.Active
        };

        projectRepoMock
            .Setup(r => r.GetById(10))
            .ReturnsAsync(existingProject);

        var service = new ProjectService(projectRepoMock.Object);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Update(10, null!, ProjectStatus.Active));

        Assert.Equal("Project name is required.", ex.Message);

        projectRepoMock.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
        projectRepoMock.Verify(r => r.Update(It.IsAny<Project>()), Times.Never);
    }
    
    [Fact]
    public async Task Update_InvalidStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();
        var service = new ProjectService(projectRepoMock.Object);

        var invalidStatus = (ProjectStatus)999;

        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Update(10, "New Name", invalidStatus));

        Assert.Equal("Invalid project status.", ex.Message);
        
        projectRepoMock.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
        projectRepoMock.Verify(r => r.Update(It.IsAny<Project>()), Times.Never);
    }
    
    [Fact]
    public async Task Delete_ValidId_DeletesProjectAndReturnsIt()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();

        var project = new Project
        {
            ProjectId = 10,
            Name = "Test Project",
            Status = (int)ProjectStatus.Active
        };

        projectRepoMock
            .Setup(r => r.GetById(10))
            .ReturnsAsync(project);

        projectRepoMock
            .Setup(r => r.Delete(project))
            .ReturnsAsync(project);

        var service = new ProjectService(projectRepoMock.Object);

        // Act
        var result = await service.Delete(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.ProjectId);

        projectRepoMock.Verify(r => r.GetById(10), Times.Once);
        projectRepoMock.Verify(r => r.Delete(project), Times.Once);
    }
    
    [Fact]
    public async Task Delete_ProjectNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();

        projectRepoMock
            .Setup(r => r.GetById(10))!
            .ReturnsAsync((Project?)null);

        var service = new ProjectService(projectRepoMock.Object);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Delete(10));

        Assert.Equal("Project not found.", ex.Message);

        projectRepoMock.Verify(r => r.GetById(10), Times.Once);
        projectRepoMock.Verify(r => r.Delete(It.IsAny<Project>()), Times.Never);
    }
}