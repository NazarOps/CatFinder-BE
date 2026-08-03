using ApplicationLayer.CatReport.DTOs;
using AutoMapper;
using DomainLayer.Models;
using DomainLayer.Models.Enum;

namespace ApplicationLayer.Common.Mappings
{
    public class AdvertisementProfile : Profile
    {
        public AdvertisementProfile()
        {
            CreateMap<Advertisement, PublicAdvertisementResponseDto>()
                .ForMember(dest => dest.Cat, opt => opt.MapFrom(src => src.Cat))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location));

            // Advertisement -> Response DTO
            CreateMap<Advertisement, AdvertisementResponseDto>()
                .ForMember(dest => dest.Cat, opt => opt.MapFrom(src => src.Cat))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.Reports, opt => opt.MapFrom(src => src.Reports));

            // Create DTO -> Entity
            CreateMap<CreateAdvertisementDto, Advertisement>()
                .ForMember(dest => dest.AccountId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(_ => AdvertisementStatus.Active))
                .ForMember(dest => dest.ModerationStatus,
                    opt => opt.MapFrom(_ => ModerationStatus.Pending))
                .ForMember(dest => dest.IsVisible,
                    opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.Cat,
                    opt => opt.MapFrom(src => src.Cat))
                .ForMember(dest => dest.Location,
                    opt => opt.MapFrom(src => src.Location));

            // Update DTO -> Entity
            CreateMap<UpdateAdvertisementDto, Advertisement>()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

