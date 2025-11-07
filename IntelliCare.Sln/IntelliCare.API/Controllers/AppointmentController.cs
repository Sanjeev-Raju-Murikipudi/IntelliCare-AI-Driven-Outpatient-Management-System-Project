using IntelliCare.Application.DTOs;
using IntelliCare.Application.Interfaces;
using IntelliCare.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliCare.API.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppointmentService _service;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(AppointmentService service, ILogger<AppointmentController> logger)
        {
            _service = service;
            _logger = logger;

        }

        [Authorize(Roles = "Admin")]

        [HttpPost("createDoctorSlot")]

        public async Task<IActionResult> CreateSlot([FromBody] CreateSlotDto dto)

        {

            try

            {

                var result = await _service.CreateSlotsAsync(dto);

                return Ok(new { message = result });

            }

            catch (ArgumentException ex)

            {

                return BadRequest(new { error = ex.Message });

            }

            catch (UnauthorizedAccessException ex)

            {

                return Unauthorized(new { error = ex.Message });

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Slot creation failed.");

                return StatusCode(500, new { error = "Internal server error." });

            }

        }


        // 👤 Patient-only: Book appointment
        [Authorize(Roles = "Patient")]
        [HttpPost("bookAppointment")]
        public async Task<IActionResult> Book([FromBody] BookAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid appointment data." });

            try
            {
                var result = await _service.BookAsync(dto);
                return Ok(new
                {
                    message = result.Message,
                    fee = result.Fee,
                    paymentRequired = result.PaymentRequired,
                    appointmentID = result.AppointmentID
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = "Invalid input: " + ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message =  ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error occurred. Please contact support." });
            }
        }

        // 👤 Patient-only: Reschedule appointment
        [Authorize(Roles = "Patient")]
        [HttpPost("rescheduleAppointment")]
        public async Task<IActionResult> Reschedule([FromBody] RescheduleAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid reschedule request." });

            try
            {
                await _service.RescheduleAsync(dto);
                return Ok("Appointment rescheduled");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = "Invalid input: " + ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message =  ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error occurred. Please contact support." });
            }
        }

        // 👤 Patient or Doctor: Cancel appointment
        [Authorize(Roles = "Patient,Doctor")]
        [HttpPost("cancelAppointment")]
        public async Task<IActionResult> Cancel([FromBody] CancelAppointmentDto dto)
        {
            try
            {
                await _service.CancelAsync(dto);
                return Ok("Appointment cancelled");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message =  ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message =  ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error occurred. Please contact support." });
            }
        }

        // 🧑‍⚕️ Doctor or Admin: Mark appointment as completed
        //[Authorize(Roles = "Doctor,Admin")]
        //[HttpPost("AppointmentsCompleted")]
        //public async Task<IActionResult> Complete([FromBody] int appointmentId)
        //{
        //    try
        //    {
        //        await _service.CompleteAppointmentAsync(appointmentId);
        //        return Ok("Appointment marked as completed.");
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new { message = "Invalid input: " + ex.Message });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return Conflict(new { message = "Completion conflict: " + ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Unexpected error occurred. Please contact support." });
        //    }
        //}

        // 👤 Patient-only: Get doctor availability
        [Authorize(Roles = "Patient,Admin")]

        [HttpGet("DoctorAvailability")]

        public async Task<IActionResult> GetAvailability([FromQuery] int doctorId)

        {

            try

            {

                var slots = await _service.GetDoctorAvailabilityAsync(doctorId);

                return Ok(slots);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { message = "Unable to fetch availability. Please try again later." });

            }

        }


        // 👤 Patient-only: Get doctor queue
        [Authorize(Roles = "Patient")]
        [HttpGet("queue/{doctorId}")]
        public async Task<IActionResult> GetQueue(int doctorId, [FromQuery] DateTime date)
        {
            if (doctorId <= 0)
                return BadRequest(new { message = "Invalid doctor ID. Please provide a valid doctor." });

            if (date.Date < DateTime.Today)
                return BadRequest(new { message = "Date cannot be in the past. Please select today or a future date." });

            try
            {
                var queue = await _service.GetQueueAsync(doctorId, date);

                if (queue == null || !queue.Any())
                    return NotFound(new { message = "No appointments found for the selected doctor and date." });

                return Ok(queue);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Optional: log ex.Message for diagnostics
                return StatusCode(500, new { message = "Something went wrong while fetching the queue. Please try again later." });
            }
        }


        // 🧪 Optional: Debug role claim
        //[Authorize]
        //[HttpGet("whoami")]
        //public IActionResult WhoAmI()
        //{
        //    var claims = User.Claims.Select(c => new { c.Type, c.Value });
        //    return Ok(claims);
        //}




        //[Authorize(Roles = "Doctor")]
        //[HttpGet("appointments/today")]
        //public async Task<IActionResult> GetAppointments()
        //{
        //    var username = User.Identity?.Name;
        //    var appointments = await _service.GetAppointmentsForDoctorAsync(username);
        //    return Ok(appointments);
        //}


        [Authorize(Roles = "Admin")]
        [HttpGet("appointments/today/all")]
        public async Task<IActionResult> GetAllDoctorsAppointmentsToday()
        {
            var appointments = await _service.GetAllAppointmentsForTodayAsync();
            return Ok(appointments);
        }



        [Authorize(Roles = "Admin")]
        [HttpGet("appointments/all")]
        public async Task<IActionResult> GetAllAppointments()
        {
            var appointments = await _service.GetAllAppointmentsAsync();
            return Ok(appointments);
        }


        [Authorize(Roles = "Patient")]

        [HttpGet("myAppointments")]

        public async Task<IActionResult> GetMyAppointments()

        {

            try

            {

                // Extract PatientID from JWT claims

                var patientIdClaim = User.Claims.FirstOrDefault(c => c.Type == "PatientID");

                if (patientIdClaim == null)

                {

                    return Unauthorized(new { message = "Patient ID not found in token." });

                }

                if (!int.TryParse(patientIdClaim.Value, out int patientId))

                {

                    return BadRequest(new { message = "Invalid Patient ID format in token." });

                }

                // Fetch appointments for this patient only

                var appointments = await _service.GetAppointmentsByPatientIdAsync(patientId);

                if (appointments == null || !appointments.Any())

                {

                    return Ok(new List<object>()); // Return empty list instead of 404 for better UX

                }

                return Ok(appointments);

            }

            catch (Exception ex)

            {

                // Log exception (optional)

                return StatusCode(StatusCodes.Status500InternalServerError,

                    new { message = "Unexpected error occurred. Please contact support." });

            }

        }


    }
}
