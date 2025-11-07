using AutoMapper;
using IntelliCare.Application.DTOs;
using IntelliCare.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliCare.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Mapping 

            CreateMap<Patient, PatientDto>().ReverseMap();
            CreateMap<Patient, CreatePatientDto>().ReverseMap();
            CreateMap<SupportDataDto, SupportData>().ReverseMap();
            CreateMap<SupportData, SupportDataDto>().ReverseMap();



            CreateMap<SupportData, ReportSummaryDto>()
                // Map the primary key DataID from SupportData to ReportID in the DTO
                .ForMember(dest => dest.ReportID, opt => opt.MapFrom(src => src.DataID))

             
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))

                // Map the generated date
                .ForMember(dest => dest.GeneratedDate, opt => opt.MapFrom(src => src.GeneratedDate));

        }
    }
}