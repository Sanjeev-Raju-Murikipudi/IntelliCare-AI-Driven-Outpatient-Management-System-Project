using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;

// Import your service, DTOs, interfaces, and domain models
using IntelliCare.Application.Services;
using IntelliCare.Application.Interfaces;
using IntelliCare.Application.DTOs;
using IntelliCare.Domain;
using IntelliCare.Domain.Enums;

namespace IntelliCare.Test
{
    [TestFixture]
    public class PatientServiceTests
    {
        // Service under test
        private PatientService _service;

        // Mocks for all dependencies
        private Mock<IPatientRepository> _mockPatientRepo;
        private Mock<IUserRepository> _mockUserRepo;
        private Mock<IMapper> _mockMapper;
        private Mock<ILogger<PatientService>> _mockLogger;
        private Mock<IUserService> _mockUserService; // ⭐️ NEW Dependency

        [SetUp]
        public void Setup()
        {
            // Initialize mocks
            _mockPatientRepo = new Mock<IPatientRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<PatientService>>();
            _mockUserService = new Mock<IUserService>(); // ⭐️ Initialize new mock

            // Instantiate the service with the mocked dependencies
            // ⭐️ Constructor signature has changed
            _service = new PatientService(
                _mockPatientRepo.Object,
                _mockLogger.Object,
                _mockUserService.Object, // ⭐️ Added IUserService
                _mockUserRepo.Object,
                _mockMapper.Object);
        }

        // --- Helper Methods to Create Test Data ---

        private Patient GetTestPatient(int id, string username, string phone)
        {
            return new Patient
            {
                PatientId = id,
                UserName = username,
                FullName = $"Test Patient {id}",
                PhoneNumber = phone,
                DOB = new DateTime(1990, 1, 1),
                InsuranceDetails = $"Insurance {id}",
                MedicalHistory = $"History {id}",
                Gender = Gender.Male,
                BloodGroup = "O+",
                Address = $"Address {id}"
            };
        }

        private User GetTestUser(int patientId, string username, string email, string phone)
        {
            return new User
            {
                UserID = patientId + 100,
                Username = username,
                PatientID = patientId,
                ContactEmail = email,
                MobileNumber = phone, // Added MobileNumber for Update test
                RoleName = UserRole.Patient
            };
        }

        // ⭐️ PatientPublicDto is now used by GetAll and GetPublicById
        private PatientPublicDto GetTestPatientPublicDto(int id, string email, string phone)
        {
            return new PatientPublicDto
            {
                PatientId = id,
                Name = $"Test Patient {id}",
                ContactEmail = email,
                PhoneNumber = phone,
                DOB = new DateTime(1990, 1, 1),
                InsuranceDetails = $"Insurance {id}",
                MedicalHistory = $"History {id}",
                Gender = Gender.Male.ToString()
            };
        }

        // ⭐️ PatientDto is now used by GetByIdAsync
        private PatientDto GetTestPatientDto(int id, string email, string phone)
        {
            return new PatientDto
            {
                PatientId = id,
                Name = $"Test Patient {id}",
                ContactEmail = email,
                PhoneNumber = phone,
                DOB = new DateTime(1990, 1, 1),
                InsuranceDetails = $"Insurance {id}",
                MedicalHistory = $"History {id}",
                Gender = Gender.Male.ToString()
            };
        }

        // ⭐️ PatientUpdateDto is used by UpdatePatientAsync
        private PatientUpdateDto GetTestPatientUpdateDto(string email, string phone)
        {
            return new PatientUpdateDto
            {
                Name = "Updated Name",
                ContactEmail = email,
                PhoneNumber = phone,
                DOB = new DateTime(1995, 5, 5),
                InsuranceDetails = "New Insurance",
                MedicalHistory = "New History",
                Address = "New Address"
            };
        }

        // --- 1. GetAllPatientsAsync Tests (Returns PatientPublicDto) ---

        [Test]
        public async Task GetAllPatientsAsync_ReturnsCombinedPatientAndUserPublicDtos()
        {
            // ARRANGE
            var patient1 = GetTestPatient(1, "user1", "111");
            var patient2 = GetTestPatient(2, "user2", "222");

            var user1 = GetTestUser(1, "user1", "user1@app.com", "111");
            var user2 = GetTestUser(2, "user2", "user2@app.com", "222");

            var patientList = new List<Patient> { patient1, patient2 };

            _mockPatientRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(patientList);
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("user1")).ReturnsAsync(user1);
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("user2")).ReturnsAsync(user2);

            // ACT
            var result = await _service.GetAllPatientsAsync();

            // ASSERT
            Assert.That(result.Count(), Is.EqualTo(2));

            var result1 = result.First(p => p.PatientId == 1);
            Assert.That(result1.Name, Is.EqualTo("Test Patient 1")); // Mapped from FullName
            Assert.That(result1.ContactEmail, Is.EqualTo("user1@app.com")); // Pulled from User

            var result2 = result.First(p => p.PatientId == 2);
            Assert.That(result2.ContactEmail, Is.EqualTo("user2@app.com"));
            Assert.That(result1, Is.TypeOf<PatientPublicDto>()); // ⭐️ Verify correct DTO type

            // Verify that the User repository was called for each patient
            _mockUserRepo.Verify(r => r.GetByUsernameAsync(It.IsAny<string>()), Times.Exactly(2));
            _mockMapper.Verify(m => m.Map<IEnumerable<PatientPublicDto>>(It.IsAny<IEnumerable<Patient>>()), Times.Never); // Verify manual mapping is used
        }

        [Test]
        public async Task GetAllPatientsAsync_WhenUserEntityIsMissing_ContactEmailIsEmptyString() // ⭐️ Changed from Null to Empty String
        {
            // ARRANGE
            var patient1 = GetTestPatient(1, "user1", "111");
            var patientList = new List<Patient> { patient1 };

            _mockPatientRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(patientList);
            // Simulate missing user record
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("user1")).ReturnsAsync((User)null);

            // ACT
            var result = await _service.GetAllPatientsAsync();

            // ASSERT
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().ContactEmail, Is.EqualTo(string.Empty)); // ⭐️ Assert against string.Empty
        }

        // --- 2. GetPatientByIdAsync Tests (Returns PatientDto) ---

        [Test]
        public async Task GetPatientByIdAsync_PatientExists_ReturnsCombinedPatientDto() // ⭐️ Renamed to match DTO type
        {
            // ARRANGE
            var patient = GetTestPatient(1, "user1", "111");
            var user = GetTestUser(1, "user1", "user1@app.com", "111");

            _mockPatientRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(patient);
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("user1")).ReturnsAsync(user);

            // ACT
            var result = await _service.GetPatientByIdAsync(1);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.PatientId, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("Test Patient 1"));
            Assert.That(result.ContactEmail, Is.EqualTo("user1@app.com"));
            Assert.That(result, Is.TypeOf<PatientDto>()); // ⭐️ Verify correct DTO type

            _mockUserRepo.Verify(r => r.GetByUsernameAsync("user1"), Times.Once);
            _mockMapper.Verify(m => m.Map<PatientDto>(It.IsAny<Patient>()), Times.Never); // Verify manual mapping is used
        }

        [Test]
        public async Task GetPatientByIdAsync_PatientDoesNotExist_ReturnsNull()
        {
            // ARRANGE
            _mockPatientRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Patient)null);

            // ACT
            var result = await _service.GetPatientByIdAsync(99);

            // ASSERT
            Assert.That(result, Is.Null);
            _mockUserRepo.Verify(r => r.GetByUsernameAsync(It.IsAny<string>()), Times.Never);
        }

        // --- 3. AddPatientAsync Tests ---

        [Test]
        public async Task AddPatientAsync_ValidDto_CallsRepositoryAndMapper()
        {
            // ARRANGE
            var createDto = new CreatePatientDto { FullName = "New Patient" };
            var patientEntity = new Patient { FullName = "New Patient" };
            var patientDto = new PatientDto { Name = "New Patient" };

            _mockMapper.Setup(m => m.Map<Patient>(createDto)).Returns(patientEntity);
            _mockMapper.Setup(m => m.Map<PatientDto>(patientEntity)).Returns(patientDto);
            _mockPatientRepo.Setup(r => r.AddAsync(patientEntity)).Returns(Task.CompletedTask);

            // ACT
            var result = await _service.AddPatientAsync(createDto);

            // ASSERT
            Assert.That(result, Is.EqualTo(patientDto));
            _mockMapper.Verify(m => m.Map<Patient>(createDto), Times.Once);
            _mockPatientRepo.Verify(r => r.AddAsync(patientEntity), Times.Once);
            _mockMapper.Verify(m => m.Map<PatientDto>(patientEntity), Times.Once);
        }

        // --- 4. UpdatePatientAsync Tests (Uses PatientUpdateDto and Authorization) ---

        [Test]
        public async Task UpdatePatientAsync_OwnPatientAndUserEmailChanged_UpdatesBothRepositories()
        {
            // ARRANGE
            int patientId = 1;
            var existingPatient = GetTestPatient(patientId, "user1", "111");
            var existingUser = GetTestUser(patientId, "user1", "old@email.com", "111");

            var updateDto = GetTestPatientUpdateDto("new@email.com", "999"); // ⭐️ PatientUpdateDto

            // ⭐️ Simulate current user is the patient being updated
            _mockUserService.Setup(s => s.GetLoggedInPatientId()).Returns(patientId);
            _mockPatientRepo.Setup(r => r.GetByIdAsync(patientId)).ReturnsAsync(existingPatient);
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("user1")).ReturnsAsync(existingUser);

            // ACT
            await _service.UpdatePatientAsync(patientId, updateDto);

            // ASSERT
            // 1. Verify Patient entity was updated with DTO properties and saved
            _mockPatientRepo.Verify(r => r.UpdateAsync(It.Is<Patient>(p =>
                 p.FullName == "Updated Name" &&
                 p.PhoneNumber == "999" &&
                 p.DOB == new DateTime(1995, 5, 5) &&
                 p.Age == DateTime.Today.Year - 1995)), Times.Once);

            // 2. Verify User entity was updated (email changed from old@email.com to new@email.com) and saved
            _mockUserRepo.Verify(r => r.UpdateAsync(It.Is<User>(u =>
                 u.ContactEmail == "new@email.com" &&
                 u.MobileNumber == "999")), Times.Once); // Also check MobileNumber update
        }



        [Test]
        public void UpdatePatientAsync_NotLoggedInPatient_ThrowsUnauthorizedAccessException() // ⭐️ NEW Test for Auth Check
        {
            // ARRANGE
            int patientIdToUpdate = 1;
            int loggedInPatientId = 2; // Different ID
            var updateDto = new PatientUpdateDto();

            // Simulate logged-in user trying to update a different patient
            _mockUserService.Setup(s => s.GetLoggedInPatientId()).Returns(loggedInPatientId);

            // ACT & ASSERT
            Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.UpdatePatientAsync(patientIdToUpdate, updateDto),
                "You are not allowed to update another patient's profile.");

            _mockPatientRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never); // Should fail before checking repo
        }

        [Test]
        public void UpdatePatientAsync_PatientNotFound_ThrowsInvalidOperationException()
        {
            // ARRANGE
            int patientId = 99;
            _mockUserService.Setup(s => s.GetLoggedInPatientId()).Returns(patientId); // Auth passes
            _mockPatientRepo.Setup(r => r.GetByIdAsync(patientId)).ReturnsAsync((Patient)null);
            var updateDto = new PatientUpdateDto();

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdatePatientAsync(patientId, updateDto), "Patient not found.");

            _mockPatientRepo.Verify(r => r.UpdateAsync(It.IsAny<Patient>()), Times.Never);
            _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        // --- 5. DeleteOwnPatientProfileAsync Tests ---

        [Test]
        public async Task DeleteOwnPatientProfileAsync_ValidPatientUser_DeletesPatientThenUser()
        {
            // ARRANGE
            string username = "testuser";
            var patient = GetTestPatient(1, username, "111");
            var user = GetTestUser(1, username, "email@test.com", "111");

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(username)).ReturnsAsync(user);
            _mockPatientRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(patient);

            // Setup Delete to track order (though Moq doesn't enforce sequence easily, we verify calls)
            // We just verify both deletes are called
            _mockPatientRepo.Setup(r => r.DeleteAsync(patient)).Returns(Task.CompletedTask);
            _mockUserRepo.Setup(r => r.DeleteAsync(user)).Returns(Task.CompletedTask);


            // ACT
            await _service.DeleteOwnPatientProfileAsync(username);

            // ASSERT
            // Verify patient is deleted first, then user (as per service implementation)
            _mockPatientRepo.Verify(r => r.DeleteAsync(patient), Times.Once);
            _mockUserRepo.Verify(r => r.DeleteAsync(user), Times.Once);
        }

        [Test]
        public void DeleteOwnPatientProfileAsync_UserNotFound_ThrowsInvalidOperationException()
        {
            // ARRANGE
            string username = "missinguser";
            _mockUserRepo.Setup(r => r.GetByUsernameAsync(username)).ReturnsAsync((User)null);

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.DeleteOwnPatientProfileAsync(username),
                "Patient profile not found or already deleted.");

            _mockPatientRepo.Verify(r => r.DeleteAsync(It.IsAny<Patient>()), Times.Never);
        }

        [Test]
        public void DeleteOwnPatientProfileAsync_PatientRecordNotFound_ThrowsInvalidOperationException() // ⭐️ Added test for missing Patient record
        {
            // ARRANGE
            string username = "testuser";
            var user = GetTestUser(1, username, "email@test.com", "111");

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(username)).ReturnsAsync(user);
            // Patient entity is null even though User has a PatientID
            _mockPatientRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Patient)null);

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.DeleteOwnPatientProfileAsync(username),
                "Patient record not found.");

            _mockPatientRepo.Verify(r => r.DeleteAsync(It.IsAny<Patient>()), Times.Never);
            _mockUserRepo.Verify(r => r.DeleteAsync(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void DeleteOwnPatientProfileAsync_UserIsNotPatientRole_ThrowsInvalidOperationException()
        {
            // ARRANGE
            string username = "doctoruser";
            var user = new User { Username = username, RoleName = UserRole.Doctor, PatientID = null };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(username)).ReturnsAsync(user);

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.DeleteOwnPatientProfileAsync(username),
                "Patient profile not found or already deleted.");

            _mockPatientRepo.Verify(r => r.DeleteAsync(It.IsAny<Patient>()), Times.Never);
        }

        // --- 6. GetPublicPatientByIdAsync Tests (NEW Method, Returns PatientPublicDto) ---

        [Test]
        public async Task GetPublicPatientByIdAsync_PatientExists_ReturnsPatientPublicDto()
        {
            // ARRANGE
            var patient = GetTestPatient(1, "user1", "111");
            var user = GetTestUser(1, "user1", "user1@app.com", "111");

            _mockPatientRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(patient);
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("user1")).ReturnsAsync(user);

            // ACT
            var result = await _service.GetPublicPatientByIdAsync(1);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.PatientId, Is.EqualTo(1));
            Assert.That(result.ContactEmail, Is.EqualTo("user1@app.com"));
            Assert.That(result, Is.TypeOf<PatientPublicDto>()); // ⭐️ Verify correct DTO type
        }

        [Test]
        public async Task GetPublicPatientByIdAsync_PatientDoesNotExist_ReturnsNull()
        {
            // ARRANGE
            _mockPatientRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Patient)null);

            // ACT
            var result = await _service.GetPublicPatientByIdAsync(99);

            // ASSERT
            Assert.That(result, Is.Null);
        }
    }
}
