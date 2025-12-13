using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Implementations;
using OneTime.Core.Services.Repository;
using OneTime.Core.Tests.TestHelpers;

namespace OneTime.Core.Tests.Services;

public class UserServiceTests
{
    [Fact]
    public async Task GetAllUsers_Returns_UsersOk()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();

        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);

        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test User",
            Email = "test@test.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 2,
            ManagerId = 1
        });
        
        context.JNUsers.Add(new JNUser
        {
            UserId = 2,
            Name = "Test User 2",
            Email = "test2@test.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 1,
            ManagerId = 1
        });
        
        await context.SaveChangesAsync();
        
        var users = await userService.GetAllUsers();
        
        Assert.NotEmpty(users);
    }
    
    [Fact]
    public async Task GetAllUsers_Returns_UsersEmpty()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();

        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);
        
        var users = await userService.GetAllUsers();
        
        Assert.Empty(users);
    }

    [Fact]
    public async Task GetUserById_Returns_User_When_Id_Is_Zero()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => userService.GetUserById(0));
        Assert.Equal("UserId must be greater than zero", ex.Message);
    }
    
    [Fact]
    public async Task GetUserById_Returns_User_When_Id_Is_Negative()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => userService.GetUserById(-1));
        Assert.Equal("UserId must be greater than zero", ex.Message);
    }
    
    [Fact]
    public async Task GetUserById_Returns_UserNotFound()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.GetUserById(1));
        Assert.Equal("User not found.", ex.Message);
    }
    
    [Fact]
    public async Task GetById_Returns_UserOk()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);
        
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test User",
            Email = "test@test.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 2,
            ManagerId = 1
        });
        
        context.JNUsers.Add(new JNUser
        {
            UserId = 2,
            Name = "Test User 2",
            Email = "test2@test.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 1,
            ManagerId = 1
        });

        await context.SaveChangesAsync();

        var user = userService.GetUserById(2);
        
        var result = await user;
        
        Assert.NotNull(result);
        Assert.Equal(2, result.UserId);
        Assert.Equal("Test User 2", result.Name);
        Assert.Equal("test2@test.com", result.Email);
        Assert.Equal("TestHash" , result.PasswordHash);
        Assert.Equal("TestSalt", result.PasswordSalt);
        Assert.Equal(1, result.Role);
        Assert.Equal(1, result.ManagerId);
    }

    [Fact]
    public async Task CreateUser_Ok()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);
        
        string name = "Test User";
        string email = "test@test.com";
        UserRole role = UserRole.Employee;
        int? managerId = 1;
        
        var userCreated = await userService.Create(name, email, role, managerId);
        
        Assert.NotNull(userCreated);
        Assert.NotEqual(0, userCreated.UserId);
        Assert.Equal(userCreated.Name, name);
        Assert.Equal(userCreated.Email, email);
        Assert.Equal(userCreated.Role, (int)role);
        Assert.Equal(userCreated.ManagerId, managerId);
    }

    [Fact]
    public async Task CreateUser_EmailAlreadyExists()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);
        
        // Adds user to the db
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test User",
            Email = "test@test.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 2,
            ManagerId = 1
        });
        
        await context.SaveChangesAsync();
        
        // New user with same email
        string name = "Test User2";
        string email = "test@test.com";
        UserRole role = UserRole.Employee;
        int? managerId = 1;
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.Create(name, email, role, managerId));
        Assert.Equal("Email is already in use.", ex.Message);
    }

    [Fact]
    public async Task UpdateUser_UserNotFound()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.Update(1, new JNUser()));
        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task UpdateUser_Ok()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);
        
        // Adds user to the db
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test User",
            Email = "test@test.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 2,
            ManagerId = 1
        });
        
        await context.SaveChangesAsync();
        
        var user = new JNUser { UserId = 1, Name = "Test User Updated", Email = "test@test.com"};
        
        var userUpdated = await userService.Update(1, user);
        
        Assert.NotNull(userUpdated);
        Assert.Equal(1, userUpdated.UserId);
        Assert.Equal("Test User Updated", userUpdated.Name);
    }
    
    [Fact]
    public async Task DeleteUser_UserNotFound()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.Delete(1));
        Assert.Equal("User not found.", ex.Message);
    }
    
    [Fact]
    public async Task DeleteUser_Ok()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);
        
        // Adds user to the db
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test User",
            Email = "test@test.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 2,
            ManagerId = 1
        });
        
        await context.SaveChangesAsync();
        
        await userService.Delete(1);
        
        Assert.Empty(context.JNUsers);
    }

    [Fact]
    public async Task DeleteUser_Manager_With_Team()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);
        
        // Adds user to the db
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test User",
            Email = "test@test.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 1, // <- Manager
            ManagerId = 0
        });
        
        context.JNUsers.Add(new JNUser
        {
            UserId = 2,
            Name = "Test User 2",
            Email = "test2@test.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 0, // <- Employee
            ManagerId = 1
        });
        
        await context.SaveChangesAsync();
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.Delete(1));
        Assert.Equal("Cannot delete manager with employees assigned.", ex.Message);
    }
    
    [Fact]
    public async Task DeleteUser_Manager_Without_Team()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);
        
        // Adds user to the db
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test User",
            Email = "test@test.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 1, // <- Manager
            ManagerId = 0
        });
        
        await context.SaveChangesAsync();
        
        userService.Delete(1);
        
        Assert.Empty(context.JNUsers);
    }
}