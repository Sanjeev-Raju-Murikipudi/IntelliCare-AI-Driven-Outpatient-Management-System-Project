using Microsoft.AspNetCore.Mvc;
using IntelliCare.Application.DTOs;
using IntelliCare.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly ISupportDataService _service;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(ISupportDataService service, ILogger<DocumentsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] DocumentUploadDto dto)
    {
        if (!ModelState.IsValid || dto.File == null || dto.File.Length == 0)
        {
            _logger.LogWarning("Upload failed: invalid model state or no file provided.");
            return BadRequest(ModelState);
        }

        try
        {
            // Read the file into a byte array
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await dto.File.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            var supportData = new SupportDataDto
            {
                DataType = "DOCUMENT",
                RefPatientID = dto.PatientId,
                FileName = dto.File.FileName,
                FileContent = fileBytes,
                Metrics = "N/A",
                PredictionDetails = "N/A"
            };

            var result = await _service.UploadDocumentAsync(supportData);
            _logger.LogInformation("Document uploaded for patient {PatientId}", dto.PatientId);
            return CreatedAtAction(nameof(GetDocuments), new { patientId = dto.PatientId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database insert failed.");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Database Error",
                Detail = "An error occurred while saving document metadata.",
                Status = 500
            });
        }
    }

    [HttpGet("{patientId}")]
    public async Task<IActionResult> GetDocuments(int patientId)
    {
        if (patientId <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Patient ID",
                Detail = "Patient ID must be a positive integer.",
                Status = 400
            });
        }

        var docs = await _service.GetDocumentsAsync(patientId);
        if (docs == null || !docs.Any())
        {
            return NotFound(new ProblemDetails
            {
                Title = "No Documents Found",
                Detail = $"No documents found for patient ID {patientId}.",
                Status = 404
            });
        }

        var count = docs.Count();
        _logger.LogInformation("Retrieved {Count} documents for patient {PatientId}", count, patientId);
        return Ok(docs);
    }
}