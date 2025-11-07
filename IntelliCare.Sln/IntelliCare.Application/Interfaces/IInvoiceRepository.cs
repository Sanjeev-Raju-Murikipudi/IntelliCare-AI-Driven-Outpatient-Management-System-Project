using IntelliCare.Domain;

namespace IntelliCare.Application.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<IEnumerable<Invoice>> GetAllAsync();
        Task<Invoice> GetByIdAsync(int id);
        Task<Invoice> CreateAsync(Invoice invoice);
        Task UpdateAsync(Invoice invoice);
        Task DeleteAsync(int id);

        Task<IEnumerable<Invoice>> GetByStatusAsync(string status);
        Task<IEnumerable<Invoice>> GetByClaimStatusAsync(string claimStatus);
        Task<IEnumerable<Invoice>> GetByPatientIdAsync(int patientId);

        Task<double> GetTotalRevenueForDoctorAsync(int doctorId, DateTime start, DateTime end);

    }
}