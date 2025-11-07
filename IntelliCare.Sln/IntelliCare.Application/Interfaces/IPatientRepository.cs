using IntelliCare.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliCare.Application.Interfaces;


public interface IPatientRepository
{
    Task CreateAsync(Patient patient);
    Task<Patient> GetByIdAsync(int id);
    Task<IEnumerable<Patient>> GetAllAsync();
    Task AddAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task DeleteAsync(Patient patient);

    Task<Patient> GetByUsernameAsync(string username);

}
