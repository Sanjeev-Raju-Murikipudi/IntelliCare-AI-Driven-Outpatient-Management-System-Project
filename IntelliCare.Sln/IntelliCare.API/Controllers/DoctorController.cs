using System;
using System.Threading.Tasks;
using IntelliCare.Application.DTOs;
using IntelliCare.Application.Interfaces;
using IntelliCare.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IntelliCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<DoctorController> _logger;
        private readonly IDoctorRepository _repo;

        public DoctorController(
            IUserService userService,
            IAppointmentService appointmentService,
            ILogger<DoctorController> logger,
            IDoctorRepository repo)
        {
            _userService = userService;
            _appointmentService = appointmentService;
            _logger = logger;
            _repo = repo;
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost("complete-profile")]
        public async Task<IActionResult> CompleteProfile([FromBody] CompleteDoctorProfileDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var loggedInUsername = User.Identity?.Name;
            if (loggedInUsername != dto.Username)
                return Unauthorized(new { error = "Username mismatch." });

            try
            {
                await _userService.CompleteDoctorProfileAsync(dto);
                return Ok(new { message = "Doctor profile completed successfully." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Doctor profile completion failed");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during doctor profile completion");
                return StatusCode(500, new { error = "Internal server error.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all appointments assigned to the logged-in doctor.
        /// </summary>
        [Authorize(Roles = "Doctor,Admin")]
        [HttpGet("appointments/today")]
        public async Task<IActionResult> GetAppointments()
        {
            var username = User.Identity?.Name;
            var appointments = await _appointmentService.GetAppointmentsForDoctorAsync(username);
            return Ok(appointments);
        }

        /// <summary>
        /// Get patient details for a specific appointment.
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpGet("appointments/patientSummary")]
        public async Task<IActionResult> GetPatientSummary([FromQuery] int appointmentId)
        {
            try
            {
                var summary = await _appointmentService.GetPatientSummaryByAppointmentIdAsync(appointmentId);
                return Ok(summary);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching patient summary for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, new { message = "Unexpected error occurred. Please contact support." });
            }
        }



        /// <summary>
        /// Mark an appointment as completed.
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpPost("appointments/markAsComplete")]
        public async Task<IActionResult> CompleteAppointment(int appointmentId)
        {
            try
            {
                await _appointmentService.MarkAppointmentCompletedAsync(appointmentId);
                return Ok(new { message = "Appointment marked as completed." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("appointments/by-date")]
        public async Task<IActionResult> GetBookedAppointmentsByDate([FromQuery] DateTime date)
        {
            var username = User.Identity?.Name;
            var appointments = await _appointmentService.GetBookedAppointmentsForDoctorByDateAsync(username, date);
            return Ok(appointments);
        }



        //[Authorize(Roles = "Admin,Patient")]
        [HttpGet("all-doctors")]
        public async Task<IActionResult> GetAllDoctors()
        {
            try
            {
                var doctors = await _userService.GetAllDoctorsAsync();
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all doctors");
                return StatusCode(500, new { message = "Unexpected error occurred. Please contact support." });
            }
        }


   


        [Authorize(Roles = "Doctor")]
        [HttpPost("upload-photo")]
        public async Task<IActionResult> UploadPhoto(IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest(new { error = "No file uploaded." });

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "doctor");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{photo.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            // ✅ Return full URL
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var url = $"{baseUrl}/uploads/doctor/{fileName}";

            return Ok(new { url });
        }



        [HttpGet("profile")]

        public async Task<IActionResult> GetProfile([FromQuery] string username)

        {

            var profile = await _repo.GetByUsernameAsync(username);

            if (profile == null)

                return NotFound("Profile not found");

            return Ok(profile);

        }

        //[HttpGet("all")]

        //public async Task<IActionResult> GetAllDoctors()

        //{

        //    // Use repository method that returns strongly-typed Doctor entities.

        //    // Select only properties that exist on Doctor entity (DoctorId, Name, Specialization).

        //    var doctorsEnumerable = await _repo.GetAllDoctorsAsync();

        //    var doctors = doctorsEnumerable

        //        .Select(d => new

        //        {

        //            d.DoctorId,

        //            d.Name,

        //            d.Specialization,

        //            d.PhotoUrl

        //        })

        //        .ToList();

        //    return Ok(doctors);

        //}






        [HttpGet("{id}")]

        public async Task<IActionResult> GetDoctorById(int id)

        {

            if (id <= 0)

                return BadRequest(new { error = "Invalid doctor ID." });

            try

            {

                var doctor = await _repo.GetDoctorByIdAsync(id);

                if (doctor == null)

                    return NotFound(new { message = "Doctor not found." });

                // Return only necessary details

                var doctorDetails = new

                {

                    doctor.DoctorId,

                    doctor.Name,

                    doctor.Specialization,

                    doctor.ExperienceYears,

                    doctor.Education,

                    doctor.Address,

                    doctor.PhotoUrl

                };

                return Ok(doctorDetails);

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error fetching doctor details for ID {DoctorId}", id);

                return StatusCode(500, new { error = "Internal server error.", details = ex.Message });

            }

        }




    }
}
