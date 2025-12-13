using System.Net;
using System.Net.Http.Json;
using OneTime.Api.Models.UsersDto;
using OneTime.Api.Tests.TestHelpers;
using OneTime.Core.Models.Enums;

namespace OneTime.Api.Tests.Endpoints;

public class UsersControllerTests : IClassFixture<OneTimeApiFactory>
{
    private readonly HttpClient _client;

    public UsersControllerTests(OneTimeApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_Returns_Ok_With_Seeded_Users()
    {
        // Act
        var response = await _client.GetAsync("api/Users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        Assert.NotNull(users);
        Assert.True(users!.Count >= 2);
        Assert.Contains(users, u => u.UserId == 1 && u.Name == "Team Lead");
        Assert.Contains(users, u => u.UserId == 2 && u.Name == "Team Member");
    }

    [Fact]
    public async Task GetById_Existing_Returns_Ok()
    {
        var response = await _client.GetAsync("api/Users/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);
        Assert.Equal(1, user!.UserId);
        Assert.Equal("Team Lead", user.Name);
    }

    [Fact]
    public async Task GetById_NotFound_Returns_404()
    {
        var response = await _client.GetAsync("api/Users/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Employee_With_Manager_Succeeds()
    {
        var dto = new UserCreateDto(
            Name: "New Employee",
            Email: "new.employee@example.com",
            Password: "irrelevant-in-test",
            Role: (int)UserRole.Employee,
            ManagerId: 1 // must have a manager for Employee
        );

        var response = await _client.PostAsJsonAsync("api/Users", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(created);
        Assert.True(created!.UserId > 0);
        Assert.Equal(dto.Name, created.Name);
        Assert.Equal(dto.Email, created.Email);
        Assert.Equal(dto.Role, created.Role);
        Assert.Equal(dto.ManagerId, created.ManagerId);
    }

    [Fact]
    public async Task Create_InvalidRole_Returns_BadRequest()
    {
        var dto = new UserCreateDto(
            Name: "Bad Role",
            Email: "bad.role@example.com",
            Password: "x",
            Role: 999, // invalid
            ManagerId: null
        );

        var response = await _client.PostAsJsonAsync("api/Users", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var msg = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid role value.", msg);
    }

    [Fact]
    public async Task Update_User_Succeeds()
    {
        // Arrange
        var create = new UserCreateDto(
            Name: "Updatable User",
            Email: "updatable@example.com",
            Password: "x",
            Role: (int)UserRole.Employee,
            ManagerId: 1
        );
        var createdResp = await _client.PostAsJsonAsync("api/Users", create);
        var created = await createdResp.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(created);

        var update = new UserUpdateDto(
            Name: "Updated User",
            Email: "updated@example.com",
            Password: "x",
            Role: (int)UserRole.Employee,
            ManagerId: 1
        );

        // Act
        var response = await _client.PutAsJsonAsync($"api/Users/{created!.UserId}", update);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(updated);
        Assert.Equal(created.UserId, updated!.UserId);
        Assert.Equal(update.Name, updated.Name);
        Assert.Equal(update.Email, updated.Email);
        Assert.Equal(update.Role, updated.Role);
        Assert.Equal(update.ManagerId, updated.ManagerId);
    }

    [Fact]
    public async Task Update_NotFound_Returns_404()
    {
        var update = new UserUpdateDto(
            Name: "Does Not Exist",
            Email: "nope@example.com",
            Password: "x",
            Role: (int)UserRole.Employee,
            ManagerId: 1
        );

        var response = await _client.PutAsJsonAsync("api/Users/999", update);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Employee_Succeeds()
    {
        // Create a disposable employee
        var dto = new UserCreateDto(
            Name: "Disposable",
            Email: "disposable@example.com",
            Password: "x",
            Role: (int)UserRole.Employee,
            ManagerId: 1
        );
        var createResp = await _client.PostAsJsonAsync("api/Users", dto);
        var created = await createResp.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(created);

        // Act
        var response = await _client.DeleteAsync($"api/Users/{created!.UserId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var message = await response.Content.ReadAsStringAsync();
        Assert.Contains($"User with ID {created.UserId} was successfully deleted.", message);
    }

    [Fact]
    public async Task Delete_Manager_With_Team_Returns_BadRequest()
    {
        // Seeded manager has an employee → service should reject deletion
        var response = await _client.DeleteAsync("api/Users/1");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var message = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cannot delete manager with employees assigned.", message);
    }
}