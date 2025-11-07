using IntelliCare.Domain;
//using IntelliCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class DoctorRepository : IDoctorRepository
{
    private readonly ApplicationDbContext _context;

    public DoctorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Doctor doctor)
    {
        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();
    }

    public async Task<Doctor> GetByIdAsync(int doctorId)
    {
        return await _context.Doctors.FindAsync(doctorId);
    }

    public async Task UpdateAsync(Doctor doctor)
    {
        _context.Doctors.Update(doctor);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Doctor>> GetAllAsync()
    {
        return await _context.Doctors.ToListAsync();
    }


    public async Task<Doctor?> GetByUsernameAsync(string username)

    {

        var user = await _context.Users

            .Include(u => u.Doctor)

            .FirstOrDefaultAsync(u => u.Username == username);

        return user?.Doctor;

    }

    public async Task<IEnumerable<Doctor>> GetAllDoctorsAsync()

    {

        return await _context.Doctors.ToListAsync();

    }



    public async Task<Doctor> GetDoctorByIdAsync(int doctorId)

    {

        return await _context.Doctors

            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

    }



}
