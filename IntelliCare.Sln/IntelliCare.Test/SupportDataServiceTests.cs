using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IntelliCare.Application.DTOs;
using IntelliCare.Application.Interfaces;
using IntelliCare.Domain;

namespace IntelliCare.Tests.Application.Services
{
    [TestFixture]
    public class SupportDataServiceTests
    {
        private ISupportDataService _service;
        private Mock<ISupportDataRepository> _mockRepository;
        private Mock<IMapper> _mockMapper;

        private int _testPatientId = 10;
        private SupportDataDto _testDto;
        private SupportData _testEntity;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<ISupportDataRepository>();
            _mockMapper = new Mock<IMapper>();

            _testDto = new SupportDataDto
            {
                DataType = "InsuranceClaim",
                //ID = 5,
                FileName = "claim.pdf",
                FileContent = new byte[] { 1, 2, 3 }
            };

            _testEntity = new SupportData
            {
                DataID = 101,
                DataType = "InsuranceClaim",
                DoctorID = 5,
                FileName = "claim.pdf",
                FileContent = new byte[] { 1, 2, 3 },
                Patient = new Patient { PatientId = _testPatientId } // Correct way to assign PatientID
            };

            _service = new SupportDataService(_mockRepository.Object, _mockMapper.Object);
        }

        [Test]
        public async Task UploadDocumentAsync_ValidDto_CallsMapperAndRepositoryCorrectly()
        {
            _mockMapper.Setup(m => m.Map<SupportData>(_testDto)).Returns(_testEntity);
            _mockRepository.Setup(r => r.AddAsync(_testEntity)).ReturnsAsync(_testEntity);
            _mockMapper.Setup(m => m.Map<SupportDataDto>(_testEntity)).Returns(_testDto);

            var result = await _service.UploadDocumentAsync(_testDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.FileName, Is.EqualTo(_testDto.FileName));

            _mockMapper.Verify(m => m.Map<SupportData>(_testDto), Times.Once);
            _mockRepository.Verify(r => r.AddAsync(_testEntity), Times.Once);
            _mockMapper.Verify(m => m.Map<SupportDataDto>(_testEntity), Times.Once);
        }

        [Test]
        public async Task GetDocumentsAsync_DocumentsExist_CallsRepositoryAndReturnsMappedDtos()
        {
            var entityList = new List<SupportData> { _testEntity };
            var dtoList = new List<SupportDataDto> { _testDto };

            _mockRepository.Setup(r => r.GetByPatientIdAsync(_testPatientId)).ReturnsAsync(entityList);
            _mockMapper.Setup(m => m.Map<IEnumerable<SupportDataDto>>(entityList)).Returns(dtoList);

            var result = await _service.GetDocumentsAsync(_testPatientId);

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().FileName, Is.EqualTo(_testDto.FileName));

            _mockRepository.Verify(r => r.GetByPatientIdAsync(_testPatientId), Times.Once);
            _mockMapper.Verify(m => m.Map<IEnumerable<SupportDataDto>>(entityList), Times.Once);
        }

        [Test]
        public async Task GetDocumentsAsync_NoDocumentsFound_ReturnsEmptyEnumerable()
        {
            var emptyEntityList = new List<SupportData>();
            var emptyDtoList = Enumerable.Empty<SupportDataDto>();

            _mockRepository.Setup(r => r.GetByPatientIdAsync(_testPatientId)).ReturnsAsync(emptyEntityList);
            _mockMapper.Setup(m => m.Map<IEnumerable<SupportDataDto>>(emptyEntityList)).Returns(emptyDtoList);

            var result = await _service.GetDocumentsAsync(_testPatientId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);

            _mockRepository.Verify(r => r.GetByPatientIdAsync(_testPatientId), Times.Once);
        }
    }
}