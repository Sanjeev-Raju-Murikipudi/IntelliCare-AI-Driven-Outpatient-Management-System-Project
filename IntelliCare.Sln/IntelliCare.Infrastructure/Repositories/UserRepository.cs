using IntelliCare.Application.Interfaces;
using IntelliCare.Domain;
using IntelliCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserRepository> _logger;
    //public UserRepository(ApplicationDbContext context)
    //{
    //    _context = context;
    //}

    public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty.");

        return await _context.Users
            .Include(u => u.Patient)
            .Include(u => u.Doctor)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.ContactEmail == email);
    }

    public async Task CreateAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(User user)
    {
    _context.Users.Remove(user);
    await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByMobileAsync(string mobile)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.MobileNumber == mobile);
    }


    public async Task<IEnumerable<(User user, Doctor doctor)>> GetAllDoctorUsersAsync()
    {
        return await _context.Users
            .Where(u => u.RoleName == UserRole.Doctor && u.DoctorID != null)
            .Include(u => u.Doctor)
            .Select(u => new ValueTuple<User, Doctor>(u, u.Doctor))
            .ToListAsync();
    }

}
