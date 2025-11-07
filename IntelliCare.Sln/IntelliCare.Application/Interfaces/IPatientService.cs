using IntelliCare.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliCare.Application.Interfaces;


public interface IPatientService
{
    Task<IEnumerable<PatientPublicDto>> GetAllPatientsAsync();
    Task<PatientDto> GetPatientByIdAsync(int id);
    Task<PatientPublicDto?> GetPublicPatientByIdAsync(int patientId);

    Task<PatientDto> AddPatientAsync(CreatePatientDto createPatientDto);
    Task UpdatePatientAsync(int id, PatientUpdateDto dto);
    Task DeleteOwnPatientProfileAsync(string username);
    Task<PatientPublicDto> GetPatientByUsernameAsync(string username);
}