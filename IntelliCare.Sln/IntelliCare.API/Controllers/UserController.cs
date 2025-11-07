using System;
using System.Threading.Tasks;
using IntelliCare.Application.DTOs;
using IntelliCare.Application.DTOs.Auth;
using IntelliCare.Application.Interfaces;
using IntelliCare.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace IntelliCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService service, ILogger<UserController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("register-patient")]
        public async Task<IActionResult> RegisterPatient([FromBody] RegisterUserDto dto)
        {
            //if (User.Identity?.IsAuthenticated == true)
            //    return StatusCode(StatusCodes.Status403Forbidden, new { error = "Logged-in users are not allowed to register new accounts." });

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                await _service.RegisterAsync(dto);
                return Ok(new { message = "Patient account created successfully." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Patient registration failed");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during patient registration");
                return StatusCode(500, new { error = "Internal server error.", details = ex.Message });
            }
        }

        [EnableRateLimiting("LoginPolicy")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            //if (User.Identity?.IsAuthenticated == true)
            //    return StatusCode(StatusCodes.Status403Forbidden, new { error = "Logged-in users are not allowed to Login new accounts." });

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var user = await _service.LoginAsync(dto);
                return Ok(user);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Login failed");
                return Unauthorized(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Login blocked or locked out");
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                return StatusCode(500, new { error = "Internal server error.", details = ex.Message });
            }
        }

        //[EnableRateLimiting("ResetPasswordPolicy")]
        //[AllowAnonymous]
        //[HttpPost("request-password-reset")]
        //public async Task<IActionResult> RequestPasswordReset([FromBody] ForgotPasswordDto dto)
        //{
        //    try
        //    {
        //        await _service.RequestPasswordResetAsync(dto.Email);
        //        return Ok(new { message = "OTP sent to your email." });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        _logger.LogWarning(ex, "Password reset request failed");
        //        return BadRequest(new { error = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unexpected error during password reset request");
        //        return StatusCode(500, new { error = "Internal server error.", details = ex.Message });
        //    }
        //}

        //[EnableRateLimiting("ResetPasswordPolicy")]
        //[AllowAnonymous]
        //[HttpPost("reset-password")]
        //public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        //{
        //    try
        //    {
        //        await _service.ResetPasswordAsync(dto.Username, dto.OTP, dto.NewPassword);
        //        return Ok(new { message = "Password reset successful." });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        _logger.LogWarning(ex, "Password reset failed");
        //        return BadRequest(new { error = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unexpected error during password reset");
        //        return StatusCode(500, new { error = "Internal server error.", details = ex.Message });
        //    }
        //}

        [Authorize]
        [HttpPost("create-doctor")]
        public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Doctor creation failed due to invalid model: {@ModelState}", ModelState);
                return ValidationProblem(ModelState);
            }

            var requesterUsername = User.Identity?.Name;

            try
            {
                await _service.CreateDoctorAsync(dto, requesterUsername);
                _logger.LogInformation("Doctor created successfully: {Username}", dto.Username);
                return StatusCode(StatusCodes.Status201Created, new { message = "Doctor created successfully.", username = dto.Username });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized doctor creation attempt by {Username}", requesterUsername);
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "Only Admins are allowed to create doctor accounts." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Doctor creation failed: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during doctor creation.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message });
            }
        }


        [HttpPost("verify-otp")]

        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyDto dto)

        {

            if (!ModelState.IsValid)

                return ValidationProblem(ModelState);

            var result = await _service.VerifyOtpAndGenerateTokenAsync(dto.Username, dto.OTPCode);

            if (result == null)

                return Unauthorized(new { error = "Invalid or expired OTP." });

            return Ok(result);

        }


        [Authorize]
        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var requesterUsername = User.Identity?.Name;

            try
            {
                var createdUsername = await _service.CreateAdminAsync(dto, requesterUsername);
                _logger.LogInformation("Admin created successfully: {Username}", createdUsername);
                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = "Admin created successfully.",
                    username = createdUsername
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized Admin creation attempt by {Username}", requesterUsername);
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "Only Admins are allowed to create Admin accounts." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Admin creation failed: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during admin creation.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
