

using IntelliCare.Application.Interfaces; 
using IntelliCare.Domain; 
using IntelliCare.Infrastructure; 
using Microsoft.EntityFrameworkCore;

public class ClinicalRecordRepository : IClinicalRecordRepository
{

    private readonly ApplicationDbContext _context;


    public ClinicalRecordRepository(ApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<ClinicalRecord> AddRecordAsync(ClinicalRecord record)
    {

        _context.ClinicalRecords.Add(record);


        await _context.SaveChangesAsync();


        return record;
    }

    // 2. Implementation of GetByAppointmentIdAsync
    public async Task<ClinicalRecord> GetByAppointmentIdAsync(int appointmentId)
    {

        return await _context.ClinicalRecords

                             .FirstOrDefaultAsync(r => r.AppointmentID == appointmentId);
    }




    public async Task UpdateAsync(ClinicalRecord record)
    {
        _context.ClinicalRecords.Update(record);
        await _context.SaveChangesAsync();

    }

    public async Task<ClinicalRecord> GetByIdAsync(int recordId)
    {

        return await _context.ClinicalRecords
                             .FirstOrDefaultAsync(r => r.ClinicalRecordID == recordId);
    }


    public async Task<List<ClinicalRecord>> GetAllAsync()
    {
        return await _context.ClinicalRecords.ToListAsync();
    }
































































    // 3. Implementation of UpdateRecordAsync
    public async Task<ClinicalRecord> UpdateRecordAsync(ClinicalRecord record)
    {
        _context.ClinicalRecords.Update(record);


        await _context.SaveChangesAsync();

        return record;
    }
}