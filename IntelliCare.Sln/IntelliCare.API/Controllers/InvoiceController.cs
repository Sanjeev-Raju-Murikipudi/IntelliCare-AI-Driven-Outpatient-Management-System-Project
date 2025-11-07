using IntelliCare.Application.DTOs;
using IntelliCare.Application.Interfaces;
using IntelliCare.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace IntelliCare.API.Controllers

{

    [Authorize]

    [ApiController]

    [Route("api/[controller]")]

    public class InvoiceController : ControllerBase

    {

        private readonly IInvoiceRepository _repository;

        private readonly IPdfGeneratorService _pdfGenerator;
        private readonly IClinicalRecordRepository _clinicalRecordRepository;

        public InvoiceController(IInvoiceRepository repository, IPdfGeneratorService pdfGenerator, IClinicalRecordRepository clinicalRecordRepository)

        {

            _repository = repository;

            _pdfGenerator = pdfGenerator;
            _clinicalRecordRepository = clinicalRecordRepository;

        }

        // Helper method 

        private decimal ApplyClaimStatusDiscount(decimal amount, string claimStatus)

        {

            if (string.IsNullOrWhiteSpace(claimStatus)) return amount;

            var normalized = claimStatus.Trim().ToLower();

            if (normalized == "approved")

            {

                const decimal discountRate = 0.20m;

                return amount - (amount * discountRate);

            }

            return amount;

        }


        [Authorize(Roles = "Admin")]

        [HttpGet]

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InvoiceDTO>))]

        [HttpGet]


        public async Task<ActionResult<IEnumerable<InvoiceDTO>>> GetAll()

        {

            var invoices = await _repository.GetAllAsync();

            return Ok(invoices.Select(i => new InvoiceDTO

            {

                InvoiceID = i.InvoiceID,

                PatientID = i.PatientID,

               ClinicalRecordID= i.ClinicalRecordID,

                Amount = i.Amount,

                InsuranceProvider = i.InsuranceProvider,

                Status = i.Status,

                ClaimStatus = i.ClaimStatus

            }));

        }



        [HttpGet("{id}")]

        [Authorize(Roles = "Admin,Patient")] 

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InvoiceDTO))]

        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<InvoiceDTO>> GetById(int id)

        {

            var invoice = await _repository.GetByIdAsync(id);

            if (invoice == null) return NotFound();

            return Ok(new InvoiceDTO

            {

                InvoiceID = invoice.InvoiceID,

                PatientID = invoice.PatientID,
                ClinicalRecordID = invoice.ClinicalRecordID,


                Amount = invoice.Amount,

                InsuranceProvider = invoice.InsuranceProvider,

                Status = invoice.Status,

                ClaimStatus = invoice.ClaimStatus

            });

        }


        [HttpPost]

        [Authorize(Roles = "Admin")]

        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(InvoiceDTO))]

        [ProducesResponseType(StatusCodes.Status400BadRequest)] 

        public async Task<ActionResult<InvoiceDTO>> Create([FromBody] CreateInvoiceDTO dto)

        {

            if (!ModelState.IsValid)

                return BadRequest(ModelState);


            var finalAmount = ApplyClaimStatusDiscount(dto.Amount, dto.ClaimStatus);

            var invoice = new Invoice

            {

                PatientID = dto.PatientID,

                ClinicalRecordID = dto.ClinicalRecordID,

                Amount = finalAmount,

                InsuranceProvider = dto.InsuranceProvider,

                Status = dto.Status,

                ClaimStatus = dto.ClaimStatus

            };

            var created = await _repository.CreateAsync(invoice);

            return CreatedAtAction(nameof(GetById), new { id = created.InvoiceID }, new InvoiceDTO

            {

                InvoiceID = created.InvoiceID,

                PatientID = created.PatientID,

                ClinicalRecordID = created.ClinicalRecordID,

                Amount = created.Amount,

                InsuranceProvider = created.InsuranceProvider,

                Status = created.Status,

                ClaimStatus = created.ClaimStatus

            });

        }

        //[HttpPut("{id}")]

        //[Authorize(Roles = "Admin")]

        //[ProducesResponseType(StatusCodes.Status204NoContent)]

        //[ProducesResponseType(StatusCodes.Status400BadRequest)]

        //[ProducesResponseType(StatusCodes.Status404NotFound)]

        //

        //public async Task<IActionResult> Update(int id, [FromBody] CreateInvoiceDTO dto)

        //{

        //    if (!ModelState.IsValid)

        //        return BadRequest(ModelState);

        //    var invoice = await _repository.GetByIdAsync(id);

        //    if (invoice == null) return NotFound();

        //    invoice.PatientID = dto.PatientID;

        //    invoice.Amount = dto.Amount; // Store original amount

        //    invoice.InsuranceProvider = dto.InsuranceProvider;

        //    invoice.Status = dto.Status;

        //    invoice.ClaimStatus = dto.ClaimStatus;

        //    await _repository.UpdateAsync(invoice);

        //    return NoContent();

        //}


        //[HttpDelete("{id}")]

        ////[Authorize(Roles = "Admin")]

        //[ProducesResponseType(StatusCodes.Status204NoContent)]

        //public async Task<IActionResult> Delete(int id)

        //{

        //    await _repository.DeleteAsync(id);

        //    return NoContent();

        //}

        [Authorize(Roles = "Admin")]

        [HttpGet("amount/{status}")]

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InvoiceDTO>))]

        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<IEnumerable<InvoiceDTO>>> GetByStatus(string status)

        {

            const string validStatusPattern = "^(Paid|Pending|Due)$";

            if (!Regex.IsMatch(status, validStatusPattern, RegexOptions.IgnoreCase))

                return BadRequest($"The Invoice Status '{status}' is invalid. Allowed values are: Paid, Pending, or Due.");

            var invoices = await _repository.GetByStatusAsync(status);

            if (!invoices.Any())

                return NotFound($"No invoices found with Status: {status}.");

            return Ok(invoices.Select(i => new InvoiceDTO

            {

                InvoiceID = i.InvoiceID,

                PatientID = i.PatientID,
                ClinicalRecordID = i.ClinicalRecordID,

                Amount = i.Amount,

                InsuranceProvider = i.InsuranceProvider,

                Status = i.Status,

                ClaimStatus = i.ClaimStatus

            }));

        }

        [Authorize(Roles = "Admin")]

        [HttpGet("claim/{claimStatus}")]

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InvoiceDTO>))]

        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<IEnumerable<InvoiceDTO>>> GetByClaimStatus(string claimStatus)

        {

            const string validClaimStatusPattern = "^(Approved|Pending|Denied|No claim)$";

            if (!Regex.IsMatch(claimStatus, validClaimStatusPattern, RegexOptions.IgnoreCase))

                return BadRequest($"The Claim Status '{claimStatus}' is invalid. Allowed values are: Approved, Pending, Denied, No claim");

            var invoices = await _repository.GetByClaimStatusAsync(claimStatus);

            if (!invoices.Any())

                return NotFound($"No invoices found with Claim Status: {claimStatus}.");

            return Ok(invoices.Select(i => new InvoiceDTO

            {

                InvoiceID = i.InvoiceID,

                PatientID = i.PatientID,
                ClinicalRecordID = i.ClinicalRecordID,

                Amount = i.Amount,

                InsuranceProvider = i.InsuranceProvider,

                Status = i.Status,

                ClaimStatus = i.ClaimStatus

            }));

        }


        [Authorize(Roles = "Admin,Patient")]

        [HttpGet("patient/{patientId}")]

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InvoiceDTO>))]

        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<IEnumerable<InvoiceDTO>>> GetByPatient(int patientId)

        {

            if (patientId <= 0)

                return BadRequest("Patient ID must be a positive integer.");

            var invoices = await _repository.GetByPatientIdAsync(patientId);

            return Ok(invoices.Select(i => new InvoiceDTO

            {

                InvoiceID = i.InvoiceID,

                PatientID = i.PatientID,

                ClinicalRecordID = i.ClinicalRecordID,

                Amount = i.Amount,

                InsuranceProvider = i.InsuranceProvider,

                Status = i.Status,

                ClaimStatus = i.ClaimStatus

            }));

        }



        [HttpGet("{id}/pdf")]

        [Authorize(Roles = "Admin,Patient")]

        [ProducesResponseType(StatusCodes.Status200OK)]

        [ProducesResponseType(StatusCodes.Status404NotFound)]

        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GeneratePdf(int id)

        {

            var invoice = await _repository.GetByIdAsync(id);

            if (invoice == null)

                return NotFound($"Invoice with ID {id} not found.");

            var discountedAmount =invoice.Amount;

            var pdfInvoiceModel = new Invoice

            {

                InvoiceID = invoice.InvoiceID,

                PatientID = invoice.PatientID,
                
                ClinicalRecordID = invoice.ClinicalRecordID,

                Amount = discountedAmount,

                InsuranceProvider = invoice.InsuranceProvider,

                Status = invoice.Status,

                ClaimStatus = invoice.ClaimStatus,

                Patient = invoice.Patient 

            };


            byte[] pdfBytes;

            try

            {

                pdfBytes = await _pdfGenerator.GenerateInvoicePdfAsync(pdfInvoiceModel);

            }

            catch

            {

                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during PDF generation.");

            }

            string fileName = $"IntelliCare_Invoice_{id}.pdf";

            return File(pdfBytes, "application/pdf", fileName);

        }



    }

}







