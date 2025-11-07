using IntelliCare.Application.DTOs;
using IntelliCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class PatientController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IUserService _userService;
    private readonly ILogger<PatientController> _logger;

    public PatientController(IPatientService patientService, IUserService userService, ILogger<PatientController> logger)
    {
        _patientService = patientService;
        _userService = userService;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientPublicDto>>> GetAllPatients()
    {
        var patients = await _patientService.GetAllPatientsAsync();
        return Ok(patients);
    }


    [Authorize(Roles = "Patient,Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<PatientPublicDto>> GetPatientById(int id)
    {
        var patient = await _patientService.GetPublicPatientByIdAsync(id);
        if (patient == null)
            return NotFound(new { message = $"Patient with ID {id} not found." });

        return Ok(patient);
    }


    //[Authorize(Roles = "Admin, Doctor,Patient")]
    //[HttpPost]
    //public async Task<ActionResult<PatientDto>> AddPatient([FromBody] CreatePatientDto createPatientDto)
    //{
    //    if (!ModelState.IsValid)
    //        return BadRequest(ModelState);

    //    try
    //    {
    //        var newPatient = await _patientService.AddPatientAsync(createPatientDto);
    //        return CreatedAtAction(nameof(GetPatientById), new { id = newPatient.PatientId }, newPatient);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error creating patient.");
    //        return StatusCode(500, $"Error creating patient: {ex.Message}");
    //    }
    //}

    [Authorize(Roles = "Patient")]

    [HttpPut("{id}")]

    public async Task<IActionResult> UpdatePatient(int id, [FromBody] PatientUpdateDto patientDto)

    {

        if (!ModelState.IsValid)

            return BadRequest(ModelState);

        if (id != patientDto.PatientId)

            return BadRequest("Patient ID mismatch.");

        try

        {

            await _patientService.UpdatePatientAsync(id, patientDto);

            return Ok("✅ Patient profile updated successfully.");

        }

        catch (Exception ex)

        {

            _logger.LogError(ex, "Error updating patient.");

            return StatusCode(500, $"Error updating patient: {ex.Message}");

        }

    }
    [Authorize(Roles = "Patient,Admin")]
    [HttpGet("by-username/{username}")]
    public async Task<IActionResult> GetPatientByUsername(string username)
    {
        var patient = await _patientService.GetPatientByUsernameAsync(username);
        if (patient == null) return NotFound(new { message = "Patient not found" });
        return Ok(patient);
    }

    [Authorize(Roles = "Patient")]
    [HttpDelete("delete-self")]
    public async Task<IActionResult> DeleteOwnProfile()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized("Invalid user context.");

        try
        {
            await _patientService.DeleteOwnPatientProfileAsync(username);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Patient self-delete failed for {Username}", username);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during patient self-delete");
            return StatusCode(500, new { error = "Internal server error.", details = ex.Message });
        }
    }


    /// <summary>
    /// Completes patient profile and links to User.
    /// </summary>
    [Authorize(Roles = "Patient")]
    [HttpPost("complete-profile")]
    public async Task<IActionResult> CompleteProfile([FromBody] CreatePatientDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var loggedInUsername = User.Identity?.Name;
        if (loggedInUsername != dto.Username)
            return Unauthorized(new { error = "Username mismatch." });

        try
        {
            var result = await _userService.CompletePatientProfileAsync(dto);

            if (result.IsNowComplete)
            {
                var updatedUser = await _userService.GetUserByUsernameAsync(dto.Username);
                var token = _userService.GenerateJwtToken(updatedUser);

                return Ok(new
                {
                    message = "Patient profile completed successfully.",
                    token = token
                });
            }

            return Ok(new
            {
                message = "Patient profile already completed."
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Profile completion failed");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during profile completion");
            return StatusCode(500, new
            {
                error = "Internal server error.",
                details = ex.Message
            });
        }
    }


}
