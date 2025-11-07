using IntelliCare.Domain;
using IntelliCare.Infrastructure;
using Microsoft.EntityFrameworkCore;

public class SupportDataRepository : ISupportDataRepository
{
    private readonly ApplicationDbContext _context;

    public SupportDataRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SupportData> AddAsync(SupportData data)
    {
        _context.SupportData.Add(data);
        await _context.SaveChangesAsync();
        return data;
    }

    public async Task<IEnumerable<SupportData>> GetByPatientIdAsync(int patientId)
    {
        return await _context.SupportData
            .Where(d => d.DoctorID == patientId)
            .ToListAsync();
    }
}
