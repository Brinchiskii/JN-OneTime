using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OneTime.Api.Models.ProjectsDto;
using OneTime.Api.Tests.TestHelpers;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using Xunit;

namespace OneTime.Api.Tests.Endpoints;

public class ProjectsControllerTests
{
    private static HttpClient CreateClient()
    {
        var factory = new OneTimeApiFactory();
        return factory.CreateClient();
    }
    
    [Fact]
    public async Task GetAll_WhenNoProjects_Returns204()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/projects");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAll_WhenProjectsExist_Returns200()
    {
        var client = CreateClient();
        
        // Arrange
        var createDto = new ProjectCreateDto(
            Name: "Test Project",
            Status: (int)ProjectStatus.Active
        );

        await client.PostAsJsonAsync("/api/projects", createDto);

        // Act
        var response = await client.GetAsync("/api/projects");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var projects = await response.Content.ReadFromJsonAsync<List<Project>>();
        Assert.NotNull(projects);
        Assert.NotEmpty(projects);
    }

    [Fact]
    public async Task GetById_ProjectExists_Returns200()
    {
        // Arrange
        var client = CreateClient();
        
        
        // Act
        var createResponse = await client.PostAsJsonAsync("/api/projects",
            new ProjectCreateDto("Project A", (int)ProjectStatus.Active));

        var created = await createResponse.Content.ReadFromJsonAsync<Project>();

        var response = await client.GetAsync($"/api/projects/{created!.ProjectId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var project = await response.Content.ReadFromJsonAsync<Project>();
        Assert.Equal("Project A", project!.Name);
    }
    
    [Fact]
    public async Task GetById_ProjectNotFound_Returns404()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/projects/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task Create_ValidInput_Returns201()
    {
        // Arrange
        var client = CreateClient();
        
        var dto = new ProjectCreateDto(
            Name: "New Project",
            Status: (int)ProjectStatus.Active);

        // Act
        var response = await client.PostAsJsonAsync("/api/projects", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var project = await response.Content.ReadFromJsonAsync<Project>();
        Assert.Equal("New Project", project!.Name);
    }
    
    [Fact]
    public async Task Create_InvalidName_Returns400()
    {
        // Arrange
        var client = CreateClient();
        
        var dto = new ProjectCreateDto(
            Name: "",
            Status: (int)ProjectStatus.Active);

        // Act
        var response = await client.PostAsJsonAsync("/api/projects", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task Create_InvalidStatus_Returns400()
    {
        // Arrange
        var client = CreateClient();
        
        var dto = new ProjectCreateDto(
            Name: "Bad Project",
            Status: 999);

        // Act
        var response = await client.PostAsJsonAsync("/api/projects", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task Update_ProjectExists_Returns200()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var createResponse = await client.PostAsJsonAsync("/api/projects",
            new ProjectCreateDto("Old Name", (int)ProjectStatus.Active));

        var project = await createResponse.Content.ReadFromJsonAsync<Project>();

        var updateDto = new ProjectUpdateDto(
            Name: "Updated Name",
            Status: (int)ProjectStatus.Active);

        var response = await client.PutAsJsonAsync(
            $"/api/projects/{project!.ProjectId}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<Project>();
        Assert.Equal("Updated Name", updated!.Name);
    }
    
    [Fact]
    public async Task Update_ProjectNotFound_Returns404()
    {
        // Arrange
        var client = CreateClient();
        
        var dto = new ProjectUpdateDto("Test", (int)ProjectStatus.Active);

        // Act
        var response = await client.PutAsJsonAsync("/api/projects/999", dto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task Delete_ProjectExists_Returns200()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var createResponse = await client.PostAsJsonAsync("/api/projects",
            new ProjectCreateDto("To Be Deleted", (int)ProjectStatus.Active));

        var project = await createResponse.Content.ReadFromJsonAsync<Project>();

        var response = await client.DeleteAsync($"/api/projects/{project!.ProjectId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ProjectNotFound_Returns404()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var response = await client.DeleteAsync("/api/projects/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}