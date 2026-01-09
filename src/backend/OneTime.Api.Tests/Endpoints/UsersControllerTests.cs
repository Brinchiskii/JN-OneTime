using System.Net;
using System.Net.Http.Json;
using OneTime.Api.Models.UsersDto;
using OneTime.Api.Tests.TestHelpers;
using OneTime.Core.Models.Enums;

namespace OneTime.Api.Tests.Endpoints;

public class UsersControllerTests : IClassFixture<OneTimeApiFactory>
{
    private HttpClient CreateClient()
    {
        var factory = new OneTimeApiFactory();
        return factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_WhenNoUsers_Returns204()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAll_WhenUsersExist_Returns200()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        await client.PostAsJsonAsync("/api/users",
            new UserCreateDto("A", "a@test.com", "p", 2, 1));

        var response = await client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/users/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingUser_Returns200()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var create = await client.PostAsJsonAsync("/api/users",
            new UserCreateDto("A", "a@test.com", "p", 2, 1));

        var user = await create.Content.ReadFromJsonAsync<UserDto>();

        var response = await client.GetAsync($"/api/users/{user!.UserId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateUser_ValidInput_Returns201()
    {
        // Arrange
        var client = CreateClient();
        
        var dto = new UserCreateDto(
            "Test User",
            "test@test.com",
            "password",
            Role: 2, // Employee
            ManagerId: 1);

        // Act
        var response = await client.PostAsJsonAsync("/api/users", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        
        Assert.NotNull(user);
        Assert.Equal("test@test.com", user!.Email);
    }

    [Fact]
    public async Task CreateUser_InvalidRole_Returns400()
    {
        // Arrange
        var client = CreateClient();
        
        var dto = new UserCreateDto("A", "a@test.com", "p", 99, 1);

        // Act
        var response = await client.PostAsJsonAsync("/api/users", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateUser_EmailAlreadyExists_Returns400()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var dto = new UserCreateDto("A", "a@test.com", "p", 2, 1);

        await client.PostAsJsonAsync("/api/users", dto);
        var response = await client.PostAsJsonAsync("/api/users", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_NotFound_Returns404()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var dto = new UserUpdateDto("A", "a@test.com", "p", 2, 1);

        var response = await client.PutAsJsonAsync("/api/users/999", dto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateUser_Valid_Returns200()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var create = await client.PostAsJsonAsync("/api/users",
            new UserCreateDto("A", "a@test.com", "p", 2, 1));

        var user = await create.Content.ReadFromJsonAsync<UserDto>();

        var update = new UserUpdateDto("Updated", "a@test.com", "p", 2, 1);

        var response = await client.PutAsJsonAsync($"/api/users/{user!.UserId}", update);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteUser_NotFound_Returns404()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var response = await client.DeleteAsync("/api/users/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_Valid_Returns200()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var create = await client.PostAsJsonAsync("/api/users",
            new UserCreateDto("A", "a@test.com", "p", 2, 1));
        
        var user = await create.Content.ReadFromJsonAsync<UserDto>();
        
        var response = await client.DeleteAsync($"/api/users/{user!.UserId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetUsersByLeader_NoUsers_Returns204()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/users/leader/1");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersByLeader_Returns200()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        await client.PostAsJsonAsync("/api/users",
            new UserCreateDto("Emp", "e@test.com", "p", 2, 1));

        var response = await client.GetAsync("/api/users/leader/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


}