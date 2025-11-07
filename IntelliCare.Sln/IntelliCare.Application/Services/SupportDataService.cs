using AutoMapper;
using IntelliCare.Application.DTOs;
using IntelliCare.Application.Interfaces;
using IntelliCare.Domain;

public interface ISupportDataService
{
    Task<SupportDataDto> UploadDocumentAsync(SupportDataDto dto);
    Task<IEnumerable<SupportDataDto>> GetDocumentsAsync(int patientId);
}

public class SupportDataService : ISupportDataService
{
    private readonly ISupportDataRepository _repository;
    private readonly IMapper _mapper;

    public SupportDataService(ISupportDataRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<SupportDataDto> UploadDocumentAsync(SupportDataDto dto)
    {
        var entity = _mapper.Map<SupportData>(dto);
        var result = await _repository.AddAsync(entity);
        return _mapper.Map<SupportDataDto>(result);
    }

    public async Task<IEnumerable<SupportDataDto>> GetDocumentsAsync(int patientId)
    {
        var docs = await _repository.GetByPatientIdAsync(patientId);
        return _mapper.Map<IEnumerable<SupportDataDto>>(docs);
    }
}
