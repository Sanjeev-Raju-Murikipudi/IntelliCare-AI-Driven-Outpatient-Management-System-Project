using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
// Removed using Hangfire;

// Import your service, DTOs, interfaces, and domain models
using IntelliCare.Application.Services; // Assuming the service is in this namespace
using IntelliCare.Application.Interfaces;
using IntelliCare.Application.DTOs;
using IntelliCare.Domain;
using IntelliCare.Domain.Enums;


namespace IntelliCare.Tests.Application.Services
{
    [TestFixture]
    public class UserServiceTests
    {
        // Service under test
        private UserService _service;

        // Mocks for all dependencies
        private Mock<IUserRepository> _mockUserRepo;
        private Mock<IPatientRepository> _mockPatientRepo;
        private Mock<IDoctorRepository> _mockDoctorRepo;
        private Mock<ILogger<UserService>> _mockLogger;
        private Mock<IConfiguration> _mockConfig;
        private Mock<IEmailSender> _mockEmailSender;
        private Mock<ISmsService> _mockSmsService;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        // Removed: private Mock<IBackgroundJobClient> _mockBackgroundJobClient;


        // Test data constants
        private const string TestUsername = "testuser";
        private const string TestPassword = "Password123";
        private const string TestEmail = "test@intellicare.com";
        private const string TestMobile = "1234567890";
        private const string AdminKey = "AdminSecretKey";
        private const string DoctorKey = "DoctorSecretKey";
        private const string JwtSecret = "thisisthetestsecretkeyforjwttokengeneration"; // Must be at least 32 bytes for HS256

        [SetUp]
        public void Setup()
        {
            // Initialize mocks
            _mockUserRepo = new Mock<IUserRepository>();
            _mockPatientRepo = new Mock<IPatientRepository>();
            _mockDoctorRepo = new Mock<IDoctorRepository>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _mockEmailSender = new Mock<IEmailSender>();
            _mockSmsService = new Mock<ISmsService>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockConfig = new Mock<IConfiguration>();
            // Removed: _mockBackgroundJobClient = new Mock<IBackgroundJobClient>();

            // Configure IConfiguration mock for JWT and Security Keys
            _mockConfig.Setup(c => c["Jwt:SecretKey"]).Returns(JwtSecret);
            _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("IntelliCareTestIssuer");
            _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("IntelliCareTestAudience");
            _mockConfig.Setup(c => c["Security:AdminCreationKey"]).Returns(AdminKey);
            _mockConfig.Setup(c => c["Security:DoctorCreationKey"]).Returns(DoctorKey);

            // Instantiate the service with the mocked dependencies
            _service = new UserService(
                _mockUserRepo.Object,
                _mockPatientRepo.Object,
                _mockHttpContextAccessor.Object,
                _mockEmailSender.Object,
                _mockSmsService.Object,
                _mockDoctorRepo.Object,
                _mockLogger.Object,
                _mockConfig.Object);

            // Removed complex static mocking setup
        }

        // Helper to create a user object
        private User GetTestUser(UserRole role, int? patientId = null, int? doctorId = null)
        {
            return new User
            {
                UserID = 1,
                Username = TestUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
                RoleName = role,
                MobileNumber = TestMobile,
                ContactEmail = TestEmail,
                FailedLoginAttempts = 0,
                LockoutEndTime = null,
                PatientID = patientId,
                DoctorID = doctorId
            };
        }

        // Helper to create an HttpContext with claims
        private void SetupHttpContext(string username, string role, string patientId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            if (patientId != null)
            {
                claims.Add(new Claim("PatientID", patientId));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            // Set up a mock HttpContext to ensure User property is not null
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.User).Returns(principal);
            httpContextMock.Setup(c => c.User.Identity.IsAuthenticated).Returns(true);

            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(httpContextMock.Object);
            // The service retrieves the username via user.Identity.Name which is derived from the ClaimsPrincipal
        }

        // --- GenerateJwtToken Tests ---

        [Test]
        public void GenerateJwtToken_PatientUser_ContainsPatientIDClaim()
        {
            // ARRANGE
            var patientUser = GetTestUser(UserRole.Patient, patientId: 42);

            // ACT
            var tokenString = _service.GenerateJwtToken(patientUser);
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(tokenString) as JwtSecurityToken;

            // ASSERT
            Assert.That(jsonToken, Is.Not.Null);
            Assert.That(jsonToken.Claims.Any(c => c.Type == "PatientID" && c.Value == "42"), Is.True);
            Assert.That(jsonToken.Claims.Any(c => c.Type == "role" && c.Value == "Patient"), Is.True);
        }

        [Test]
        public void GenerateJwtToken_AdminUser_DoesNotContainPatientIDClaim()
        {
            // ARRANGE
            var adminUser = GetTestUser(UserRole.Admin);

            // ACT
            var tokenString = _service.GenerateJwtToken(adminUser);
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(tokenString) as JwtSecurityToken;

            // ASSERT
            Assert.That(jsonToken, Is.Not.Null);
            Assert.That(jsonToken.Claims.Any(c => c.Type == "PatientID"), Is.False);
            Assert.That(jsonToken.Claims.Any(c => c.Type == "role" && c.Value == "Admin"), Is.True);
        }

        // --- RegisterAsync Tests (Patient Registration) ---


        [Test]
        public void RegisterAsync_ExistingUsername_ThrowsInvalidOperationException()
        {
            // ARRANGE
            var existingUser = GetTestUser(UserRole.Patient);
            var dto = new RegisterUserDto { Username = TestUsername, Password = TestPassword, ContactEmail = "new@test.com", MobileNumber = "9876543210" };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(dto.Username)).ReturnsAsync(existingUser);

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterAsync(dto), "Username already exists.");
            _mockUserRepo.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        // --- CompletePatientProfileAsync Tests ---

        [Test]
        public async Task CompletePatientProfileAsync_ValidUncompletedUser_CreatesPatientAndLinksToUser()
        {
            // ARRANGE
            var user = GetTestUser(UserRole.Patient, patientId: null);
            var dto = new CreatePatientDto
            {
                Username = TestUsername,
                FullName = "New Patient",
                DOB = new DateTime(2000, 1, 1),
                Gender = Gender.Female,
                BloodGroup = "A+",
                InsuranceDetails = "Ins1",
                MedicalHistory = "None",
                Address = "Test Address"
            };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(TestUsername)).ReturnsAsync(user);
            _mockPatientRepo.Setup(r => r.CreateAsync(It.IsAny<Patient>()))
                .Callback<Patient>(p => p.PatientId = 42) // Simulate ID generation
                .Returns(Task.CompletedTask)
                .Verifiable();

            // ACT
            var result = await _service.CompletePatientProfileAsync(dto);

            // ASSERT
            Assert.That(result.IsNowComplete, Is.True);
            _mockPatientRepo.Verify(r => r.CreateAsync(It.Is<Patient>(p =>
                p.FullName == dto.FullName &&
                p.UserName == TestUsername
                )), Times.Once);
            _mockUserRepo.Verify(r => r.UpdateAsync(It.Is<User>(u =>
                u.PatientID == 42
                )), Times.Once); // Verify link was created
        }

        [Test]
        public void CompletePatientProfileAsync_UserIsNotPatient_ThrowsInvalidOperationException()
        {
            // ARRANGE
            var user = GetTestUser(UserRole.Doctor, doctorId: 1);
            var dto = new CreatePatientDto { Username = TestUsername };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(TestUsername)).ReturnsAsync(user);

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(() => _service.CompletePatientProfileAsync(dto), "Invalid patient user.");
            _mockPatientRepo.Verify(r => r.CreateAsync(It.IsAny<Patient>()), Times.Never);
        }

        [Test]
        public async Task CompletePatientProfileAsync_ProfileAlreadyCompleted_ReturnsNotComplete()
        {
            // ARRANGE
            var user = GetTestUser(UserRole.Patient, patientId: 42);
            var dto = new CreatePatientDto { Username = TestUsername };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(TestUsername)).ReturnsAsync(user);

            // ACT
            var result = await _service.CompletePatientProfileAsync(dto);

            // ASSERT
            Assert.That(result.IsNowComplete, Is.False);
            _mockPatientRepo.Verify(r => r.CreateAsync(It.IsAny<Patient>()), Times.Never);
        }

        // --- CompleteDoctorProfileAsync Tests ---

        [Test]
        public async Task CompleteDoctorProfileAsync_ValidIncompleteDoctor_UpdatesDoctorProfile()
        {
            // ARRANGE
            var doctor = new Doctor { DoctorId = 1, Name = "Pending", Specialization = "Pending" };
            var user = GetTestUser(UserRole.Doctor, doctorId: 1);
            var dto = new CompleteDoctorProfileDto { Username = TestUsername, Name = "Dr. New Name", Specialization = "Cardiology" };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(TestUsername)).ReturnsAsync(user);
            _mockDoctorRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doctor);

            // ACT
            await _service.CompleteDoctorProfileAsync(dto);

            // ASSERT
            _mockDoctorRepo.Verify(r => r.UpdateAsync(It.Is<Doctor>(d =>
                d.Name == dto.Name && d.Specialization == dto.Specialization
                )), Times.Once);
        }

        [Test]
        public void CompleteDoctorProfileAsync_ProfileAlreadyCompleted_ThrowsInvalidOperationException()
        {
            // ARRANGE
            var doctor = new Doctor { DoctorId = 1, Name = "Dr. Complete", Specialization = "Dermatology" };
            var user = GetTestUser(UserRole.Doctor, doctorId: 1);
            var dto = new CompleteDoctorProfileDto { Username = TestUsername, Name = "Dr. New Name", Specialization = "Cardiology" };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(TestUsername)).ReturnsAsync(user);
            _mockDoctorRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doctor);

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(() => _service.CompleteDoctorProfileAsync(dto), "Doctor profile already completed.");
            _mockDoctorRepo.Verify(r => r.UpdateAsync(It.IsAny<Doctor>()), Times.Never);
        }

        // --- CreateDoctorAsync Tests ---

        [Test]
        public async Task CreateDoctorAsync_ValidAdminAndKey_CreatesDoctorAndUser()
        {
            // ARRANGE
            var adminUser = GetTestUser(UserRole.Admin);
            var dto = new CreateDoctorDto { Username = "newdoc", Password = TestPassword, ContactEmail = "doc@test.com", MobileNumber = "1234567891", DoctorCreationKey = DoctorKey };
            var newDoctor = new Doctor { DoctorId = 50 }; // Expected ID

            _mockUserRepo.Setup(r => r.GetByUsernameAsync("newdoc")).ReturnsAsync((User)null);
            _mockUserRepo.Setup(r => r.GetByUsernameAsync(TestUsername)).ReturnsAsync(adminUser); // Requester
            _mockUserRepo.Setup(r => r.GetByEmailAsync(dto.ContactEmail)).ReturnsAsync((User)null);
            _mockUserRepo.Setup(r => r.GetByMobileAsync(dto.MobileNumber)).ReturnsAsync((User)null);
            _mockDoctorRepo.Setup(r => r.CreateAsync(It.IsAny<Doctor>()))
                .Callback<Doctor>(d => d.DoctorId = newDoctor.DoctorId)
                .Returns(Task.CompletedTask);

            // ACT
            await _service.CreateDoctorAsync(dto, TestUsername);

            // ASSERT
            _mockDoctorRepo.Verify(r => r.CreateAsync(It.Is<Doctor>(d =>
                d.Name == "Pending" && d.Specialization == "Pending"
                )), Times.Once);
            _mockUserRepo.Verify(r => r.CreateAsync(It.Is<User>(u =>
                u.Username == dto.Username &&
                u.RoleName == UserRole.Doctor &&
                u.DoctorID == newDoctor.DoctorId
                )), Times.Once);
        }

        [Test]
        public void CreateDoctorAsync_InvalidCreationKey_ThrowsInvalidOperationException()
        {
            // ARRANGE
            var adminUser = GetTestUser(UserRole.Admin);
            var dto = new CreateDoctorDto { Username = "newdoc", Password = TestPassword, DoctorCreationKey = "WrongKey" };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(TestUsername)).ReturnsAsync(adminUser); // Requester

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateDoctorAsync(dto, TestUsername), "Invalid doctor creation key.");
            _mockDoctorRepo.Verify(r => r.CreateAsync(It.IsAny<Doctor>()), Times.Never);
            _mockUserRepo.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void CreateDoctorAsync_RequesterIsNotAdmin_ThrowsUnauthorizedAccessException()
        {
            // ARRANGE
            var patientUser = GetTestUser(UserRole.Patient);
            var dto = new CreateDoctorDto { Username = "newdoc", Password = TestPassword, DoctorCreationKey = DoctorKey };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(TestUsername)).ReturnsAsync(patientUser); // Requester

            // ACT & ASSERT
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CreateDoctorAsync(dto, TestUsername), "Only Admins can create doctors.");
            _mockDoctorRepo.Verify(r => r.CreateAsync(It.IsAny<Doctor>()), Times.Never);
            _mockUserRepo.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        // --- CreateAdminAsync Tests ---

        [Test]
        public async Task CreateAdminAsync_ValidAdminAndKey_CreatesAdminUser()
        {
            // ARRANGE
            var adminUser = GetTestUser(UserRole.Admin);
            var dto = new CreateAdminDto { Username = "newadmin", Password = TestPassword, ContactEmail = "admin@test.com", MobileNumber = "1234567891", AdminCreationKey = AdminKey };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync("newadmin")).ReturnsAsync((User)null);
            _mockUserRepo.Setup(r => r.GetByUsernameAsync(TestUsername)).ReturnsAsync(adminUser); // Requester
            _mockUserRepo.Setup(r => r.GetByEmailAsync(dto.ContactEmail)).ReturnsAsync((User)null);
            _mockUserRepo.Setup(r => r.GetByMobileAsync(dto.MobileNumber)).ReturnsAsync((User)null);

            // ACT
            await _service.CreateAdminAsync(dto, TestUsername);

            // ASSERT
            _mockUserRepo.Verify(r => r.CreateAsync(It.Is<User>(u =>
                u.Username == dto.Username &&
                u.RoleName == UserRole.Admin &&
                u.ContactEmail == dto.ContactEmail
                )), Times.Once);
        }

        [Test]
        public void CreateAdminAsync_InvalidAdminKey_ThrowsInvalidOperationException()
        {
            // ARRANGE
            var adminUser = GetTestUser(UserRole.Admin);
            var dto = new CreateAdminDto { Username = "newadmin", Password = TestPassword, AdminCreationKey = "WrongKey" };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(TestUsername)).ReturnsAsync(adminUser); // Requester

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAdminAsync(dto, TestUsername), "Invalid Admin creation key.");
            _mockUserRepo.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        // --- LoginAsync Tests ---



        [Test]
        public void LoginAsync_InvalidPassword_IncrementsFailedAttemptsAndThrowsUnauthorized()
        {
            // ARRANGE
            var user = GetTestUser(UserRole.Patient);
            var loginDto = new LoginRequestDto { Username = TestUsername, Password = "WrongPassword" };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(TestUsername)).ReturnsAsync(user);

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LoginAsync(loginDto), "Invalid credentials.");

            // ASSERT
            _mockUserRepo.Verify(r => r.UpdateAsync(It.Is<User>(u =>
                u.FailedLoginAttempts == 1 && u.LockoutEndTime == null)), Times.Once);
            _mockSmsService.Verify(s => s.SendOtpAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void LoginAsync_ThirdFailedAttempt_LocksOutUser()
        {
            // ARRANGE
            var user = GetTestUser(UserRole.Patient);
            user.FailedLoginAttempts = 2; // Two previous failures
            var loginDto = new LoginRequestDto { Username = TestUsername, Password = "WrongPassword" };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(TestUsername)).ReturnsAsync(user);

            // ACT & ASSERT
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LoginAsync(loginDto), "Invalid credentials.");

            // ASSERT
            _mockUserRepo.Verify(r => r.UpdateAsync(It.Is<User>(u =>
                u.FailedLoginAttempts == 3 && u.LockoutEndTime.HasValue)), Times.Once);
            // Note: We don't check the exact LockoutEndTime because of minor time differences, 
            // but we ensure it's a value (HasValue)
        }

        [Test]
        public void LoginAsync_UserIsLockedOut_ThrowsInvalidOperationException()
        {
            // ARRANGE
            var user = GetTestUser(UserRole.Patient);
            user.LockoutEndTime = DateTime.UtcNow.AddMinutes(5); // Locked out
            var loginDto = new LoginRequestDto { Username = TestUsername, Password = TestPassword };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(TestUsername)).ReturnsAsync(user);

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(() => _service.LoginAsync(loginDto), "Account is locked.");
            _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        // --- VerifyOtpAsync Tests ---

        [Test]
        public async Task VerifyOtpAsync_ValidOtp_ClearsOtpAndReturnsTrue()
        {
            // ARRANGE
            var user = GetTestUser(UserRole.Patient);
            user.OTPCode = "123456";
            user.OTPExpiry = DateTime.UtcNow.AddMinutes(5);

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(TestUsername)).ReturnsAsync(user);

            // ACT
            var result = await _service.VerifyOtpAndGenerateTokenAsync(TestUsername, "123456");

            // ASSERT
            Assert.That(result, Is.True);
            _mockUserRepo.Verify(r => r.UpdateAsync(It.Is<User>(u =>
                u.OTPCode == null && u.OTPExpiry == null)), Times.Once);
        }

        [Test]
        public async Task VerifyOtpAsync_ExpiredOtp_ReturnsFalse()
        {
            // ARRANGE
            var user = GetTestUser(UserRole.Patient);
            user.OTPCode = "123456";
            user.OTPExpiry = DateTime.UtcNow.AddMinutes(-1); // Expired

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(TestUsername)).ReturnsAsync(user);

            // ACT
            var result = await _service.VerifyOtpAndGenerateTokenAsync(TestUsername, "123456");

            // ASSERT
            Assert.That(result, Is.False);
            _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        // --- GetLoggedInPatientId Tests ---


        [Test]
        public void GetLoggedInPatientId_UnauthenticatedUser_ThrowsUnauthorizedAccessException()
        {
            // ARRANGE
            // Set up a mock HttpContext where IsAuthenticated is false
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.User.Identity.IsAuthenticated).Returns(false);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

            // ACT & ASSERT
            Assert.Throws<UnauthorizedAccessException>(() => _service.GetLoggedInPatientId(), "User is not authenticated.");
        }


    }
}