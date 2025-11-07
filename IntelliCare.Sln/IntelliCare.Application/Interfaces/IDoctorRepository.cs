using IntelliCare.Domain;

public interface IDoctorRepository
{
    Task CreateAsync(Doctor doctor);
    Task<Doctor> GetByIdAsync(int doctorId);
    Task<IEnumerable<Doctor>> GetAllAsync();

    Task UpdateAsync(Doctor doctor);

    Task<Doctor?> GetByUsernameAsync(string username);

    Task<IEnumerable<Doctor>> GetAllDoctorsAsync();

    Task<Doctor> GetDoctorByIdAsync(int doctorId);




}
