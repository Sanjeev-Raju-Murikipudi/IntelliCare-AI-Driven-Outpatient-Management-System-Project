using BCrypt.Net;
using Hangfire;
using Hangfire.MemoryStorage;
using IntelliCare.Application.DTOs;
using IntelliCare.Application.Interfaces;
using IntelliCare.Application.Models;
using IntelliCare.Domain;
using IntelliCare.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Security;



public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IPatientRepository _patientRepo;
    private readonly IDoctorRepository _doctorRepo;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _config;
    private readonly IEmailSender _emailSender;
    private readonly ISmsService _smsService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    // Replace the field declaration for _patientRepository with the correct type
    private readonly IPatientRepository _patientRepository;

    // Update the constructor to initialize _patientRepository
    public UserService(IUserRepository repo, IPatientRepository patientRepo, IHttpContextAccessor httpContextAccessor, IEmailSender emailSender, ISmsService smsService, IDoctorRepository doctorRepo, ILogger<UserService> logger, IConfiguration config)
    {
        _repo = repo;
        _patientRepo = patientRepo;
        _doctorRepo = doctorRepo;
        _logger = logger;
        _emailSender = emailSender;
        _smsService = smsService;
        _config = config;
        _httpContextAccessor = httpContextAccessor;
        _patientRepository = patientRepo; // Add this line
    }

    public string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim("username", user.Username),
        new Claim("role", user.RoleName.ToString())
    };

        // Add PatientID claim only if user is a patient
        if (user.RoleName == UserRole.Patient && user.PatientID.HasValue)
        {
            claims.Add(new Claim("PatientID", user.PatientID.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(3),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }



    public async Task RegisterAsync(RegisterUserDto dto)
    {
        if (await _repo.GetByUsernameAsync(dto.Username) != null)
            throw new InvalidOperationException("Username already exists.");

        if (await _repo.GetByEmailAsync(dto.ContactEmail) != null)
            throw new InvalidOperationException("Email already registered.");

        if (await _repo.GetByMobileAsync(dto.MobileNumber) != null)
            throw new InvalidOperationException("Mobile number already registered.");

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = Hash(dto.Password),
            RoleName = UserRole.Patient,
            MobileNumber = dto.MobileNumber,
            ContactEmail = dto.ContactEmail,
            DoctorID = null,
            PatientID = null,
            OTPCode = null,
            OTPExpiry = null
        };

        await _repo.CreateAsync(user);
        _logger.LogInformation("✅ Registered patient user: {Username} with ContactEmail: {Email}", user.Username, user.ContactEmail);
    }

    //public async Task CreateUserByAdminAsync(RegisterUserDto dto, UserRole roleToAssign, string requesterUsername)
    //{
    //    var requester = await _repo.GetByUsernameAsync(requesterUsername);
    //    if (requester == null || requester.RoleName != UserRole.Admin)
    //        throw new UnauthorizedAccessException("Only Admins can create new users.");

    //    if (roleToAssign == UserRole.Patient)
    //        throw new InvalidOperationException("Admins cannot create Patients directly.");

    //    if (await _repo.GetByUsernameAsync(dto.Username) != null)
    //        throw new InvalidOperationException("Username already exists.");

    //    var user = new User
    //    {
    //        Username = dto.Username,
    //        PasswordHash = Hash(dto.Password),
    //        RoleName = roleToAssign,
    //        MobileNumber = dto.MobileNumber,
    //        ContactEmail = dto.ContactEmail,
    //        DoctorID = null,
    //        PatientID = null,
    //        OTPCode = null,
    //        OTPExpiry = null
    //    };

    //    await _repo.CreateAsync(user);
    //    _logger.LogInformation("✅ Admin {Admin} created user {Username} with role {Role}", requester.Username, user.Username, roleToAssign);
    //}


    public async Task<User> GetUserByUsernameAsync(string username)
    {
        return await _repo.GetByUsernameAsync(username);
    }


    public async Task<ProfileCompletionResult> CompletePatientProfileAsync(CreatePatientDto dto)
    {
        var user = await _repo.GetByUsernameAsync(dto.Username);
        if (user == null || user.RoleName != UserRole.Patient)
            throw new InvalidOperationException("Invalid patient user.");

        if (user.PatientID != null)
            return new ProfileCompletionResult { IsNowComplete = false };

        var today = DateTime.Today;
        var age = today.Year - dto.DOB.Year;
        if (dto.DOB.Date > today.AddYears(-age)) age--;

        var patient = new Patient
        {
            UserName = user.Username,
            FullName = dto.FullName,
            DOB = dto.DOB,
            Age = age,
            Gender = dto.Gender,
            BloodGroup = dto.BloodGroup,
            PhoneNumber = user.MobileNumber,
            InsuranceDetails = dto.InsuranceDetails,
            MedicalHistory = dto.MedicalHistory,
            Address = dto.Address
        };

        await _patientRepo.CreateAsync(patient);

        user.PatientID = patient.PatientId;
        await _repo.UpdateAsync(user);

        _logger.LogInformation("✅ Patient profile completed for {Username}", user.Username);

        return new ProfileCompletionResult { IsNowComplete = true };
    }



    public async Task<object> CompleteDoctorProfileAsync(CompleteDoctorProfileDto dto)

    {

        // Step 1: Validate user and role

        var user = await _repo.GetByUsernameAsync(dto.Username);

        if (user == null || user.RoleName != UserRole.Doctor)

            throw new InvalidOperationException("Invalid doctor user.");

        // Step 2: Ensure doctor record is linked

        if (user.DoctorID == null)

            throw new InvalidOperationException("Doctor record not linked.");

        var doctor = await _doctorRepo.GetByIdAsync(user.DoctorID.Value);

        if (doctor == null)

            throw new InvalidOperationException("Doctor record not found.");

        // Step 3: Track changes and update only modified fields

        var updatedFields = new List<string>();

        if (doctor.Name != dto.Name)

        {

            doctor.Name = dto.Name;

            updatedFields.Add("Name");

        }

        if (doctor.Specialization != dto.Specialization)

        {

            doctor.Specialization = dto.Specialization;

            updatedFields.Add("Specialization");

        }

        if (doctor.Education != dto.Education)

        {

            doctor.Education = dto.Education;

            updatedFields.Add("Education");

        }

        if (doctor.Address != dto.Address)

        {

            doctor.Address = dto.Address;

            updatedFields.Add("Address");

        }

        if (doctor.ExperienceYears != dto.ExperienceYears)

        {

            doctor.ExperienceYears = dto.ExperienceYears;

            updatedFields.Add("ExperienceYears");

        }

        if (doctor.PhotoUrl != dto.PhotoUrl)

        {

            doctor.PhotoUrl = dto.PhotoUrl;

            updatedFields.Add("PhotoUrl");

        }

        // Optional: Track last update time (add this property to Doctor entity if needed)

        // doctor.LastUpdatedAt = DateTime.UtcNow;

        // Step 4: Save changes

        await _doctorRepo.UpdateAsync(doctor);

        _logger.LogInformation("✅ Doctor profile updated for {Username}. Fields changed: {Fields}",

            user.Username, string.Join(", ", updatedFields));

        // Step 5: Return structured response

        return new

        {

            message = "Profile updated successfully.",

            updatedFields

        };

    }



    //public async Task CompleteDoctorProfileAsync(CompleteDoctorProfileDto dto)

    //{


    //    // Step 1: Validate user and role
    //    var user = await _repo.GetByUsernameAsync(dto.Username);
    //    if (user == null || user.RoleName != UserRole.Doctor)
    //        throw new InvalidOperationException("Invalid doctor user.");

    //    // Step 2: Fetch linked doctor record
    //    if (user.DoctorID == null)
    //        throw new InvalidOperationException("Doctor record not linked.");

    //    var doctor = await _doctorRepo.GetByIdAsync(user.DoctorID.Value);
    //    if (doctor == null)
    //        throw new InvalidOperationException("Doctor record not found.");

    //    // Step 3: Check if profile is already completed
    //    if (!string.IsNullOrWhiteSpace(doctor.Name) &&
    //        doctor.Name != "Pending" &&
    //        !string.IsNullOrWhiteSpace(doctor.Specialization) &&
    //        doctor.Specialization != "Pending")
    //    {
    //        throw new InvalidOperationException("Doctor profile already completed.");
    //    }

    //    // Step 4: Update doctor profile
    //    doctor.Name = dto.Name;
    //    doctor.Specialization = dto.Specialization;
    //    await _doctorRepo.UpdateAsync(doctor);

    //    _logger.LogInformation("✅ Doctor profile completed for {Username}", user.Username);
    //}



    public async Task CreateDoctorAsync(CreateDoctorDto dto, string requesterUsername)

    {

        // Step 1: Check for duplicates

        if (await _repo.GetByUsernameAsync(dto.Username) != null)

            throw new InvalidOperationException("Username already exists.");

        if (await _repo.GetByEmailAsync(dto.ContactEmail) != null)

            throw new InvalidOperationException("Email already registered.");

        if (await _repo.GetByMobileAsync(dto.MobileNumber) != null)

            throw new InvalidOperationException("Mobile number already registered.");

        // Step 2: Validate requester

        var requester = await _repo.GetByUsernameAsync(requesterUsername);

        if (requester == null || requester.RoleName != UserRole.Admin)

            throw new UnauthorizedAccessException("Only Admins can create doctors.");

        // Step 3: Validate creation key

        var expectedKey = _config["Security:DoctorCreationKey"];

        if (dto.DoctorCreationKey != expectedKey)

            throw new InvalidOperationException("Invalid doctor creation key.");

        // Step 4: Create Doctor record with placeholders

        var doctor = new Doctor

        {

            Name = "Pending",

            Specialization = "Pending",

            Education = "",

            Address = "",

            ExperienceYears = 0,

            PhotoUrl = "",

            Appointments = new List<Appointment>()

        };

        await _doctorRepo.CreateAsync(doctor);

        // Step 5: Create linked User record

        var user = new User

        {

            Username = dto.Username,

            PasswordHash = Hash(dto.Password),

            RoleName = UserRole.Doctor,

            MobileNumber = dto.MobileNumber,

            ContactEmail = dto.ContactEmail,

            DoctorID = doctor.DoctorId,

            PatientID = null,

            OTPCode = null,

            OTPExpiry = null

        };

        await _repo.CreateAsync(user);

        _logger.LogInformation("✅ Doctor user registered: {Username}", user.Username);

    }



    //public async Task CreateDoctorAsync(CreateDoctorDto dto, string requesterUsername)
    //{

    //    if (await _repo.GetByUsernameAsync(dto.Username) != null)
    //        throw new InvalidOperationException("Username already exists.");

    //    if (await _repo.GetByEmailAsync(dto.ContactEmail) != null)
    //        throw new InvalidOperationException("Email already registered.");

    //    if (await _repo.GetByMobileAsync(dto.MobileNumber) != null)
    //        throw new InvalidOperationException("Mobile number already registered.");
    //    var requester = await _repo.GetByUsernameAsync(requesterUsername);

    //    if (requester == null || requester.RoleName != UserRole.Admin)
    //        throw new UnauthorizedAccessException("Only Admins can create doctors.");

    //    var expectedKey = _config["Security:DoctorCreationKey"];
    //    if (dto.DoctorCreationKey != expectedKey)
    //        throw new InvalidOperationException("Invalid doctor creation key.");


    //    // Create blank Doctor record (profile to be completed later)
    //    var doctor = new Doctor
    //    {
    //        Name = "Pending",
    //        Specialization = "Pending"
    //    };

    //    await _doctorRepo.CreateAsync(doctor);

    //    // Create linked User record
    //    var user = new User
    //    {
    //        Username = dto.Username,
    //        PasswordHash = Hash(dto.Password),
    //        RoleName = UserRole.Doctor,
    //        MobileNumber = dto.MobileNumber,
    //        ContactEmail = dto.ContactEmail,
    //        DoctorID = doctor.DoctorId,
    //        PatientID = null,
    //        OTPCode = null,
    //        OTPExpiry = null
    //    };

    //    await _repo.CreateAsync(user);
    //    _logger.LogInformation("✅ Doctor user registered: {Username}", user.Username);
    //}



    public async Task<string> CreateAdminAsync(CreateAdminDto dto, string requesterUsername)
    {
        if (await _repo.GetByUsernameAsync(dto.Username) != null)
            throw new InvalidOperationException("Username already exists.");

        if (await _repo.GetByEmailAsync(dto.ContactEmail) != null)
            throw new InvalidOperationException("Email already registered.");

        if (await _repo.GetByMobileAsync(dto.MobileNumber) != null)
            throw new InvalidOperationException("Mobile number already registered.");

        var requester = await _repo.GetByUsernameAsync(requesterUsername);
        if (requester == null || requester.RoleName != UserRole.Admin)
            throw new UnauthorizedAccessException("Only Admin can create Admins.");

        var expectedKey = _config["Security:AdminCreationKey"];
        if (dto.AdminCreationKey != expectedKey)
            throw new InvalidOperationException("Invalid Admin creation key.");

        var existing = await _repo.GetByUsernameAsync(dto.Username);
        if (existing != null)
            throw new InvalidOperationException("Username already exists.");

        var newAdmin = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleName = UserRole.Admin,
            MobileNumber = dto.MobileNumber,
            ContactEmail = dto.ContactEmail // ✅ Add this
        };


        await _repo.CreateAsync(newAdmin);
        _logger.LogInformation("✅ Admin created: {Username}", newAdmin.Username);
        _logger.LogInformation("Incoming DTO: {@dto}", dto);

        return newAdmin.Username;

    }

    public async Task<object> LoginAsync(LoginRequestDto dto)
    {
        var user = await _repo.GetByUsernameAsync(dto.Username);
        if (user == null)
        {
            _logger.LogWarning("Login failed for {Username}: user not found", dto.Username);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (user.LockoutEndTime.HasValue && user.LockoutEndTime > DateTime.UtcNow)
        {
            var remaining = user.LockoutEndTime.Value - DateTime.UtcNow;
            _logger.LogWarning("Login blocked for {Username}: locked out for {Minutes} more minutes", dto.Username, remaining.TotalMinutes);
            throw new InvalidOperationException($"Account is locked. Try again in {Math.Ceiling(remaining.TotalMinutes)} minutes.");
        }

        if (!Verify(dto.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= 3)
            {
                user.LockoutEndTime = DateTime.UtcNow.AddMinutes(5);
                _logger.LogWarning("User {Username} locked out due to 3 failed login attempts", dto.Username);
            }

            await _repo.UpdateAsync(user);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // ✅ Successful login
        user.FailedLoginAttempts = 0;
        user.LockoutEndTime = null;

        // 🔐 Generate OTP
        var otp = new Random().Next(100000, 999999).ToString();
        user.OTPCode = otp;
        user.OTPExpiry = DateTime.UtcNow.AddMinutes(5);

        await _repo.UpdateAsync(user);


        //// 📲 Send OTP via SMS



        //    await _smsService.SendOtpAsync(user.MobileNumber, $"Your IntelliCare login OTP is {otp}");



        try
        {
            BackgroundJob.Enqueue<IEmailSender>(email =>
                email.SendEmailAsync(user.ContactEmail, "IntelliCare Login OTP", $"Your OTP is: {otp}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue OTP email job");
        }

        _logger.LogInformation("✅ OTP email job enqueued for {Email}", user.ContactEmail);


        _logger.LogInformation("Login successful for {Username}", dto.Username);

        var token = GenerateJwtToken(user);

        return new
        {
            message = "Login successful. OTP has been sent to your registered email.",
            token,
            user = new UserDto
            {
                UserID = user.UserID,
                Username = user.Username,
                RoleName = user.RoleName,
                MobileNumber = user.MobileNumber,
                
            }
        };
    }


    public async Task<object> VerifyOtpAndGenerateTokenAsync(string username, string otpCode)

    {

        var user = await _repo.GetByUsernameAsync(username);

        if (user == null)

        {

            _logger.LogWarning("OTP verification failed: user not found {Username}", username);

            return null;

        }

        if (string.IsNullOrEmpty(user.OTPCode) || user.OTPCode != otpCode || user.OTPExpiry < DateTime.UtcNow)

        {

            _logger.LogWarning("OTP verification failed for {Username}: invalid or expired OTP", username);

            return null;

        }

        user.OTPCode = null;

        user.OTPExpiry = null;

        await _repo.UpdateAsync(user);

        _logger.LogInformation("✅ OTP verified for {Username}", username);

        var token = GenerateJwtToken(user);

        return new

        {

            message = "OTP verified successfully.",

            token,

            user = new UserDto

            {

                UserID = user.UserID,

                Username = user.Username,

                RoleName = user.RoleName,

                MobileNumber = user.MobileNumber

            }

        };

    }








    public async Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        var user = await _repo.GetByUsernameAsync(username);
        if (user == null)
        {
            _logger.LogWarning("User not found: {Username}", username);
            return false;
        }

        var isValid = Verify(password, user.PasswordHash);
        _logger.LogInformation("Credential validation for {Username}: {Result}", username, isValid ? "Success" : "Failure");
        return isValid;
    }

    //public async Task<string> GenerateOtpAsync(string username)
    //{
    //    var user = await _repo.GetByUsernameAsync(username);
    //    if (user == null)
    //    {
    //        _logger.LogWarning("OTP generation failed: user not found {Username}", username);
    //        return null;
    //    }

    //    var otp = new Random().Next(100000, 999999).ToString();
    //    user.OTPCode = otp;
    //    user.OTPExpiry = DateTime.UtcNow.AddMinutes(5);

    //    try
    //    {
    //        await _repo.UpdateAsync(user);

    //        // ✅ Send SMS
    //        if (!string.IsNullOrEmpty(user.MobileNumber))
    //        {
    //            await _smsService.SendOtpAsync(user.MobileNumber, $"Your IntelliCare login OTP is {otp}");
    //            _logger.LogInformation("OTP sent via SMS to {Mobile}", user.MobileNumber);
    //        }

    //        // ✅ Send Email via Hangfire
    //        if (!string.IsNullOrEmpty(user.ContactEmail))
    //        {
    //            BackgroundJob.Enqueue<IEmailSender>(email =>
    //                email.SendEmailAsync(user.ContactEmail, "IntelliCare Login OTP", $"Your OTP is: {otp}"));
    //            _logger.LogInformation("OTP email job enqueued for {Email}", user.ContactEmail);
    //        }

    //        return otp;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error generating OTP for {Username}", username);
    //        return null;
    //    }
    //}




    //public async Task<bool> VerifyOtpAsync(string username, string otpCode)
    //{
    //    var user = await _repo.GetByUsernameAsync(username);
    //    if (user == null)
    //    {
    //        _logger.LogWarning("OTP verification failed: user not found {Username}", username);
    //        return false;
    //    }

    //    if (string.IsNullOrEmpty(user.OTPCode) || user.OTPCode != otpCode || user.OTPExpiry < DateTime.UtcNow)
    //    {
    //        _logger.LogWarning("OTP verification failed for {Username}: invalid or expired OTP", username);
    //        return false;
    //    }

    //    user.OTPCode = null;
    //    user.OTPExpiry = null;

    //    try
    //    {
    //        await _repo.UpdateAsync(user);
    //        _logger.LogInformation("✅ OTP verified for {Username}", username);
    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error verifying OTP for {Username}", username);
    //        return false;
    //    }
    //}

    public int GetLoggedInPatientId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var username = user.Identity.Name;
        if (string.IsNullOrWhiteSpace(username))
            throw new UnauthorizedAccessException("Username claim not found.");

        var userEntity = _repo.GetByUsernameAsync(username).Result;
        if (userEntity == null || userEntity.PatientID == null)
            throw new InvalidOperationException("Please complete your profile before booking appointment.");

        return userEntity.PatientID.Value;
    }



    // No change needed to this method, as the field _patientRepository is now correctly typed
    public async Task<PatientPublicDto> GetPatientByUsernameAsync(string username)
    {
        var patient = await _patientRepository.GetByUsernameAsync(username);
        if (patient == null) return null;

        return new PatientPublicDto
        {
            PatientId = patient.PatientId,
            Name = patient.FullName,
            DOB = patient.DOB,
            Gender = patient.Gender.ToString(), // <-- FIX: Convert enum to string
            BloodGroup = patient.BloodGroup,
            PhoneNumber = patient.PhoneNumber,
            ContactEmail = null, // or assign from another source if available
            InsuranceDetails = patient.InsuranceDetails,
            MedicalHistory = patient.MedicalHistory,
            Address = patient.Address
        };
    }

    public async Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync()
{
    var doctorUsers = await _repo.GetAllDoctorUsersAsync();

    return doctorUsers
        .Where(tuple =>
            !string.IsNullOrWhiteSpace(tuple.doctor.Name) &&
            tuple.doctor.Name != "Pending" &&
            !string.IsNullOrWhiteSpace(tuple.doctor.Specialization) &&
            tuple.doctor.Specialization != "Pending"
        )
        .Select(tuple => new DoctorDto
        {
            DoctorId=tuple.doctor.DoctorId,
            Username = tuple.user.Username,
            
            Name = tuple.doctor.Name,
            Specialization = tuple.doctor.Specialization,
            Education = tuple.doctor.Education,
            Address = tuple.doctor.Address,
            ExperienceYears = tuple.doctor.ExperienceYears,
            PhotoUrl = tuple.doctor.PhotoUrl
        });
}


    private string Hash(string input) => BCrypt.Net.BCrypt.HashPassword(input);
    private bool Verify(string input, string hash) => BCrypt.Net.BCrypt.Verify(input, hash);
}
