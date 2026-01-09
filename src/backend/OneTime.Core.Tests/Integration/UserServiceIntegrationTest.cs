using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Implementations;
using OneTime.Core.Services.Repository;
using OneTime.Core.Tests.TestHelpers;

namespace OneTime.Core.Tests.Integration;

public class UserServiceIntegrationTest
{
    [Fact]
    public async Task GetAllUsers_Returns_UsersOk()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();

        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);

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
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
        var users = await userService.GetAllUsers();
        
        Assert.Empty(users);
    }

    [Fact]
    public async Task GetUserById_Returns_User_When_Id_Is_Zero()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => userService.GetUserById(0));
        Assert.Equal("UserId must be greater than zero", ex.Message);
    }
    
    [Fact]
    public async Task GetUserById_Returns_User_When_Id_Is_Negative()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => userService.GetUserById(-1));
        Assert.Equal("UserId must be greater than zero", ex.Message);
    }
    
    [Fact]
    public async Task GetUserById_Returns_UserNotFound()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.GetUserById(1));
        Assert.Equal("User not found.", ex.Message);
    }
    
    [Fact]
    public async Task GetById_Returns_UserOk()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
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
    public async Task GetUsersByLeaderId_Returns_Users()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);

        var employee = new JNUser
        {
            UserId = 1,
            Name = "Test User",
            Email = "test@test.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = (int)UserRole.Employee,
            ManagerId = 2
        };

        context.JNUsers.Add(employee);
        
        // Manager
        context.JNUsers.Add(new JNUser
        {
            UserId = 2,
            Name = "Test User 2",
            Email = "test2@test.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = (int)UserRole.Manager,
            ManagerId = 1
        });

        await context.SaveChangesAsync();
        
        var users = await userService.GetUsersByLeaderId(2);
        Assert.Single(users);
        Assert.Contains(employee, users);
    }

    [Fact]
    public async Task GetUsersByLeaderId_Returns_Empty()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
        // Manager
        context.JNUsers.Add(new JNUser
        {
            UserId = 2,
            Name = "Test User 2",
            Email = "test2@test.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = (int)UserRole.Manager,
            ManagerId = 1
        });
        
        await context.SaveChangesAsync();
        
        var users = await userService.GetUsersByLeaderId(2);
        Assert.Empty(users);
    }

    [Fact]
    public async Task GetUserByLeaderId_Returns_User_When_Id_Is_Zero()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => userService.GetUsersByLeaderId(0));
        Assert.Equal("UserId must be greater than zero", ex.Message);
    }
    
    [Fact]
    public async Task GetUserByLeaderId_Returns_User_When_Id_Is_Negative()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => userService.GetUsersByLeaderId(-1));
        Assert.Equal("UserId must be greater than zero", ex.Message);
    }

    [Fact]
    public async Task CreateUser_Ok()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
        string name = "Test User";
        string email = "test@test.com";
        string password = "x";
        UserRole role = UserRole.Employee;
        int? managerId = 1;
        
        var userCreated = await userService.Create(name, email, password, role, managerId);
        
        Assert.NotNull(userCreated);
        Assert.NotEqual(0, userCreated.UserId);
        Assert.Equal(userCreated.Name, name);
        Assert.Equal(userCreated.Email, email);
        Assert.NotEqual(userCreated.PasswordHash, password);
        Assert.Equal(userCreated.Role, (int)role);
        Assert.Equal(userCreated.ManagerId, managerId);
    }

    [Fact]
    public async Task CreateUser_EmailAlreadyExists()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
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
        string password = "x";
        UserRole role = UserRole.Employee;
        int? managerId = 1;
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.Create(name, email, password, role, managerId));
        Assert.Equal("Email is already in use.", ex.Message);
    }

    [Fact]
    public async Task Create_Employee_Without_Manager_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
        await context.SaveChangesAsync();
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            userService.Create("Test user", "test@mail.com", "test", UserRole.Employee, null));
        Assert.Equal("Employees must have a manager.", ex.Message);

    }

    [Fact]
    public async Task Create_Admin_With_Manager_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
        await context.SaveChangesAsync();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            userService.Create("Test user", "test@mail.com", "Test", UserRole.Admin, 2));
        Assert.Equal("Admins/managers cannot have a manager.", ex.Message);
    }
    
    [Fact]
    public async Task Create_Manager_With_Manager_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
        await context.SaveChangesAsync();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            userService.Create("Test user", "test@mail.com", "Test", UserRole.Manager, 2));
        Assert.Equal("Admins/managers cannot have a manager.", ex.Message);
    }
    
    [Fact]
    public async Task UpdateUser_UserNotFound()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.Update(1, "Test User", "test@test.com", UserRole.Employee, 13 ));
        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task UpdateUser_Ok()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
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
        
        var user = new JNUser { UserId = 1, Name = "Test User Updated", Email = "test@test.com", PasswordHash = "TestHash", PasswordSalt = "TestSalt", Role = 2, ManagerId = 1};
        
        var userUpdated = await userService.Update(user.UserId, user.Name, user.Email, (UserRole)user.Role, user.ManagerId);
        
        Assert.NotNull(userUpdated);
        Assert.Equal(1, userUpdated.UserId);
        Assert.Equal("Test User Updated", userUpdated.Name);
    }

    [Fact]
    public async Task Update_Email_Already_In_Use_By_OtherUser_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);

        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test User 1",
            Email = "test@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
        });
        context.JNUsers.Add(new JNUser
        {
            UserId = 2,
            Name = "Test User 2",
            Email = "test2@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 1
        });
        
        await context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            userService.Update(1, "Test user 2", "test2@mail.com", UserRole.Employee, 2));
        Assert.Equal("Email is already in use.", ex.Message);
    }

    [Fact]
    public async Task Update_Employee_Without_Manager_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
        // Employee
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test User 1",
            Email = "test@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
        });
        
        await context.SaveChangesAsync();
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            userService.Update(1, "Test user", "test@mail.com", UserRole.Employee, null));
        Assert.Equal("Employees must have a manager.", ex.Message);
    }

    [Fact]
    public async Task Update_Admin_With_Manager_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);

        // Admin
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test User 1",
            Email = "test@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 0
        });
        
        // Manager
        context.JNUsers.Add(new JNUser
        {
            UserId = 2,
            Name = "Test User 2",
            Email = "test2@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 1
        });
        
        await context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            userService.Update(1, "Test user 1", "test@mail.com", UserRole.Admin, 2));
        Assert.Equal("Admins/managers cannot have a manager.", ex.Message);
    }
    
    [Fact]
    public async Task Update_Manager_With_Manager_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);

        // Manager
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test User 1",
            Email = "test@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 1
        });
        
        // Manager
        context.JNUsers.Add(new JNUser
        {
            UserId = 2,
            Name = "Test User 2",
            Email = "test2@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 1
        });
        
        await context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            userService.Update(1, "Test user 1", "test@mail.com", UserRole.Admin, 2));
        Assert.Equal("Admins/managers cannot have a manager.", ex.Message);
    }
    
    [Fact]
    public async Task DeleteUser_UserNotFound()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.Delete(1));
        Assert.Equal("User not found.", ex.Message);
    }
    
    [Fact]
    public async Task DeleteUser_Ok()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
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
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
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
        var passwordHasher = new PasswordHasher();
        var userService = new UserService(userRepository, passwordHasher);
        
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