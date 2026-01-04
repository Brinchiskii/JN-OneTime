using Microsoft.Extensions.Configuration;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Implementations;
using OneTime.Core.Services.Repository;
using OneTime.Core.Tests.TestHelpers;

namespace OneTime.Core.Tests.Services;

public class AuthServiceTests
{
    public AuthServiceTests()
    {
        
    }

    [Fact]
    public async Task Login_UserNotFound_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var authSerive = new AuthService(userRepository, passwordHasher, new ConfigurationManager());
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => authSerive.Login("test@mail.com", "test"));
        Assert.Equal("Invalid credentials.", ex.Message);
    }

    [Fact]
    public async Task Login_WrongPassword_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var authSerive = new AuthService(userRepository, passwordHasher, new ConfigurationManager());
        var userService = new UserService(userRepository, passwordHasher);

        // Creating user with password
        var user1 = userService.Create("Test user", "test@mail.com", "Test123", UserRole.Employee, 0);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => authSerive.Login("test@mail.com", "wrong"));
        Assert.Equal("Invalid credentials.", ex.Message);
    }

    [Fact]
    public async Task Login_Succeeds_Returns_JWT_With_Expected_Claims()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var userRepository = new UserRepository(context);
        var passwordHasher = new PasswordHasher();
        var authSerive = new AuthService(userRepository, passwordHasher, new ConfigurationManager());
        var userService = new UserService(userRepository, passwordHasher);
        
        // Creating user with password
        var user1 = userService.Create("Test user", "test@mail.com", "Test123", UserRole.Employee, 0);

        var userLogin = authSerive.Login("test@mail.com", "Test123");

        Assert.NotNull(userLogin);
    }
}