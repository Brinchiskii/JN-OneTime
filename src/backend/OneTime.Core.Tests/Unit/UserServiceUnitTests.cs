using Moq;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Implementations;
using OneTime.Core.Services.Interfaces;
using Xunit;

namespace OneTime.Core.Tests.Unit;

public class UserServiceUnitTests
{
    [Fact]
    public async Task GetUserById_IdIsZero_ThrowsArgumentException()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var userService = new UserService(
            userRepositoryMock.Object,
            passwordHasherMock.Object
        );
        
        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => userService.GetUserById(0));
        
        Assert.Equal("UserId must be greater than zero", ex.Message);
        
        userRepositoryMock.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetUserById_IdIsNegative_ThrowsArgumentException()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var userService = new UserService(
            userRepositoryMock.Object,
            passwordHasherMock.Object
        );
        
        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => userService.GetUserById(-1));
        
        Assert.Equal("UserId must be greater than zero", ex.Message);
        
        userRepositoryMock.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetUserById_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var userService = new UserService(
            userRepositoryMock.Object,
            passwordHasherMock.Object
        );
        
        // Act + Assert
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.GetUserById(1));
        
        Assert.Equal("User not found.", ex.Message);
        
        userRepositoryMock.Verify(r => r.GetById(1), Times.Once);
    }

    [Fact]
    public async Task Create_EmployeeWithoutManager_ThrowsInvalidOperationException()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var userService = new UserService(
            userRepositoryMock.Object,
            passwordHasherMock.Object
        );
        
        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.Create(
            name: "Test user",
            email: "test@test.com",
            password: "password",
            role: UserRole.Employee,
            managerId: null));
        
        Assert.Equal("Employees must have a manager.", ex.Message);
        
        userRepositoryMock.Verify(r => r.GetByEmail(It.IsAny<string>()), Times.Never);
        
        userRepositoryMock.Verify(r => r.Create(It.IsAny<JNUser>()), Times.Never);
        
        passwordHasherMock.Verify(r => r.HashPassword(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Create_AdminWithManager_ThrowsInvalidOperationException()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var userService = new UserService(
            userRepositoryMock.Object,
            passwordHasherMock.Object
        );
        
        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.Create(
            name: "Test user",
            email: "test@test.com",
            password: "password",
            role: UserRole.Admin,
            managerId: 1));
        
        Assert.Equal("Admins/managers cannot have a manager.", ex.Message);
        
        userRepositoryMock.Verify(r => r.GetByEmail(It.IsAny<string>()), Times.Never);
        
        userRepositoryMock.Verify(r => r.Create(It.IsAny<JNUser>()), Times.Never);
        
        passwordHasherMock.Verify(r => r.HashPassword(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task Create_ManagerWithManager_ThrowsInvalidOperationException()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var userService = new UserService(
            userRepositoryMock.Object,
            passwordHasherMock.Object
        );
        
        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.Create(
            name: "Test user",
            email: "test@test.com",
            password: "password",
            role: UserRole.Manager,
            managerId: 1));
        
        Assert.Equal("Admins/managers cannot have a manager.", ex.Message);
        
        userRepositoryMock.Verify(r => r.GetByEmail(It.IsAny<string>()), Times.Never);
        
        userRepositoryMock.Verify(r => r.Create(It.IsAny<JNUser>()), Times.Never);
        
        passwordHasherMock.Verify(r => r.HashPassword(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Create_EmailAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        // Test data
        var existingUser = new JNUser
        {
            UserId = 42,
            Email = "test@test.com"
        };

        userRepositoryMock
            .Setup(r => r.GetByEmail("test@test.com"))
            .ReturnsAsync(existingUser);
        
        var userService = new UserService(
            userRepositoryMock.Object,
            passwordHasherMock.Object
        );
        
        // Act + Assert

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.Create(
            name: "New user",
            email: "test@test.com",
            password: "password",
            role: UserRole.Employee,
            managerId: 1));
        
        Assert.Equal("Email is already in use.", ex.Message);
        
        userRepositoryMock.Verify(r => r.GetByEmail(It.IsAny<string>()), Times.Once);
        
        userRepositoryMock.Verify(r => r.Create(It.IsAny<JNUser>()), Times.Never);
        
        passwordHasherMock.Verify(r => r.HashPassword(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Create_ValidInput_UsesPasswordHasher()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        // Test data
        var fakeHash = "fakeHash";
        var fakeSalt = "fakeSalt";

        passwordHasherMock
            .Setup(r => r.HashPassword("password"))
            .Returns((fakeHash, fakeSalt));
        
        userRepositoryMock
            .Setup(r => r.GetByEmail(It.IsAny<string>()))
            .ReturnsAsync((JNUser?)null);

        userRepositoryMock
            .Setup(r => r.Create(It.IsAny<JNUser>()))
            .ReturnsAsync((JNUser u) => u);
        
        var userService = new UserService(
            userRepositoryMock.Object,
            passwordHasherMock.Object
        );
        
        // Act

        var result = await userService.Create(
            name: "Test user",
            email: "test@test.com",
            password: "password",
            role: UserRole.Employee,
            managerId: 1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(fakeHash, result.PasswordHash);
        Assert.Equal(fakeSalt, result.PasswordSalt);
        Assert.Equal("test@test.com", result.Email);
        
        passwordHasherMock.Verify(r => r.HashPassword("password"), Times.Once);
        
        userRepositoryMock.Verify(r => r.Create(It.IsAny<JNUser>()), Times.Once);
        
        userRepositoryMock.Verify(r => r.GetByEmail(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Update_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var userService = new UserService(
            userRepositoryMock.Object,
            passwordHasherMock.Object
        );
        
        // Act + Assert
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.Update(
            1, "newName", "newEmail", UserRole.Employee, 1));
        
        Assert.Equal("User not found.", ex.Message);
        
        userRepositoryMock.Verify(r => r.GetById(1), Times.Once);
    }
    
    [Fact]
    public async Task Update_EmployeeWithoutManager_ThrowsInvalidOperationException()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var existingUser = new JNUser
        {
            UserId = 1,
            Role = (int)UserRole.Employee
        };

        userRepositoryMock
            .Setup(r => r.GetById(1))
            .ReturnsAsync(existingUser);

        var service = new UserService(userRepositoryMock.Object, passwordHasherMock.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Update(1, "Name", "mail@test.com", UserRole.Employee, null));

        Assert.Equal("Employees must have a manager.", ex.Message);
        
        
    }
    
    [Fact]
    public async Task Update_ManagerWithManager_ThrowsInvalidOperationException()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        userRepositoryMock
            .Setup(r => r.GetById(1))
            .ReturnsAsync(new JNUser { UserId = 1 });

        var service = new UserService(userRepositoryMock.Object, passwordHasherMock.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Update(1, "Name", "mail@test.com", UserRole.Manager, 2));

        Assert.Equal("Admins/managers cannot have a manager.", ex.Message);
    }
    
    [Fact]
    public async Task Update_AdminWithManager_ThrowsInvalidOperationException()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        userRepositoryMock
            .Setup(r => r.GetById(1))
            .ReturnsAsync(new JNUser { UserId = 1 });

        var service = new UserService(userRepositoryMock.Object, passwordHasherMock.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Update(1, "Name", "mail@test.com", UserRole.Admin, 2));

        Assert.Equal("Admins/managers cannot have a manager.", ex.Message);
    }

    [Fact]
    public async Task Update_EmailInUseByOtherUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var existingUser1 = new JNUser
        {
            UserId = 1,
            Name = "Test user",
            Email = "test1@test.com"
        };
        
        var existingUser2 = new JNUser
        {
            UserId = 2,
            Name = "Test user",
            Email = "test2@test.com"
        };
        
        userRepositoryMock
            .Setup(r => r.GetById(1))
            .ReturnsAsync(existingUser1);
        
        userRepositoryMock
            .Setup(r => r.GetByEmail("test2@test.com"))
            .ReturnsAsync(existingUser2);

        var userService = new UserService(
            userRepositoryMock.Object,
            passwordHasherMock.Object
        );
        
        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.Update(
            id: 1,
            name: "newName",
            email: "test2@test.com",
            role: UserRole.Employee,
            managerId: 2));
        
        Assert.Equal("Email is already in use.", ex.Message);
        
        userRepositoryMock.Verify(r => r.GetById(1), Times.Once);
        
        userRepositoryMock.Verify(r => r.GetByEmail("test2@test.com"), Times.Once);
    }
    
    [Fact]
    public async Task Delete_UserNotFound_ThrowsInvalidOperationException()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        userRepositoryMock
            .Setup(r => r.GetById(1))!
            .ReturnsAsync((JNUser?)null);

        var service = new UserService(userRepositoryMock.Object, passwordHasherMock.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Delete(1));

        Assert.Equal("User not found.", ex.Message);
        
        userRepositoryMock.Verify(r => r.GetById(1), Times.Once);
        
        userRepositoryMock.Verify(r => r.Delete(1), Times.Never);
    }
    
    [Fact]
    public async Task Delete_ManagerWithTeam_ThrowsInvalidOperationException()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var manager = new JNUser
        {
            UserId = 1,
            Role = (int)UserRole.Manager
        };

        userRepositoryMock
            .Setup(r => r.GetById(1))
            .ReturnsAsync(manager);

        userRepositoryMock
            .Setup(r => r.CheckManagersTeam(1))
            .ReturnsAsync(true);

        var service = new UserService(userRepositoryMock.Object, passwordHasherMock.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Delete(1));

        Assert.Equal("Cannot delete manager with employees assigned.", ex.Message);
        
        userRepositoryMock.Verify(r => r.GetById(1), Times.Once);
        
        userRepositoryMock.Verify(r => r.CheckManagersTeam(1), Times.Once);
        
        userRepositoryMock.Verify(r => r.Delete(1), Times.Never);
    }
    
    [Fact]
    public async Task GetUsersByLeaderId_IdIsZero_ThrowsArgumentException()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var service = new UserService(userRepositoryMock.Object, passwordHasherMock.Object);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetUsersByLeaderId(0));

        Assert.Equal("UserId must be greater than zero", ex.Message);
        
        userRepositoryMock.Verify(r => r.GetUsersByLeaderId(It.IsAny<int>()), Times.Never);
    }
    
    [Fact]
    public async Task GetUsersByLeaderId_IdIsNegative_ThrowsArgumentException()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var service = new UserService(userRepositoryMock.Object, passwordHasherMock.Object);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetUsersByLeaderId(-1));

        Assert.Equal("UserId must be greater than zero", ex.Message);
        
        userRepositoryMock.Verify(r => r.GetUsersByLeaderId(It.IsAny<int>()), Times.Never);
    }

}