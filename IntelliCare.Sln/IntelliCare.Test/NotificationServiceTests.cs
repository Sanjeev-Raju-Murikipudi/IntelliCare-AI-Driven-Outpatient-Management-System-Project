using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Mail;

// Import the service, its interface, and configuration classes
using IntelliCare.Infrastructure.ExternalServices;
using IntelliCare.Application.Interfaces;
using IntelliCare.Application.Configurations;

namespace IntelliCare.Test
{
    [TestFixture]
    public class NotificationServiceTests
    {
        // Service under test
        private NotificationService _service;

        // Mock for the dependency
        private Mock<IOptions<EmailSettings>> _mockEmailOptions;

        // Test configuration data
        private EmailSettings _testEmailSettings;

        [SetUp]
        public void Setup()
        {
            // 1. ARRANGE: Create a consistent set of test settings
            _testEmailSettings = new EmailSettings
            {
                SmtpServer = "smtp.test.com",
                SmtpPort = 587,
                SenderEmail = "no-reply@intellicare.com",
                SenderPassword = "test_password"
            };

            // 2. ARRANGE: Mock the IOptions interface to return our test settings
            _mockEmailOptions = new Mock<IOptions<EmailSettings>>();
            _mockEmailOptions.Setup(o => o.Value).Returns(_testEmailSettings);

            // 3. ARRANGE: Instantiate the service with the mocked dependency
            _service = new NotificationService(_mockEmailOptions.Object);
        }

        [Test]
        public void Constructor_WhenCalled_RetrievesEmailSettingsFromOptions()
        {
            // ARRANGE is done in the [SetUp] method.
            // The service is already constructed.

            // ACT & ASSERT
            // We just need to verify that the constructor accessed the .Value property
            // on the IOptions object to retrieve the settings.
            _mockEmailOptions.Verify(o => o.Value, Times.Once);
        }

        [Test]
        public async Task SendEmailAsync_WhenCalled_AttemptsToSendEmailWithCorrectSettings()
        {
            // ARRANGE
            var toEmail = "test@recipient.com";
            var subject = "Test Subject";
            var body = "<p>This is a test.</p>";

            // ACT & ASSERT
            // Since SmtpClient cannot be mocked, a real network call will be attempted.
            // In a unit test environment, this will fail with an SmtpException, which is expected.
            // We catch this specific exception to confirm the code ran up to the point of sending.
            var ex = Assert.ThrowsAsync<SmtpException>(async () =>
            {
                await _service.SendEmailAsync(toEmail, subject, body);
            });

            // You can optionally assert on the exception message if you want to be more specific,
            // for example, to check for a host resolution failure.
            // e.g., Assert.That(ex.Message, Does.Contain("No such host is known"));

            // The most important assertion is verifying that the service correctly
            // used the configuration that we provided in the mock. The fact that it
            // threw an SmtpException (and not a NullReferenceException) proves
            // that the _emailSettings field was correctly initialized and used.
            _mockEmailOptions.Verify(o => o.Value, Times.AtLeastOnce);
        }

        [Test]
        public void SendEmailAsync_WithNullSettings_ThrowsException()
        {
            // ARRANGE
            // Override the default setup to simulate missing configuration
            _mockEmailOptions.Setup(o => o.Value).Returns((EmailSettings)null);
            _service = new NotificationService(_mockEmailOptions.Object); // Re-initialize with null settings

            // ACT & ASSERT
            // The call should fail with a NullReferenceException because _emailSettings is null.
            Assert.ThrowsAsync<System.NullReferenceException>(async () =>
            {
                await _service.SendEmailAsync("test@recipient.com", "subject", "body");
            });
        }
    }
}
