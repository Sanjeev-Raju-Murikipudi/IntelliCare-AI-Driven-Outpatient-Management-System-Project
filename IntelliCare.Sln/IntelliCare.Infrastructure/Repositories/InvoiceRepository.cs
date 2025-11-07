using IntelliCare.Application.Interfaces;
using IntelliCare.Domain;
using Microsoft.EntityFrameworkCore;

namespace IntelliCare.Infrastructure.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public InvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync() =>
            await _context.Invoices.ToListAsync();

        public async Task<Invoice> GetByIdAsync(int id) =>
            await _context.Invoices
            .Include(i => i.Patient)
            .FirstOrDefaultAsync(i => i.InvoiceID == id);

        public async Task<Invoice> CreateAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task UpdateAsync(Invoice invoice)
        {
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<IEnumerable<Invoice>> GetByStatusAsync(string status)
        {
            return await _context.Invoices
                .Where(i => i.Status.ToLower() == status.ToLower())
                .ToListAsync();
        }


        public async Task<IEnumerable<Invoice>> GetByClaimStatusAsync(string claimStatus)
        {
            return await _context.Invoices
                .Where(i => i.ClaimStatus.ToLower() == claimStatus.ToLower())
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByPatientIdAsync(int patientId)
        {
            return await _context.Invoices
                .Where(i => i.PatientID == patientId)
                .ToListAsync();
        }




        public async Task<double> GetTotalRevenueForDoctorAsync(
            int doctorId,
            DateTime start,
            DateTime end)
        {
            var queryStart = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            var queryEnd = DateTime.SpecifyKind(end, DateTimeKind.Utc);

            
            queryEnd = queryEnd.AddSeconds(1);

           
            var totalRevenue = await (
                from appointment in _context.Appointments

                where appointment.DoctorID == doctorId
                    && appointment.Date_Time >= queryStart
                    && appointment.Date_Time <= queryEnd

                
                join clinicalRecord in _context.ClinicalRecords
                    on appointment.AppointmentID equals clinicalRecord.AppointmentID

             
                join invoice in _context.Invoices
                    on clinicalRecord.ClinicalRecordID equals invoice.ClinicalRecordID

                select invoice
            )
            .Distinct()
            
            .Where(i => i.Status.ToLower() == "paid")
            .SumAsync(i => (double?)i.Amount) ?? 0.0;

            return totalRevenue;
        }


    }
}






























//// Services/ReportService.cs (Implementation of the reporting logic)
//public async Task<double> GetTotalRevenueForDoctorAsync(
//    int doctorId,
//    DateTime start,
//    DateTime end)
//{
//    var queryStart = DateTime.SpecifyKind(start, DateTimeKind.Utc);
//    var queryEnd = DateTime.SpecifyKind(end, DateTimeKind.Utc);

//    // Safety measure: Ensure the end range is inclusive.
//    queryEnd = queryEnd.AddSeconds(1);

//    // --- Query Logic: Updated to use ClinicalRecord for accurate linkage ---
//    var totalRevenue = await (
//        from appointment in _context.Appointments

//            // 1. Filter by Doctor and Time
//        where appointment.DoctorID == doctorId
//            && appointment.Date_Time >= queryStart
//            && appointment.Date_Time <= queryEnd

//        // 2. Join Appointment to ClinicalRecord (Consultation)
//        join clinicalRecord in _context.ClinicalRecords // Assumes _context.ClinicalRecords exists
//            on appointment.AppointmentID equals clinicalRecord.AppointmentID

//        // 3. Join ClinicalRecord to Invoice (The Bill for the service)
//        // This is the CRITICAL change, linking the service to the charge.
//        join invoice in _context.Invoices
//            on clinicalRecord.ClinicalRecordID equals invoice.ClinicalRecordID

//        select invoice
//    )
//    .Distinct()
//    // We only care about PAID invoices for accurate revenue reporting (Best Practice)
//    // .Where(i => i.Status.ToLower() == "paid") // <-- RECOMMENDED FILTER
//    .SumAsync(i => (double?)i.Amount) ?? 0.0;

//    return totalRevenue;
//}




//public async Task<double> GetTotalRevenueForDoctorAsync(
//    int doctorId,
//    DateTime start,
//    DateTime end)
//{
//    // --- Time Handling (Keep the existing hack if DB requires it) ---
//    var queryStart = DateTime.SpecifyKind(start, DateTimeKind.Utc);
//    var queryEnd = DateTime.SpecifyKind(end, DateTimeKind.Utc);

//    // Safety measure: Ensure the end range is inclusive (add a millisecond instead of a full second)
//    queryEnd = queryEnd.AddMilliseconds(1);

//    // --- Corrected Query Logic: Appointment -> ClinicalRecord -> Invoice ---
//    var totalRevenue = await (
//        from appointment in _context.Appointments

//            // 1. Filter by Doctor and Time Range
//        where appointment.DoctorID == doctorId
//            && appointment.Date_Time >= queryStart
//            && appointment.Date_Time < queryEnd // Use '<' with added millisecond for cleaner range

//        // 2. Join Appointment to ClinicalRecord (Assumes ClinicalRecord has AppointmentID)
//        join clinicalRecord in _context.ClinicalRecords
//            on appointment.AppointmentID equals clinicalRecord.AppointmentID

//        // 3. Join ClinicalRecord to Invoice (Assumes Invoice has ClinicalRecordID)
//        join invoice in _context.Invoices
//            on clinicalRecord.ClinicalRecordID equals invoice.ClinicalRecordID

//        // 4. (Recommended) Filter to only include 'Paid' Invoices for true revenue
//        where invoice.Status == "Paid" // Use a more robust check like Enum if possible

//        // 5. Select the Amount and Sum
//        select invoice
//    )
//    .SumAsync(i => (double?)i.Amount) ?? 0.0;

//    return totalRevenue;
//}
