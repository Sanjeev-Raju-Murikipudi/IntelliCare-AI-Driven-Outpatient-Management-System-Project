using NUnit.Framework;
using System.Threading.Tasks;
using System;
using System.Linq;
using QuestPDF.Infrastructure; // REQUIRED for LicenseType

// Import the service and domain models
using IntelliCare.Infrastructure.Services;
using IntelliCare.Domain;

namespace IntelliCare.Tests.Infrastructure.Services
{
    [TestFixture]
    public class QuestPdfGeneratorServiceTests
    {
        // Service under test
        private QuestPdfGeneratorService _service;

        [SetUp]
        public void Setup()
        {
            // Instantiate the service directly as it has no dependencies to mock.
            _service = new QuestPdfGeneratorService();

            // 🛑 CRITICAL FIX: Set the QuestPDF License Type to Community to prevent 
            // the license enforcement exception (System.Exception).
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // --- Helper Methods to Create Test Data ---

        private Invoice GetTestInvoice(bool withPatient)
        {
            var invoice = new Invoice
            {
                InvoiceID = 12345,
                PatientID = 101,
                Amount = 150.75m,
                Status = "Paid",
                InsuranceProvider = "BlueCross",
                ClaimStatus = "Submitted",
                //DateCreated = DateTime.Now.AddDays(-5),
                //DateUpdated = DateTime.Now
            };

            if (withPatient)
            {
                // Patient data is required to fully test the display logic
                invoice.Patient = new Patient
                {
                    FullName = "Mr. Test Patient",
                    PhoneNumber = "999-555-1212"
                };
            }

            return invoice;
        }

        // --- Test Methods ---

        [Test]
        public async Task GenerateInvoicePdfAsync_ValidInvoiceData_ReturnsNonEmptyValidByteArray()
        {
            // ARRANGE
            var invoice = GetTestInvoice(withPatient: true);

            // ACT
            var pdfBytes = await _service.GenerateInvoicePdfAsync(invoice);

            // ASSERT
            // 1. Check that the output is not null and has a meaningful size
            Assert.That(pdfBytes, Is.Not.Null, "The generated PDF byte array should not be null.");
            Assert.That(pdfBytes.Length, Is.GreaterThan(100), "The generated PDF should have a substantial byte size.");

            // 2. Verify the byte array header matches the PDF magic number (%PDF-)
            Assert.That(pdfBytes.Take(5).ToArray(), Is.EqualTo(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }),
                "The byte array header must match the PDF magic number (%PDF-).");
        }

        [Test]
        public async Task GenerateInvoicePdfAsync_InvoiceMissingPatientData_DoesNotThrowAndReturnsValidPdf()
        {
            // ARRANGE
            // Patient property is intentionally null to test null-coalescing logic
            var invoice = GetTestInvoice(withPatient: false);

            // ACT
            var pdfBytes = await _service.GenerateInvoicePdfAsync(invoice);

            // ASSERT
            // The fact that this test completes without throwing a NullReferenceException 
            // confirms that the service successfully used the "N/A" fallbacks.

            // 1. Verify a byte array is still produced
            Assert.That(pdfBytes, Is.Not.Null);
            Assert.That(pdfBytes.Length, Is.GreaterThan(100));
        }
    }
}