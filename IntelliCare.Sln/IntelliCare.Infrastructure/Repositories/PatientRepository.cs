using IntelliCare.Domain;
using IntelliCare.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliCare.Infrastructure
{
    public class PatientRepository : IPatientRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PatientRepository> _logger;

        public PatientRepository(ApplicationDbContext context, ILogger<PatientRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Patient> GetByIdAsync(int id)
        {
            return await _context.Patients.FindAsync(id);
        }

        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            return await _context.Patients.ToListAsync();
        }

        public async Task AddAsync(Patient patient)
        {
            await _context.Patients.AddAsync(patient);
            await SaveAsync("AddAsync");
        }

        public async Task<Patient> GetByUsernameAsync(string username)
        {
            return await _context.Patients
                .FirstOrDefaultAsync(p => p.UserName == username);
        }

        public async Task UpdateAsync(Patient patient)
        {
            _context.Entry(patient).State = EntityState.Modified;
            await SaveAsync("UpdateAsync");
        }

        public async Task CreateAsync(Patient patient)
        {
            _context.Patients.Add(patient);
            await SaveAsync("CreateAsync");
        }

        public async Task DeleteAsync(Patient patient)
        {
            _context.Patients.Remove(patient);
            await SaveAsync("DeleteAsync");
        }

        private async Task SaveAsync(string method)
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during {method} in PatientRepository.");
                throw;
            }
        }
    }
}
