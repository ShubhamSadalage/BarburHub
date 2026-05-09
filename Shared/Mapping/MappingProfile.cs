using AutoMapper;
using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Features.Products.Dtos;

namespace BarberHub.Web.Shared.Mapping;

/// <summary>
/// Sample AutoMapper profile. Feature-specific profiles can be added alongside their slices
/// (e.g. Features/Products/Mapping/ProductProfile.cs). All profiles are auto-registered in Program.cs.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductListItemDto>()
            .ForMember(dst => dst.EffectivePrice, opt => opt.MapFrom(src => src.EffectivePrice))
            .ForMember(dst => dst.BarberName,
                opt => opt.MapFrom(src =>
                    src.Barber != null ? (src.Barber.ShopName ?? src.Barber.FullName) : string.Empty));
    }
}
