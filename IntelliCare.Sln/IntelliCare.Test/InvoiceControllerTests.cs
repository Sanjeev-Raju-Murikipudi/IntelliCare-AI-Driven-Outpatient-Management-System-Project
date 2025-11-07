using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliCare.API.Controllers; // Adjust namespace if necessary
using IntelliCare.Application.DTOs;
using IntelliCare.Application.Interfaces;
using IntelliCare.Domain; // Assuming Invoice and ClinicalRecord are here

namespace IntelliCare.Test
{
    [TestFixture]
    public class InvoiceControllerTests
    {
        // Controller under test
        private  InvoiceController _controller;

        // Mocks for all dependencies
        private Mock<IInvoiceRepository> _mockInvoiceRepo;
        private Mock<IPdfGeneratorService> _mockPdfGenerator;
        private Mock<IClinicalRecordRepository> _mockClinicalRecordRepo;

        [SetUp]
        public void Setup()
        {
            // Initialize mocks
            _mockInvoiceRepo = new Mock<IInvoiceRepository>();
            _mockPdfGenerator = new Mock<IPdfGeneratorService>();
            _mockClinicalRecordRepo = new Mock<IClinicalRecordRepository>();

            // Initialize the Controller with the mocked dependencies
            _controller = new InvoiceController(
                _mockInvoiceRepo.Object,
                _mockPdfGenerator.Object,
                _mockClinicalRecordRepo.Object);
        }

        // --- Helper Methods for Test Data ---

        private Invoice GetTestInvoice(int id, int patientId, string claimStatus = "No claim")
        {
            return new Invoice
            {
                InvoiceID = id,
                PatientID = patientId,
                ClinicalRecordID = id * 10,
                Amount = 100.00m,
                InsuranceProvider = "TestProvider",
                Status = "Pending",
                ClaimStatus = claimStatus
            };
        }

        private CreateInvoiceDTO GetCreateInvoiceDto(decimal amount = 100.00m, string claimStatus = "No claim")
        {
            return new CreateInvoiceDTO
            {
                PatientID = 1,
                ClinicalRecordID = 10,
                Amount = amount,
                InsuranceProvider = "TestProvider",
                Status = "Due",
                ClaimStatus = claimStatus
            };
        }

        // --- 1. GET ALL (/api/Invoice) Tests ---

        [Test]
        public async Task GetAll_ReturnsOkWithAllInvoices()
        {
            // ARRANGE
            var invoices = new List<Invoice>
            {
                GetTestInvoice(1, 10),
                GetTestInvoice(2, 20)
            };
            _mockInvoiceRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(invoices);

            // ACT
            var result = await _controller.GetAll();

            // ASSERT
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var returnedInvoices = okResult.Value as IEnumerable<InvoiceDTO>;

            // Verify all items were returned and mapped to DTOs
            Assert.That(returnedInvoices.Count(), Is.EqualTo(2));
            Assert.That(returnedInvoices.First().InvoiceID, Is.EqualTo(1));
        }

        // --- 2. GET BY ID (/api/Invoice/{id}) Tests ---

        [Test]
        public async Task GetById_InvoiceExists_ReturnsOkWithInvoiceDTO()
        {
            // ARRANGE
            var invoice = GetTestInvoice(5, 1);
            _mockInvoiceRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(invoice);

            // ACT
            var result = await _controller.GetById(5);

            // ASSERT
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var returnedDto = okResult.Value as InvoiceDTO;

            Assert.That(returnedDto.InvoiceID, Is.EqualTo(5));
            Assert.That(returnedDto.Amount, Is.EqualTo(100.00m));
        }

        [Test]
        public async Task GetById_InvoiceNotFound_ReturnsNotFound()
        {
            // ARRANGE
            _mockInvoiceRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Invoice)null);

            // ACT
            var result = await _controller.GetById(999);

            // ASSERT
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        // --- 3. POST (Create) (/api/Invoice) Tests ---

        [Test]
        public async Task Create_ValidData_Returns201CreatedAndAppliesDiscount()
        {
            // ARRANGE
            var inputDto = GetCreateInvoiceDto(amount: 500.00m, claimStatus: "Approved");
            var createdInvoice = GetTestInvoice(10, inputDto.PatientID, "Approved");

            // Expected discounted amount: 500 * (1 - 0.20) = 400.00
            createdInvoice.Amount = 400.00m;

            // Mock the repository's creation call
            _mockInvoiceRepo.Setup(r => r.CreateAsync(It.IsAny<Invoice>()))
                            .ReturnsAsync(createdInvoice);

            // ACT
            var result = await _controller.Create(inputDto);

            // ASSERT
            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            var returnedDto = createdResult.Value as InvoiceDTO;

            // 1. Verify the correct HTTP status code
            Assert.That(createdResult.StatusCode, Is.EqualTo(StatusCodes.Status201Created));

            // 2. Verify the discount logic (Internal Controller Logic)
            Assert.That(returnedDto.Amount, Is.EqualTo(400.00m));

            // 3. Verify the repository was called with the final calculated amount
            _mockInvoiceRepo.Verify(r => r.CreateAsync(
                It.Is<Invoice>(i => i.Amount == 400.00m && i.ClaimStatus == "Approved")),
                Times.Once);
        }

        [Test]
        public async Task Create_InvalidModelState_Returns400BadRequest()
        {
            // ARRANGE
            var inputDto = GetCreateInvoiceDto();
            // Manually add a validation error to the ModelState
            _controller.ModelState.AddModelError("PatientID", "Required");

            // ACT
            var result = await _controller.Create(inputDto);

            // ASSERT
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            _mockInvoiceRepo.Verify(r => r.CreateAsync(It.IsAny<Invoice>()), Times.Never); // Should not hit the repo
        }

        // --- 4. GET BY STATUS (/api/Invoice/amount/{status}) Tests ---

        [Test]
        public async Task GetByStatus_ValidStatus_ReturnsOkWithInvoices()
        {
            // ARRANGE
            var invoices = new List<Invoice> { GetTestInvoice(1, 1) };
            _mockInvoiceRepo.Setup(r => r.GetByStatusAsync("Paid")).ReturnsAsync(invoices);

            // ACT
            var result = await _controller.GetByStatus("Paid");

            // ASSERT
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var returnedInvoices = okResult.Value as IEnumerable<InvoiceDTO>;
            Assert.That(returnedInvoices.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetByStatus_InvalidStatus_Returns400BadRequest()
        {
            // ACT
            var result = await _controller.GetByStatus("InvalidStatus");

            // ASSERT
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetByStatus_NoInvoicesFound_Returns404NotFound()
        {
            // ARRANGE
            _mockInvoiceRepo.Setup(r => r.GetByStatusAsync("Due")).ReturnsAsync(new List<Invoice>());

            // ACT
            var result = await _controller.GetByStatus("Due");

            // ASSERT
            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        }

        // --- 5. PDF Generation (/api/Invoice/{id}/pdf) Tests ---

        [Test]
        public async Task GeneratePdf_InvoiceExists_ReturnsFileResult()
        {
            // ARRANGE
            var invoice = GetTestInvoice(10, 1);
            var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // Fake PDF data

            _mockInvoiceRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(invoice);
            _mockPdfGenerator.Setup(p => p.GenerateInvoicePdfAsync(It.IsAny<Invoice>()))
                             .ReturnsAsync(pdfBytes);

            // ACT
            var result = await _controller.GeneratePdf(10);

            // ASSERT
            Assert.That(result, Is.InstanceOf<FileContentResult>());
            var fileResult = result as FileContentResult;

            Assert.That(fileResult.ContentType, Is.EqualTo("application/pdf"));
            Assert.That(fileResult.FileDownloadName, Is.EqualTo("IntelliCare_Invoice_10.pdf"));
            Assert.That(fileResult.FileContents.Length, Is.EqualTo(pdfBytes.Length));
        }

        [Test]
        public async Task GeneratePdf_InvoiceNotFound_Returns404NotFound()
        {
            // ARRANGE
            _mockInvoiceRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Invoice)null);

            // ACT
            var result = await _controller.GeneratePdf(999);

            // ASSERT
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GeneratePdf_GeneratorThrowsException_Returns500InternalServerError()
        {
            // ARRANGE
            var invoice = GetTestInvoice(11, 1);
            _mockInvoiceRepo.Setup(r => r.GetByIdAsync(11)).ReturnsAsync(invoice);

            // Mock the PDF service to throw an exception
            _mockPdfGenerator.Setup(p => p.GenerateInvoicePdfAsync(It.IsAny<Invoice>()))
                             .ThrowsAsync(new Exception("PDF library failed."));

            // ACT
            var result = await _controller.GeneratePdf(11);

            // ASSERT
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }
    }
}