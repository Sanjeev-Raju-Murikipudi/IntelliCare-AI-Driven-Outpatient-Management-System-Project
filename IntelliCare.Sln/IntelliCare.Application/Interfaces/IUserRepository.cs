using IntelliCare.Domain;
using System.Threading.Tasks;

public interface IUserRepository
{
    Task<User> GetByUsernameAsync(string username);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);

    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByMobileAsync(string mobile);
    Task DeleteAsync(User user);
    Task<IEnumerable<(User user, Doctor doctor)>> GetAllDoctorUsersAsync();



}
