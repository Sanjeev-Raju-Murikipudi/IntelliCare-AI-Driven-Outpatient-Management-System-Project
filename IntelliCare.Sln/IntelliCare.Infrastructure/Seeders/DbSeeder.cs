//using IntelliCare.Domain.Entities;
using IntelliCare.Domain.Enums;
using IntelliCare.Application.Interfaces;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using IntelliCare.Domain;

public class DbSeeder
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(IUserRepository userRepository, ILogger<DbSeeder> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task SeedAdminAsync()
    {
        var existingAdmin = await _userRepository.GetByUsernameAsync("admin1");
        if (existingAdmin != null)
        {
            _logger.LogInformation("Admin user already exists.");
            return;
        }

        var adminUser = new User
        {
            Username = "admin1",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            RoleName = UserRole.Admin,
            MobileNumber = "9390362455",
            ContactEmail = "intellicare108@gmail.com"
        };

        await _userRepository.CreateAsync(adminUser);
        _logger.LogInformation("✅ Seeded admin1 user.");
    }
}
