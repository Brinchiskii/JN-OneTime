using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Core.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<IEnumerable<JNUser>> GetAllUsers()
    {
        var users = await _userRepository.GetAll();

        // Return empty array if no users were found
        if (users == null)
        {
            return [];
        }
        
        return users;
    }

    public async Task<JNUser> GetUserById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("UserId must be greater than zero");

        var user = await _userRepository.GetById(id);

        if (user == null)
            throw new InvalidOperationException("User not found.");

        return user;
    }

    public async Task<JNUser> Create(JNUser user)
    {
        // Validate role/manager invariants as well for this overload
        ValidateRoleAndManager((UserRole)user.Role, user.ManagerId);

        var existingUser = await _userRepository.GetByEmail(user.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email is already in use.");
        }

        return await _userRepository.Create(user);
    }

    public async Task<JNUser> Create(string name, string email, UserRole role, int? managerId)
    {
        //ValidateRoleAndManager(role, managerId);

        var existing = await _userRepository.GetByEmail(email);
        if (existing != null)
            throw new InvalidOperationException("Email is already in use.");
        
        var user = new JNUser
        {
            Name = name,
            Email = email,
            PasswordHash = "",
            PasswordSalt = "",
            Role = (int)role,
            ManagerId = managerId
        };

        return await _userRepository.Create(user);
    }

    public async Task<JNUser> Update(int id, JNUser user)
    {
        // Checking if a user exists
        var existingUserById = await _userRepository.GetById(id);

        if (existingUserById == null)
            throw new InvalidOperationException("User not found.");

        // Checking if email is already in use
        var existingUserByEmail = await _userRepository.GetByEmail(user.Email);

        if (existingUserByEmail != null && existingUserByEmail.UserId != id)
        {
            throw new InvalidOperationException("Email is already in use.");
        }

        // Validate role/manager invariants
        ValidateRoleAndManager((UserRole)user.Role, user.ManagerId);

        // Updating user
        var updatedUser = await _userRepository.Update(existingUserById, user.Name, user.Email, (UserRole)user.Role, user.ManagerId);

        return updatedUser;
    }

    public async Task<JNUser> Update(int id, string name, string email, UserRole role, int? managerId)
    {
        var user = await _userRepository.GetById(id);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        ValidateRoleAndManager(role, managerId);

        var emailOwner = await _userRepository.GetByEmail(email);
        if (emailOwner != null && emailOwner.UserId != id)
            throw new InvalidOperationException("Email is already in use.");

        return await _userRepository.Update(user, name, email, role, managerId);
    }

    public async Task<JNUser> Delete(int id)
    {
        // Checking if a user exists
        var deleteUser = await _userRepository.GetById(id);

        if (deleteUser == null)
            throw new InvalidOperationException("User not found.");

        // Manager cannot be deleted if he has employees assigned
        if (((UserRole)deleteUser.Role) == UserRole.Manager)
        {
            var hasTeam = await _userRepository.CheckManagersTeam(deleteUser.UserId);
            if (hasTeam)
                throw new InvalidOperationException("Cannot delete manager with employees assigned.");
        }

        return await _userRepository.Delete(id);
    }

    private static void ValidateRoleAndManager(UserRole role, int? managerId)
    {
        switch (role)
        {
            case UserRole.Admin:
            case UserRole.Manager:
                if (managerId.HasValue)
                    throw new InvalidOperationException("Admins/managers cannot have a manager.");
                break;
            case UserRole.Employee:
                if (!managerId.HasValue)
                    throw new InvalidOperationException("Employees must have a manager.");
                break;
        }
    }
}