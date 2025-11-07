using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using IntelliCare.Application.DTOs;
using IntelliCare.Application.Interfaces;
using IntelliCare.Domain;
using IntelliCare.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace IntelliCare.Application.Services;

/// <summary>
/// Implements the IPatientService interface, containing the business logic
/// for patient management.
/// </summary>
public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepo;
    private readonly ILogger<PatientService> _logger;
    private readonly IUserService _userService;



    public PatientService(IPatientRepository patientRepository, ILogger<PatientService> logger, IUserService userService, IUserRepository userRepo, IMapper mapper)
    {
        _patientRepository = patientRepository;
        _mapper = mapper;
        _logger = logger;
        _userRepo = userRepo;
        _userService = userService;
    }

    public async Task<IEnumerable<PatientPublicDto>> GetAllPatientsAsync()
    {
        var patients = await _patientRepository.GetAllAsync();
        var result = new List<PatientPublicDto>();

        foreach (var p in patients)
        {
            var user = await _userRepo.GetByUsernameAsync(p.UserName);

            result.Add(new PatientPublicDto
            {
                PatientId = p.PatientId,
                Name = p.FullName,
                ContactEmail = user?.ContactEmail ?? string.Empty,
                PhoneNumber = p.PhoneNumber,
                DOB = p.DOB,
                Age = p.Age,
                Gender = p.Gender.ToString(),
                BloodGroup = p.BloodGroup,
                InsuranceDetails = p.InsuranceDetails,
                MedicalHistory = p.MedicalHistory,
                Address = p.Address
            });
        }

        return result;
    }

    public async Task<PatientPublicDto> GetPatientByUsernameAsync(string username)
    {
        var patient = await _patientRepository.GetByUsernameAsync(username);
        if (patient == null) return null;
        var user = await _userRepo.GetByUsernameAsync(patient.UserName);


        return new PatientPublicDto
        {
            PatientId = patient.PatientId,
            Name = patient.FullName,
            DOB = patient.DOB,
            Gender = patient.Gender.ToString(),
            BloodGroup = patient.BloodGroup,
            PhoneNumber = patient.PhoneNumber,
            ContactEmail = user?.ContactEmail ?? string.Empty,
            InsuranceDetails = patient.InsuranceDetails,
            MedicalHistory = patient.MedicalHistory,
            Address = patient.Address
        };
    }
    public async Task<PatientDto> GetPatientByIdAsync(int patientId)
    {
        var patient = await _patientRepository.GetByIdAsync(patientId);
        if (patient == null) return null;

        var user = await _userRepo.GetByUsernameAsync(patient.UserName);

        return new PatientDto
        {
            PatientId = patient.PatientId,
            Name = patient.FullName,
            ContactEmail = user?.ContactEmail ?? string.Empty,
            PhoneNumber = patient.PhoneNumber,
            DOB = patient.DOB,
            Age = patient.Age,
            Gender = patient.Gender.ToString(),
            BloodGroup = patient.BloodGroup,
            InsuranceDetails = patient.InsuranceDetails,
            MedicalHistory = patient.MedicalHistory,
            Address = patient.Address,
           
        };
    }


    public async Task<PatientPublicDto?> GetPublicPatientByIdAsync(int patientId)
    {
        var patient = await _patientRepository.GetByIdAsync(patientId);
        if (patient == null) return null;

        var user = await _userRepo.GetByUsernameAsync(patient.UserName);

        return new PatientPublicDto
        {
            PatientId = patient.PatientId,
            Name = patient.FullName,
            ContactEmail = user?.ContactEmail ?? string.Empty,
            PhoneNumber = patient.PhoneNumber,
            DOB = patient.DOB,
            Age = patient.Age,
            Gender = patient.Gender.ToString(),
            BloodGroup = patient.BloodGroup,
            InsuranceDetails = patient.InsuranceDetails,
            MedicalHistory = patient.MedicalHistory,
            Address = patient.Address
        };
    }

    public async Task<PatientDto> AddPatientAsync(CreatePatientDto createPatientDto)
    {
        var patient = _mapper.Map<Patient>(createPatientDto);
        await _patientRepository.AddAsync(patient);
        // Note: In a real-world scenario, we would use a Unit of Work here to save changes.
        return _mapper.Map<PatientDto>(patient);
    }

  


    //}
    public async Task UpdatePatientAsync(int id, PatientUpdateDto dto)
    {
        var loggedInPatientId = _userService.GetLoggedInPatientId();
        if (id != loggedInPatientId)
            throw new UnauthorizedAccessException("You are not allowed to update another patient's profile.");

        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null)
            throw new InvalidOperationException("Patient not found.");

        // 🔄 Update only selected fields
        patient.FullName = dto.Name;
        patient.PhoneNumber = dto.PhoneNumber;
        patient.DOB = dto.DOB;
        patient.InsuranceDetails = dto.InsuranceDetails;
        patient.MedicalHistory = dto.MedicalHistory;
        patient.Address = dto.Address;

        // ✅ Recalculate Age from DOB
        var today = DateTime.Today;
        patient.Age = today.Year - dto.DOB.Year;
        if (dto.DOB.Date > today.AddYears(-patient.Age)) patient.Age--;

        await _patientRepository.UpdateAsync(patient);

        // 🔄 Update linked User entity
        var user = await _userRepo.GetByUsernameAsync(patient.UserName);
        if (user != null)
        {
            if (user.ContactEmail != dto.ContactEmail)
                user.ContactEmail = dto.ContactEmail;

            if (user.MobileNumber != dto.PhoneNumber)
                user.MobileNumber = dto.PhoneNumber;

            await _userRepo.UpdateAsync(user);
        }

        _logger.LogInformation("✅ Patient and linked User updated for {Username}", patient.UserName);
    }




    public async Task DeleteOwnPatientProfileAsync(string username)
    {
        var user = await _userRepo.GetByUsernameAsync(username);
        if (user == null || user.RoleName != UserRole.Patient || user.PatientID == null)
            throw new InvalidOperationException("Patient profile not found or already deleted.");

        var patient = await _patientRepository.GetByIdAsync(user.PatientID.Value);
        if (patient == null)
            throw new InvalidOperationException("Patient record not found.");

       

        // Delete patient first
        await _patientRepository.DeleteAsync(patient);

        // Then delete user
        await _userRepo.DeleteAsync(user);

        _logger.LogInformation("🗑️ Patient and user account deleted for {Username}", username);
    }


}
