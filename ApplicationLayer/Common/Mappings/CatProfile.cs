using ApplicationLayer.Cat.DTOs;
using AutoMapper;

namespace ApplicationLayer.Common.Mappings
{
    public class CatProfile : Profile
    {
        public CatProfile()
        {
            CreateMap<DomainLayer.Models.Cat, PublicCatResponseDto>();
            CreateMap<DomainLayer.Models.Cat, CatResponseDto>();

            CreateMap<CreateCatDto, DomainLayer.Models.Cat>();

            CreateMap<UpdateCatDto, DomainLayer.Models.Cat>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
