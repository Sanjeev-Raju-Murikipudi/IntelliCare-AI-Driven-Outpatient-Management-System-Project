using IntelliCare.Application.Interfaces;
using IntelliCare.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 

[Authorize]
[Route("api/[controller]")] 
[ApiController]
public class ConsultationController : ControllerBase
{
    private readonly IConsultationService _consultationService;

  
    public ConsultationController(IConsultationService consultationService)
    {
      
        _consultationService = consultationService;
    }


    [HttpPost]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(PrescriptionDetailDto), 201)] 
    [ProducesResponseType(400)]
    public async Task<IActionResult> RecordConsultation([FromBody] RecordConsultationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
           
            var clinicalRecordId = await _consultationService.RecordNewConsultationAsync(dto);

          
            var prescriptionDetails = await _consultationService.GenerateEPrescriptionAsync(dto.AppointmentId);

        
            return StatusCode(201, prescriptionDetails);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

  
    [Authorize(Roles = "Patient,Admin")]
    [HttpGet("prescription/{appointmentId}")] 
                                             
    public async Task<IActionResult> GetEPrescription(int appointmentId)
    {
        try
        {
            var prescriptionDetails = await _consultationService.GenerateEPrescriptionAsync(appointmentId);

            
            return Ok(prescriptionDetails);
        }
        catch (Exception ex)
        {
         
            return NotFound(new { message = ex.Message });
        }
    }


  
    [Authorize(Roles = "Admin")]
    [HttpPatch("PrescriptionStatus/{clinicalRecordId}")]
    [ProducesResponseType(204)] 
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdatePrescriptionStatus(
        int clinicalRecordId,
        [FromBody] PrescriptionStatusUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _consultationService.UpdatePrescriptionStatusAsync(
                clinicalRecordId,
                dto.NewStatus,
                dto.DeliveryETA);

            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("{appointmentId}")]

    [Authorize(Roles = "Doctor,Admin")]

    [ProducesResponseType(typeof(PrescriptionDetailDto), 200)]

    [ProducesResponseType(404)]

    public async Task<IActionResult> GetPrescription(int appointmentId)

    {

        try

        {

            var prescription = await _consultationService.GenerateEPrescriptionAsync(appointmentId);

            return Ok(prescription);

        }

        catch (Exception ex)

        {

            // If no clinical record exists, return 404

            return NotFound(new { error = ex.Message });

        }

    }


    [Authorize(Roles = "Admin")]
    [HttpGet("AllPrescriptions")]
    [ProducesResponseType(typeof(List<PrescriptionDetailDto>), 200)]
    public async Task<IActionResult> GetAllPrescriptions()
    {
        try
        {
            var prescriptions = await _consultationService.GetAllPrescriptionsAsync();
            return Ok(prescriptions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

}