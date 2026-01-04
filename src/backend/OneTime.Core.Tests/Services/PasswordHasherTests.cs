using OneTime.Core.Services.Implementations;

namespace OneTime.Core.Tests.Services;

public class PasswordHasherTests
{
    public PasswordHasherTests()
    {
        
    }

    [Fact]
    public void HashPassword_Empty_Throws()
    {
        var passwordHasher = new PasswordHasher();
        
        string password = "";
        
        var ex = Assert.Throws<ArgumentException>(() => passwordHasher.HashPassword(password));
        Assert.Equal("Password cannot be empty", ex.Message);
    }

    [Fact]
    public void HashPassword_Returns_NonEmpty_Hash_And_Salt()
    {
        var passwordHasher = new PasswordHasher();
        
        string password = "test";
        
        var result = passwordHasher.HashPassword(password);
        
        Assert.NotEmpty(result.Hash);
        Assert.NotEmpty(result.Salt);
    }

    [Fact]
    public void VerifyHashedPassword_Returns_True()
    {
        var passwordHasher = new PasswordHasher();
        var password = "test";

        var result = passwordHasher.HashPassword(password);
        
        var isValid = passwordHasher.VerifyPassword(password, result.Hash, result.Salt);

        Assert.True(isValid);
    }
    
    [Fact]
    public void VerifyHashedPassword_WrongPassword_Returns_False()
    {
        var passwordHasher = new PasswordHasher();
        var password = "test";
        
        var result = passwordHasher.HashPassword(password);
        
        var isValid = passwordHasher.VerifyPassword("wrong", result.Hash, result.Salt);
        
        Assert.False(isValid);
    }
    
    [Fact]
    public void VerifyHashedPassword_InvalidHashBase64_Returns_False()
    {
        var passwordHasher = new PasswordHasher();
        var password = "test";
        
        var result = passwordHasher.HashPassword(password);
        
        var isValid = passwordHasher.VerifyPassword(password, "BaseHash", result.Salt);
        
        Assert.False(isValid);
    }
    
    [Fact]
    public void VerifyHashedPassword_InvalidSaltBase64_Returns_False()
    {
        var passwordHasher = new PasswordHasher();
        var password = "test";
        
        var result = passwordHasher.HashPassword(password);
        
        var isValid = passwordHasher.VerifyPassword(password, result.Hash, "BaseSalt");
        
        Assert.False(isValid);
    }
    
    [Fact]
    public void VerifyPassword_WrongSizeHash_Returns_False()
    {
        var passwordHasher = new PasswordHasher();
        var password = "test";
        
        var result = passwordHasher.HashPassword(password);
        
        var isValid = passwordHasher.VerifyPassword(password, result.Hash.Substring(0, result.Hash.Length - 1), result.Salt);
        
        Assert.False(isValid);
    }
    
    [Fact]
    public void VerifyPassword_WrongSizeSalt_Returns_False()
    {
        var passwordHasher = new PasswordHasher();
        var password = "test";
        
        var result = passwordHasher.HashPassword(password);
        
        var isValid = passwordHasher.VerifyPassword(password, result.Hash, result.Salt.Substring(0, result.Salt.Length - 1));
        
        Assert.False(isValid);
    }
}